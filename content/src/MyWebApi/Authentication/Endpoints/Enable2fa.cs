using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using System.Text;

namespace MyWebApi.Authentication.Endpoints;

public class Enable2fa : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/enable-2fa", Handle)
        .WithSummary("Enable two-factor authentication for current user");

    public record Response(string SharedKey, string QrCodeUri, string[] RecoveryCodes);

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

        if (await userManager.GetTwoFactorEnabledAsync(currentUser))
        {
            return new ValidationError("Two-factor authentication is already enabled.");
        }

        // Generate the shared key for the authenticator app
        var key = await userManager.GetAuthenticatorKeyAsync(currentUser);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(currentUser);
            key = await userManager.GetAuthenticatorKeyAsync(currentUser);
        }

        // Generate QR code URI for authenticator apps
        var qrCodeUri = GenerateQrCodeUri(currentUser.Email!, key!);

        // Generate recovery codes
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);

        var response = new Response(
            FormatKey(key!),
            qrCodeUri,
            recoveryCodes?.ToArray() ?? Array.Empty<string>()
        );

        return TypedResults.Ok(response);
    }

    private static string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        var appName = "MyWebApi"; // You can make this configurable
        return string.Format(authenticatorUriFormat, Uri.EscapeDataString(appName), Uri.EscapeDataString(email), unformattedKey);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }
}