using Application.Api.Features.Auth;
using Application.Api.Features.Cart;
using Application.Api.Features.Content;
using Application.Api.Features.Orders;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Common.Data;

/// <summary>
/// Single shared EF Core context - deliberately in Common/, not inside any
/// one feature folder, since it's genuinely cross-feature infrastructure
/// (Products, Orders, Cart, Auth, Users entities all live behind it).
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Each feature's IEntityTypeConfiguration<T> lives next to its
        // entity, not here - this just discovers and applies all of them.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
