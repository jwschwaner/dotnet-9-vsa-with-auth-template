using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using System.Text.RegularExpressions;

namespace MyWebApi.Authentication.Endpoints;

public class Verify2fa : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/verify-2fa", Handle)
        .WithSummary("Verify and enable two-factor authentication")
        .WithRequestValidation<Request>();

    public record Request(string Code);
    public record Response(string Message, bool IsEnabled);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .Must(BeValidCode)
                .WithMessage("Code must be 6 digits");
        }

        private static bool BeValidCode(string code)
        {
            return Regex.IsMatch(code.Replace(" ", "").Replace("-", ""), @"^\d{6}$");
        }
    }

    private static async Task<Results<Ok<Response>, UnauthorizedHttpResult, ValidationError>> Handle(
        Request request,
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

        // Remove spaces and dashes from the code
        var code = request.Code.Replace(" ", "").Replace("-", "");

        // Verify the code
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            currentUser, 
            userManager.Options.Tokens.AuthenticatorTokenProvider, 
            code);

        if (!isValid)
        {
            return new ValidationError("Invalid verification code. Please try again.");
        }

        // Enable 2FA for the user
        await userManager.SetTwoFactorEnabledAsync(currentUser, true);

        return TypedResults.Ok(new Response(
            "Two-factor authentication has been enabled successfully.", 
            true));
    }
}