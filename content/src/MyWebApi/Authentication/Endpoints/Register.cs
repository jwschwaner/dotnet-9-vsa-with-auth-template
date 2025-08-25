using Microsoft.AspNetCore.Identity;
using MyWebApi.Authentication.Models;
using MyWebApi.Authentication.Services;

namespace MyWebApi.Authentication.Endpoints;

public class Register : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/register", Handle)
        .WithSummary("Register a new user account")
        .WithRequestValidation<Request>();

    public record Request(string Email, string Password, string ConfirmPassword, string? FirstName, string? LastName);
    public record Response(string Message);
    
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Password and confirmation password do not match.");
        }
    }

    private static async Task<Results<Ok<Response>, ValidationError>> Handle(
        Request request,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new ValidationError("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Split('@')[0],
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new ValidationError($"Registration failed: {errors}");
        }

        // Add user to default role
        await userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        
        await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

        return TypedResults.Ok(new Response("Registration successful. Please check your email to confirm your account."));
    }
}