using Azure;
using Azure.Communication.Email;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FutureMeFunction;

public class SendScheduledEmails
{
    //Variables
    private readonly ILogger _logger;

    private readonly TableClient _tableClient;
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;

    //Constructor
    public SendScheduledEmails(ILoggerFactory loggerFactory, TableClient tableClient, EmailClient emailClient)
    {
        _logger = loggerFactory.CreateLogger<SendScheduledEmails>();

        _tableClient = tableClient;
        _emailClient = emailClient;
        _senderAddress = Environment.GetEnvironmentVariable("AcsEmailSender")
            ?? throw new InvalidOperationException("AcsEmailSender is not configured.");
    }

    //Function
    [Function("SendScheduledEmails")]
    public async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
    {
        //Loop through entities in table
        DateTimeOffset now = DateTimeOffset.UtcNow;

        string todayPartitionKey = now.ToString("yyyyMMdd");
        string yesterdayPartitionKey = now.AddDays(-1).ToString("yyyyMMdd");

        var dueEmails = _tableClient.Query<ScheduledEmailEntity>(
            e =>
                (e.PartitionKey == todayPartitionKey || e.PartitionKey == yesterdayPartitionKey) &&
                 e.Status == "Pending" &&
                 e.ScheduledForUtc <= DateTimeOffset.UtcNow);

        foreach (ScheduledEmailEntity scheduledEmailEntity in dueEmails)
        {
            //Try
            try
            {
                //Skip if recipient email is empty
                if (string.IsNullOrWhiteSpace(scheduledEmailEntity.RecipientEmail))
                {
                    scheduledEmailEntity.Status = "Failed";
                    scheduledEmailEntity.LastError = "RecipientEmail is missing.";

                    _tableClient.UpdateEntity(scheduledEmailEntity, scheduledEmailEntity.ETag, TableUpdateMode.Replace);

                    continue;
                }

                //Make sure subject and entity are valid
                if (string.IsNullOrWhiteSpace(scheduledEmailEntity.Subject)) scheduledEmailEntity.Subject = "A mail from your past self!";
                if (string.IsNullOrWhiteSpace(scheduledEmailEntity.Body)) scheduledEmailEntity.Body = "This message had no content.";

                //Throw error is scheduled time is invalid
                if (scheduledEmailEntity.ScheduledForUtc == default)
                    throw new InvalidOperationException("ScheduledForUtc is missing or invalid.");

                //Compose email content
                EmailContent emailContent = new EmailContent(scheduledEmailEntity.Subject);
                emailContent.PlainText = scheduledEmailEntity.Body;

                //Compose email message
                EmailMessage emailMessage = new EmailMessage(
                    _senderAddress,
                    scheduledEmailEntity.RecipientEmail,
                    emailContent);

                //Send email
                EmailSendOperation emailOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

                //Update entity
                scheduledEmailEntity.Status = "Sent";
                scheduledEmailEntity.LastError = null;

                _tableClient.UpdateEntity(scheduledEmailEntity, scheduledEmailEntity.ETag, TableUpdateMode.Merge);

                //Log success
                _logger.LogInformation($"Mail successfully sent for entity {scheduledEmailEntity.PartitionKey}/{scheduledEmailEntity.RowKey} to {scheduledEmailEntity.RecipientEmail}.");
            }

            //Log exception
            catch (Exception ex)
            {
                //Log full exception
                _logger.LogError(ex, $"Failed to send mail for entity {scheduledEmailEntity.PartitionKey}/{scheduledEmailEntity.RowKey}.");

                //Try
                try
                {
                    //Update entity
                    scheduledEmailEntity.Status = "Failed";
                    scheduledEmailEntity.LastError = ex.ToString();

                    _tableClient.UpdateEntity(scheduledEmailEntity, scheduledEmailEntity.ETag, TableUpdateMode.Merge);
                }

                //Log exception
                catch (Exception updateException)
                {
                    _logger.LogError(
                        updateException,
                        $"Failed to update failed status for entity {scheduledEmailEntity.PartitionKey}/{scheduledEmailEntity.RowKey}.");
                }
            }
        }
    }
}