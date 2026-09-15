using Azure.Storage.Queues;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzureFunctions.Functions;

public class QueueFunction
{
    private readonly QueueClient queueClient;

    public QueueFunction()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureStorage_ConnectionString");

        var queueName = Environment.GetEnvironmentVariable("AzureStorage_QueueName");

        queueClient = new QueueClient(connectionString, queueName);

        queueClient.CreateIfNotExists();
    }

    [Function("AddQueue")]
    public async Task<IActionResult> AddQueue([HttpTrigger(AuthorizationLevel.Function, "post", Route = "AddQueue")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return new BadRequestObjectResult("Request body cannot be empty.");
        }

        var orderMessage = JsonSerializer.Deserialize<OrderMessage>(requestBody, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

        if (orderMessage == null)
        {
            return new BadRequestObjectResult("Invalid order message.");
        }

        var message = JsonSerializer.Serialize(orderMessage);
        await queueClient.SendMessageAsync(message);
        return new OkObjectResult("Order added successfully!");
    }

    [Function("GetQueueMessage")]
    public async Task<IActionResult> GetQueueMessages([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetQueueMessage")] HttpRequest req)
    {
        List<OrderMessage> messages = [];

        var response = await queueClient.PeekMessagesAsync(maxMessages: 32);

        foreach (var message in response.Value)
        {
            var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message.MessageText);

            if (orderMessage != null)
            {
                messages.Add(orderMessage);
            }
        }

        return new OkObjectResult(messages);
    }
}