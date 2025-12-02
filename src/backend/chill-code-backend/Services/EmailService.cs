public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        // Log per test
        Console.WriteLine($"Sending email to {to}: {subject}");
        return Task.CompletedTask;

        // Oppure implementa SMTP
    }
}