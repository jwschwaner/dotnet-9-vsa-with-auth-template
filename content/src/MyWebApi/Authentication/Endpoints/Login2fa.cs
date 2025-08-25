using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using System.Text.RegularExpressions;

namespace MyWebApi.Authentication.Endpoints;

public class Login2fa : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/login-2fa", Handle)
        .WithSummary("Complete login with two-factor authentication code")
        .WithRequestValidation<Request>();

    public record Request(string Code, bool RememberMe = false, bool RememberMachine = false);
    public record Response(string Message);

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
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        // Get the user who is in the middle of 2FA login process
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return new ValidationError("Unable to load two-factor authentication user.");
        }

        // Remove spaces and dashes from the code
        var code = request.Code.Replace(" ", "").Replace("-", "");

        // Try authenticator code first
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            code, 
            request.RememberMe, 
            request.RememberMachine);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return TypedResults.Ok(new Response("Login successful."));
        }

        if (result.IsLockedOut)
        {
            return new ValidationError("Account locked due to multiple failed attempts.");
        }

        // If authenticator code failed, try recovery code
        var recoveryResult = await signInManager.TwoFactorRecoveryCodeSignInAsync(code);
        if (recoveryResult.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return TypedResults.Ok(new Response("Login successful using recovery code."));
        }

        return new ValidationError("Invalid authentication code. Please try again.");
    }
}