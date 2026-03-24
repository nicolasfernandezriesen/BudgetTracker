using brevo_csharp.Api;
using brevo_csharp.Model;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace BudgetTracker.Services.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly TransactionalEmailsApi _apiInstance;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;

        var configuration = new brevo_csharp.Client.Configuration();
        configuration.ApiKey["api-key"] = _emailSettings.ApiKey;

        _apiInstance = new TransactionalEmailsApi(configuration);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        SendSmtpEmailSender sender = new SendSmtpEmailSender(_emailSettings.SenderName, _emailSettings.SenderEmail);

        SendSmtpEmailTo receiver = new SendSmtpEmailTo(email);
        List<SendSmtpEmailTo> toList = new List<SendSmtpEmailTo> { receiver };

        try
        {
            var sendSmtpEmail = new SendSmtpEmail(
                sender: sender,
                to: toList,
                htmlContent: htmlMessage,
                subject: subject
            );

            await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error to send the error with Brevo: " + ex.Message);
            throw;
        }
    }
}