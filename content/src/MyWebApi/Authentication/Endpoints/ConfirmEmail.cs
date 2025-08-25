using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class ConfirmEmail : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/confirm-email", Handle)
        .WithSummary("Confirm user email address")
        .WithRequestValidation<Request>();

    public record Request(string UserId, string Token);
    public record Response(string Message);
    
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    private static async Task<Results<Ok<Response>, ValidationError>> Handle(
        [AsParameters] Request request,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ValidationError("Invalid user.");
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new ValidationError($"Email confirmation failed: {errors}");
        }

        return TypedResults.Ok(new Response("Email confirmed successfully. You can now log in."));
    }
}