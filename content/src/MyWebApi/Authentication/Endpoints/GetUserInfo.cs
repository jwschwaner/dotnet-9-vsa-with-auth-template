using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class GetUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/user-info", Handle)
        .WithSummary("Get current user information");

    public record Response(
        string Id,
        string? UserName,
        string? Email,
        string? FirstName,
        string? LastName,
        bool EmailConfirmed,
        bool TwoFactorEnabled,
        IList<string> Roles
    );

    private static async Task<Results<Ok<Response>, UnauthorizedHttpResult>> Handle(
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!user.Identity?.IsAuthenticated == true)
        {
            return TypedResults.Unauthorized();
        }

        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return TypedResults.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(currentUser);

        var response = new Response(
            currentUser.Id,
            currentUser.UserName,
            currentUser.Email,
            currentUser.FirstName,
            currentUser.LastName,
            currentUser.EmailConfirmed,
            currentUser.TwoFactorEnabled,
            roles
        );

        return TypedResults.Ok(response);
    }
}