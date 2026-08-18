using System.Security.Claims;

namespace Application.Api.Common.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The Sub claim gets inbound-mapped to ClaimTypes.NameIdentifier by the
    /// default JwtBearer handler. Every feature that scopes data to "the
    /// current user" (Cart, Orders) reads it through here rather than
    /// re-parsing claims inline in each controller.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Used for privilege-escalation checks - e.g. a Sub Admin must not be able to create or edit another Admin/Sub Admin.</summary>
    public static IReadOnlyCollection<string> GetRoles(this ClaimsPrincipal user) =>
        user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
}
