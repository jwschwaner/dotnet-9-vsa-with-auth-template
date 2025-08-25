using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class GetRecoveryCodes : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/recovery-codes", Handle)
        .WithSummary("Generate new recovery codes for 2FA");

    public record Response(string[] RecoveryCodes, string Message);

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
            return new ValidationError("Two-factor authentication must be enabled to generate recovery codes.");
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);
        
        return TypedResults.Ok(new Response(
            recoveryCodes?.ToArray() ?? Array.Empty<string>(),
            "New recovery codes have been generated. Store them securely as they won't be shown again."
        ));
    }
}