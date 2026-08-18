using Application.Api.Common.Data;
using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Products;

namespace Application.Api.Tests;

[TestFixture]
public class ProductServiceTests
{
    private AppDbContext _context = null!;
    private ProductService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
        _sut = new ProductService(new UnitOfWork(_context));

        _context.Categories.Add(new Category { Id = 100, Name = "Test Category", Slug = "test-category" });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task CreateAsync_ReturnsValidationError_WhenCategoryDoesNotExist()
    {
        // Arrange
        var dto = new CreateProductDto("Widget", "desc", 10m, 5, null, 999);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Validation));
    }

    [Test]
    public async Task CreateAsync_PersistsProduct_WhenCategoryExists()
    {
        // Arrange
        var dto = new CreateProductDto("Widget", "desc", 10m, 5, null, 100);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Widget"));
        Assert.That(_context.Products.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_FiltersBySearchTerm()
    {
        // Arrange
        await _sut.CreateAsync(new CreateProductDto("Blue Widget", "d", 10m, 5, null, 100));
        await _sut.CreateAsync(new CreateProductDto("Red Gadget", "d", 10m, 5, null, 100));

        // Act
        var result = await _sut.GetAllAsync("Widget");

        // Assert
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Name, Is.EqualTo("Blue Widget"));
    }

    [Test]
    public async Task GetAllAsync_ExcludesInactiveProducts()
    {
        // Arrange
        var created = await _sut.CreateAsync(new CreateProductDto("Widget", "d", 10m, 5, null, 100));
        await _sut.DeleteAsync(created.Value.Id); // soft delete -> IsActive = false

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task DeleteAsync_SoftDeletes_RowStillExistsInDatabase()
    {
        // Arrange
        var created = await _sut.CreateAsync(new CreateProductDto("Widget", "d", 10m, 5, null, 100));

        // Act
        await _sut.DeleteAsync(created.Value.Id);

        // Assert - row must survive, not be removed, since past order line
        // items reference the product by id.
        var stillInDb = await _context.Products.FindAsync(created.Value.Id);
        Assert.That(stillInDb, Is.Not.Null);
        Assert.That(stillInDb!.IsActive, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Act
        var result = await _sut.DeleteAsync(9999);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.NotFound));
    }
}
