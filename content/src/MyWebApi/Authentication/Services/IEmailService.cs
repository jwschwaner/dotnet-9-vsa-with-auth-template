namespace MyWebApi.Authentication.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
    Task Send2faCodeAsync(string email, string code);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Email confirmation link for {Email}: {Link}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetLink)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Password reset link for {Email}: {Link}", email, resetLink);
        return Task.CompletedTask;
    }

    public Task Send2faCodeAsync(string email, string code)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("2FA code for {Email}: {Code}", email, code);
        return Task.CompletedTask;
    }
}