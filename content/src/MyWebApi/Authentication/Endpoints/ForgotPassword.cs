using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using MyWebApi.Authentication.Services;

namespace MyWebApi.Authentication.Endpoints;

public class ForgotPassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/forgot-password", Handle)
        .WithSummary("Request password reset")
        .WithRequestValidation<Request>();

    public record Request(string Email);
    public record Response(string Message);
    
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    private static async Task<Ok<Response>> Handle(
        Request request,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        
        // Always return success to prevent email enumeration
        if (user == null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return TypedResults.Ok(new Response("If an account with that email exists, a password reset link has been sent."));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{context.Request.Scheme}://{context.Request.Host}/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        await emailService.SendPasswordResetAsync(user.Email!, resetLink);

        return TypedResults.Ok(new Response("If an account with that email exists, a password reset link has been sent."));
    }
}