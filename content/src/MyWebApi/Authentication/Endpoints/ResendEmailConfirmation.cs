using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using MyWebApi.Authentication.Services;

namespace MyWebApi.Authentication.Endpoints;

public class ResendEmailConfirmation : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/resend-email-confirmation", Handle)
        .WithSummary("Resend email confirmation")
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
        if (user == null || await userManager.IsEmailConfirmedAsync(user))
        {
            return TypedResults.Ok(new Response("If an unconfirmed account with that email exists, a confirmation email has been sent."));
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

        return TypedResults.Ok(new Response("If an unconfirmed account with that email exists, a confirmation email has been sent."));
    }
}