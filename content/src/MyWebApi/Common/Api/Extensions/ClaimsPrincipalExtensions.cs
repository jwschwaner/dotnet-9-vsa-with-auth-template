namespace System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("User ID not found in claims");
        }
        return userId;
    }

    public static string? GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetUserEmail(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.Email);
    }

    public static bool IsInRole(this ClaimsPrincipal claimsPrincipal, string role)
    {
        return claimsPrincipal.IsInRole(role);
    }

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}