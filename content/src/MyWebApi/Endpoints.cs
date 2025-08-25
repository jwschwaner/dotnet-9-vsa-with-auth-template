using MyWebApi.Common.Api.Filters;
using MyWebApi.Authentication.Endpoints;

namespace MyWebApi;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("")
            .AddEndpointFilter<RequestLoggingFilter>()
            .WithOpenApi();

        endpoints.MapAuthenticationEndpoints();

        // Add your feature endpoints here
        // endpoints.MapFeatureEndpoints();
    }

    private static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/auth")
            .WithTags("Authentication");
            
        endpoints.MapPublicGroup()
            .MapEndpoint<Register>()
            .MapEndpoint<Login>()
            .MapEndpoint<Login2fa>()
            .MapEndpoint<ForgotPassword>()
            .MapEndpoint<ResetPassword>()
            .MapEndpoint<ConfirmEmail>()
            .MapEndpoint<ResendEmailConfirmation>();

        endpoints.MapAuthorizedGroup()
            .MapEndpoint<Logout>()
            .MapEndpoint<ChangePassword>()
            .MapEndpoint<GetUserInfo>()
            .MapEndpoint<Enable2fa>()
            .MapEndpoint<Disable2fa>()
            .MapEndpoint<Verify2fa>()
            .MapEndpoint<GetRecoveryCodes>();
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    private static RouteGroupBuilder MapAuthorizedGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}