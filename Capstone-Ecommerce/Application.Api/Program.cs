using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Application.Api.Common.Data;
using Application.Api.Common.Repository;
using Application.Api.Features.Auth;
using Application.Api.Features.Auth.Revocation;
using Application.Api.Features.Cart;
using Application.Api.Features.Content;
using Application.Api.Features.Dashboard;
using Application.Api.Features.Orders;
using Application.Api.Features.Orders.Payment;
using Application.Api.Features.Products;
using Application.Api.Features.Reports;
using Application.Api.Features.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Polly;

var builder = WebApplication.CreateBuilder(args);

const string AngularClientCors = "AngularClient";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds the "Authorize" button in Swagger UI so protected endpoints
    // (Admin-only product mutations, /auth/me) can be tested manually with
    // a bearer token, not just the public GET endpoints.
    var scheme = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Paste the access token from /api/auth/login - no 'Bearer ' prefix needed.",
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddSingleton<IPaymentGateway, SimulatedPaymentGateway>();
builder.Services.AddResiliencePipeline<string, PaymentChargeResult>(PaymentResilience.PipelineKey, PaymentResilience.Configure);
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHostedService<PaymentRetryWorker>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Each rule registered separately - RevocationRuleEngine resolves them all
// via IEnumerable<IRevocationRule>, so adding a 4th rule later never
// requires touching this list's consumers, only adding one more line here.
builder.Services.AddScoped<IRevocationRule, PasswordChangedRule>();
builder.Services.AddScoped<IRevocationRule, RoleChangedRule>();
builder.Services.AddScoped<IRevocationRule, ManualAdminRevokeRule>();
builder.Services.AddScoped<RevocationRuleEngine>();

var corsOrigin = builder.Configuration["Cors:AllowedOrigin"];

Console.WriteLine($"CORS ORIGIN: {corsOrigin}");

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularClientCors, policy =>
        policy
            .WithOrigins(corsOrigin!)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Runs on EVERY authenticated request, not just login/refresh - this
        // is what makes revocation instant instead of "wait up to 15 minutes
        // for the access token to naturally expire." A stateless JWT alone
        // can't do this; this hook is what makes it stateful again exactly
        // where it matters.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var iatClaim = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Iat);

                if (userIdClaim is null || iatClaim is null)
                {
                    context.Fail("Token is missing required claims.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var engine = context.HttpContext.RequestServices.GetRequiredService<RevocationRuleEngine>();

                var user = await db.Users.FindAsync(int.Parse(userIdClaim));
                if (user is null)
                {
                    context.Fail("User no longer exists.");
                    return;
                }

                var tokenIssuedAtUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatClaim)).UtcDateTime;

                if (engine.IsRevoked(user, tokenIssuedAtUtc))
                {
                    context.Fail("Session has been revoked.");
                }
            },
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("Customer", policy => policy.RequireRole("Customer"));

// Keyed per client IP, not globally - one abusive caller shouldn't be able
// to lock every other user out of login/register/refresh.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

var app = builder.Build();

// Apply pending migrations, then seed the Admin user + product catalog -
// runs once at startup so the DB is always ready before the app accepts traffic.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAdminUserAsync(db);
    await DbSeeder.SeedProductsAsync(db);
}

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(AngularClientCors);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the implicit top-level-statement Program class to the test
// project's WebApplicationFactory<Program> - it's internal by default.
public partial class Program;





//test purpose for azure
