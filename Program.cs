using Azure.Communication.Email;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//Prepare builder
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

//Table client
var tableClient_connectionString = Environment.GetEnvironmentVariable("ScheduledEmailsTableConnectionString");

if (string.IsNullOrWhiteSpace(tableClient_connectionString))
    throw new InvalidOperationException("ScheduledEmailsTableConnectionString is not configured.");

builder.Services.AddSingleton(_ => new TableClient(tableClient_connectionString, "ScheduledEmails"));

//Email client
var emailCommunicationClient_connectionString = Environment.GetEnvironmentVariable("CommunicationServicesConnectionString");

if (string.IsNullOrWhiteSpace(emailCommunicationClient_connectionString))
    throw new InvalidOperationException("CommunicationServicesConnectionString is not configured.");

builder.Services.AddSingleton(_ => new EmailClient(emailCommunicationClient_connectionString));

//Run build
builder.Build().Run();