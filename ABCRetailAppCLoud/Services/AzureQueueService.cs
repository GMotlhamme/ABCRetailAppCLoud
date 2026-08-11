using ABCRetailAppCLoud.Models;
using Azure.Storage.Queues;
using System.Text.Json;

namespace ABCRetailAppCLoud.Services
{
    public class AzureQueueService
    {
        private readonly QueueClient queueClient;

        public AzureQueueService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;

            var queueName = configuration.GetSection("AzureStorage:QueueName").Value;

            queueClient = new QueueClient(connectionString, queueName);

            queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(OrderMessage orderMessage)
        {
            var message = JsonSerializer.Serialize(orderMessage);
            await queueClient.SendMessageAsync(message);
        }

        public async Task<List<OrderMessage>> GetMessagesAsync()
        {
            List<OrderMessage> messages = [];

            var response = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var message in response.Value)
            {
                var orderMessage =
        JsonSerializer.Deserialize<OrderMessage>(message.MessageText);

                if (orderMessage != null)
                {
                    messages.Add(orderMessage);
                }
            }

            return messages;
        }
    }
}
