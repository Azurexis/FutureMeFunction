using Azure.Communication.Email;
using Azure.Core;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FutureMeFunction;

public class ScheduleEmail
{
    //Variables
    private readonly ILogger<ScheduleEmail> _logger;

    private readonly TableClient _tableClient;
    private readonly EmailClient _emailClient;

    //Constructor
    public ScheduleEmail(ILogger<ScheduleEmail> logger, TableClient tableClient, EmailClient emailClient)
    {
        _logger = logger;

        _tableClient = tableClient;
        _emailClient = emailClient;
    }

    //Function
    [Function("ScheduleEmail")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        //Try
        try
        {
            //Get schedule email request
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ScheduleEmailRequest? request = await JsonSerializer.DeserializeAsync<ScheduleEmailRequest>(req.Body, jsonSerializerOptions);
             
            //Validate request
            if (request is null)
                return new BadRequestObjectResult("Request body is missing or invalid.");

            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
                return new BadRequestObjectResult("RecipientEmail is required.");

            if (request.ScheduledForUtc == default)
                return new BadRequestObjectResult("ScheduledForUtc is missing or invalid.");

            if (request.ScheduledForUtc <= DateTimeOffset.UtcNow)
                return new BadRequestObjectResult("ScheduledForUtc must be in the future.");

            //Prepare entity
            var entity = new ScheduledEmailEntity
            {
                PartitionKey = request.ScheduledForUtc.UtcDateTime.ToString("yyyyMMdd"),
                RowKey = Guid.NewGuid().ToString("N"),

                RecipientEmail = request.RecipientEmail,
                Subject = request.Subject ?? "",
                Body = request.Body ?? "",
                ScheduledForUtc = request.ScheduledForUtc,

                Status = "Pending",
            };

            //Add to table
            await _tableClient.AddEntityAsync(entity);

            return new OkObjectResult("Success!");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error scheduling email");
            _logger.LogError(exception.Message.ToString());

            return new BadRequestObjectResult(exception.Message);
        }
    }
}