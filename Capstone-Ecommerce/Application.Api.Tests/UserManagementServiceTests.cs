using Application.Api.Common.Data;
using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Application.Api.Features.Users;

namespace Application.Api.Tests;

/// <summary>
/// Covers the privilege-escalation guards added when the Sub Admin/Supervisor/
/// Support Agent role hierarchy was introduced - a Sub Admin must never be
/// able to create, edit, or touch sessions for an Admin/Sub Admin account,
/// and Customer must never be assignable from the admin Create User panel.
/// </summary>
[TestFixture]
public class UserManagementServiceTests
{
    private AppDbContext _context = null!;
    private UserManagementService _sut = null!;

    private static readonly string[] AdminCaller = [RoleNames.Admin];
    private static readonly string[] SubAdminCaller = [RoleNames.SubAdmin];

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
        _sut = new UserManagementService(new UnitOfWork(_context));
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private CreateUserDto ValidCreateDto(List<string> roles) =>
        new("New Staffer", $"staffer{Guid.NewGuid():N}@test.com", "9800011122", "Sample@123", roles);

    [Test]
    public async Task CreateAsync_RejectsCustomerRole_RegardlessOfCaller()
    {
        // Act
        var result = await _sut.CreateAsync(ValidCreateDto([RoleNames.Customer]), AdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.CustomerNotAllowed"));
    }

    [Test]
    public async Task CreateAsync_AllowsAdminToCreateAnotherAdmin()
    {
        // Act
        var result = await _sut.CreateAsync(ValidCreateDto([RoleNames.Admin]), AdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task CreateAsync_RejectsSubAdminCreatingAnAdmin()
    {
        // Act
        var result = await _sut.CreateAsync(ValidCreateDto([RoleNames.Admin]), SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.PrivilegeEscalation"));
    }

    [Test]
    public async Task CreateAsync_RejectsSubAdminCreatingAnotherSubAdmin()
    {
        // Act
        var result = await _sut.CreateAsync(ValidCreateDto([RoleNames.SubAdmin]), SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.PrivilegeEscalation"));
    }

    [Test]
    public async Task CreateAsync_AllowsSubAdminCreatingASupervisor()
    {
        // Act
        var result = await _sut.CreateAsync(ValidCreateDto([RoleNames.Supervisor]), SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task UpdateRolesAsync_RejectsSubAdminEditingAnExistingAdmin()
    {
        // Arrange - seed a real Admin-role user to edit.
        var created = await _sut.CreateAsync(ValidCreateDto([RoleNames.Admin]), AdminCaller);

        // Act
        var result = await _sut.UpdateRolesAsync(created.Value.Id, new UpdateUserRolesDto([RoleNames.Supervisor]), SubAdminCaller);

        // Assert - blocked even though the NEW role set (Supervisor) is one a Sub Admin could otherwise grant.
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.PrivilegeEscalation"));
    }

    [Test]
    public async Task UpdateRolesAsync_RejectsSubAdminGrantingAdminToAnOrdinaryUser()
    {
        // Arrange - a plain Supervisor account, not privileged.
        var created = await _sut.CreateAsync(ValidCreateDto([RoleNames.Supervisor]), AdminCaller);

        // Act
        var result = await _sut.UpdateRolesAsync(created.Value.Id, new UpdateUserRolesDto([RoleNames.Admin]), SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.PrivilegeEscalation"));
    }

    [Test]
    public async Task UpdateRolesAsync_AllowsSubAdminEditingAnOrdinaryStaffAccount()
    {
        // Arrange
        var created = await _sut.CreateAsync(ValidCreateDto([RoleNames.Supervisor]), AdminCaller);

        // Act
        var result = await _sut.UpdateRolesAsync(created.Value.Id, new UpdateUserRolesDto([RoleNames.SupportAgent]), SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Roles, Is.EquivalentTo(new[] { RoleNames.SupportAgent }));
    }

    [Test]
    public async Task RevokeSessionsAsync_RejectsSubAdminRevokingAnAdminAccount()
    {
        // Arrange
        var created = await _sut.CreateAsync(ValidCreateDto([RoleNames.Admin]), AdminCaller);

        // Act
        var result = await _sut.RevokeSessionsAsync(created.Value.Id, SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Code, Is.EqualTo("Users.PrivilegeEscalation"));
    }

    [Test]
    public async Task RevokeSessionsAsync_AllowsSubAdminRevokingAnOrdinaryStaffAccount()
    {
        // Arrange
        var created = await _sut.CreateAsync(ValidCreateDto([RoleNames.SupportAgent]), AdminCaller);

        // Act
        var result = await _sut.RevokeSessionsAsync(created.Value.Id, SubAdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task CreateAsync_ReturnsConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = ValidCreateDto([RoleNames.Supervisor]);
        await _sut.CreateAsync(dto, AdminCaller);

        // Act - same email again
        var result = await _sut.CreateAsync(dto, AdminCaller);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
    }
}
