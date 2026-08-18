using Application.Api.Common.Data;
using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Content;

namespace Application.Api.Tests;

[TestFixture]
public class ContentServiceTests
{
    private AppDbContext _context = null!;
    private ContentService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
        _sut = new ContentService(new UnitOfWork(_context));
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private static CreateContentBlockDto ValidDto(bool isActive = true, int order = 0) =>
        new($"key-{Guid.NewGuid():N}", "Big Sale!", "Up to 50% off this week.", null, isActive, order);

    [Test]
    public async Task CreateAsync_ReturnsConflict_WhenKeyAlreadyExists()
    {
        // Arrange
        var dto = ValidDto();
        await _sut.CreateAsync(dto);
        var duplicate = dto with { Title = "Different Title" }; // same Key

        // Act
        var result = await _sut.CreateAsync(duplicate);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
    }

    [Test]
    public async Task GetActiveAsync_ExcludesInactiveBlocks()
    {
        // Arrange
        await _sut.CreateAsync(ValidDto(isActive: true, order: 1));
        await _sut.CreateAsync(ValidDto(isActive: false, order: 2));

        // Act
        var result = await _sut.GetActiveAsync();

        // Assert
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].IsActive, Is.True);
    }

    [Test]
    public async Task GetActiveAsync_OrdersByDisplayOrder()
    {
        // Arrange
        await _sut.CreateAsync(ValidDto(order: 2) with { Title = "Second" });
        await _sut.CreateAsync(ValidDto(order: 1) with { Title = "First" });

        // Act
        var result = await _sut.GetActiveAsync();

        // Assert
        Assert.That(result.Value.Select(b => b.Title), Is.EqualTo(new[] { "First", "Second" }));
    }

    [Test]
    public async Task GetAllAsync_IncludesInactiveBlocks()
    {
        // Arrange
        await _sut.CreateAsync(ValidDto(isActive: false));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.That(result.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ChangesFieldsAndStampsUpdatedAtUtc()
    {
        // Arrange
        var created = await _sut.CreateAsync(ValidDto());
        var updateDto = new UpdateContentBlockDto("New Title", "New body", null, false, 5);

        // Act
        var result = await _sut.UpdateAsync(created.Value.Id, updateDto);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Title, Is.EqualTo("New Title"));
        Assert.That(result.Value.IsActive, Is.False);
        Assert.That(result.Value.UpdatedAtUtc, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_HardDeletes_RowNoLongerExists()
    {
        // Arrange
        var created = await _sut.CreateAsync(ValidDto());

        // Act
        await _sut.DeleteAsync(created.Value.Id);

        // Assert
        var stillInDb = await _context.ContentBlocks.FindAsync(created.Value.Id);
        Assert.That(stillInDb, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ReturnsNotFound_WhenBlockDoesNotExist()
    {
        // Act
        var result = await _sut.DeleteAsync(9999);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.NotFound));
    }
}
