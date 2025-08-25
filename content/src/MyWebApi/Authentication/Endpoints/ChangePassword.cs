using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class ChangePassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/change-password", Handle)
        .WithSummary("Change user password")
        .WithRequestValidation<Request>();

    public record Request(string CurrentPassword, string NewPassword, string ConfirmPassword);
    public record Response(string Message);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("New password and confirmation password do not match.");
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

        var result = await userManager.ChangePasswordAsync(currentUser, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new ValidationError($"Password change failed: {errors}");
        }

        return TypedResults.Ok(new Response("Password changed successfully."));
    }
}