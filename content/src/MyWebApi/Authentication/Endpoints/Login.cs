using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class Login : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/login", Handle)
        .WithSummary("Login with email and password")
        .WithRequestValidation<Request>();

    public record Request(string Email, string Password, bool RememberMe = false);
    public record Response(string Message, bool RequiresTwoFactor = false);
    
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    private static async Task<Results<Ok<Response>, UnauthorizedHttpResult, ValidationError>> Handle(
        Request request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return TypedResults.Unauthorized();
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return new ValidationError("Email not confirmed. Please check your email and confirm your account.");
        }

        var result = await signInManager.PasswordSignInAsync(
            user, 
            request.Password, 
            request.RememberMe, 
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return TypedResults.Ok(new Response("Login successful."));
        }

        if (result.RequiresTwoFactor)
        {
            return TypedResults.Ok(new Response("Two-factor authentication required. Use /auth/login-2fa endpoint.", true));
        }

        if (result.IsLockedOut)
        {
            return new ValidationError("Account locked due to multiple failed login attempts. Please try again later.");
        }

        if (result.IsNotAllowed)
        {
            return new ValidationError("Login not allowed. Please confirm your email first.");
        }

        return TypedResults.Unauthorized();
    }
}