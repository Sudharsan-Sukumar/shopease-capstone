using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Application.Api.Tests;

/// <summary>
/// Black-box HTTP tests through the real ASP.NET Core pipeline (routing,
/// auth, the actual configured SQL Server database) via WebApplicationFactory -
/// no mocks, matching the A10 demo's integration-test pattern. Requires the
/// same local SQL Server instance the app itself uses to be running.
/// </summary>
[TestFixture]
public class ApiIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetProducts_ReturnsOk_ForAnonymousCaller()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetProductById_ReturnsNotFound_ForNonexistentProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetActiveContent_ReturnsOk_ForAnonymousCaller()
    {
        // Act - public placement endpoint, no auth required.
        var response = await _client.GetAsync("/api/content");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Login_ReturnsUnauthorized_ForWrongPassword()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "admin@shopease.com", password = "definitely-wrong" });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetDashboardSummary_ReturnsUnauthorized_WithoutAToken()
    {
        // Act - staff-only endpoint, hit with no Authorization header at all.
        var response = await _client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateProduct_ReturnsUnauthorized_WithoutAToken()
    {
        // Act - Admin/Sub Admin-only mutation, no token at all.
        var response = await _client.PostAsJsonAsync("/api/products", new { name = "x", description = "x", price = 1, stock = 1, categoryId = 1 });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
