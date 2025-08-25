using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class Disable2fa : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/disable-2fa", Handle)
        .WithSummary("Disable two-factor authentication");

    public record Response(string Message, bool IsEnabled);

    private static async Task<Results<Ok<Response>, UnauthorizedHttpResult, ValidationError>> Handle(
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

        if (!await userManager.GetTwoFactorEnabledAsync(currentUser))
        {
            return new ValidationError("Two-factor authentication is not enabled.");
        }

        // Disable 2FA
        var disableResult = await userManager.SetTwoFactorEnabledAsync(currentUser, false);
        if (!disableResult.Succeeded)
        {
            return new ValidationError("Failed to disable two-factor authentication.");
        }

        // Reset the authenticator key
        await userManager.ResetAuthenticatorKeyAsync(currentUser);

        return TypedResults.Ok(new Response(
            "Two-factor authentication has been disabled.", 
            false));
    }
}