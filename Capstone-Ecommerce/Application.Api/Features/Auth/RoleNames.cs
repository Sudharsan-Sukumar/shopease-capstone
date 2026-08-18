namespace Application.Api.Features.Auth;

/// <summary>
/// Central place for role name strings so [Authorize(Roles=...)] attributes
/// and the privilege-escalation checks in UserManagementService can't drift
/// out of sync with each other.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string SubAdmin = "SubAdmin";
    public const string Supervisor = "Supervisor";
    public const string SupportAgent = "SupportAgent";
    public const string Customer = "Customer";

    /// <summary>Only these two can create staff accounts or edit anyone's roles.</summary>
    public const string UserManagers = $"{Admin},{SubAdmin}";

    /// <summary>Everyone who can see the user list / revoke a session, even without edit rights.</summary>
    public const string StaffRoles = $"{Admin},{SubAdmin},{Supervisor},{SupportAgent}";

    /// <summary>Product catalog CRUD - Sub Admin has near-full admin access here.</summary>
    public const string ProductManagers = $"{Admin},{SubAdmin}";

    /// <summary>Content block (banner/promo) CRUD - same tier as product management.</summary>
    public const string ContentManagers = $"{Admin},{SubAdmin}";

    /// <summary>Roles a Sub Admin is NOT allowed to grant or edit - prevents self- or peer-escalation.</summary>
    public static readonly string[] Privileged = [Admin, SubAdmin];
}
