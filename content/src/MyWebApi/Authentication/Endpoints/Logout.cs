using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;

namespace MyWebApi.Authentication.Endpoints;

public class Logout : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/logout", Handle)
        .WithSummary("Logout current user");

    public record Response(string Message);

    private static async Task<Ok<Response>> Handle(
        SignInManager<ApplicationUser> signInManager,
        CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();
        return TypedResults.Ok(new Response("Logout successful."));
    }
}