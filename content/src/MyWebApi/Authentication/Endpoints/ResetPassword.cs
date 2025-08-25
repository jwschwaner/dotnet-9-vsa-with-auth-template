using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class ResetPassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/reset-password", Handle)
        .WithSummary("Reset user password")
        .WithRequestValidation<Request>();

    public record Request(string Email, string Token, string Password, string ConfirmPassword);
    public record Response(string Message);
    
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Password and confirmation password do not match.");
        }
    }

    private static async Task<Results<Ok<Response>, ValidationError>> Handle(
        Request request,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new ValidationError("Invalid request.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new ValidationError($"Password reset failed: {errors}");
        }

        return TypedResults.Ok(new Response("Password reset successful. You can now log in with your new password."));
    }
}