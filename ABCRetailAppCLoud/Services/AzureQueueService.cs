using ABCRetailAppCLoud.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ABCRetailAppCLoud.Services
{
    public class AzureQueueService
    {
        private readonly IHttpClientFactory http;

        public AzureQueueService(IHttpClientFactory http)
        {
            this.http = http;
        }

        public async Task SendMessageAsync(OrderMessage orderMessage)
        {
            var client = http.CreateClient("AzureFunctions");
            await client.PostAsJsonAsync("api/AddQueue", new OrderMessage
            {
                OrderId = orderMessage.OrderId,
                CustomerId = orderMessage.CustomerId,
                ProductId = orderMessage.ProductId,
                Action = orderMessage.Action,
                Quantity = orderMessage.Quantity,
                CreatedAt = orderMessage.CreatedAt
            });
        }

        public async Task<List<OrderMessage>?> GetMessagesAsync()
        {
            var client = http.CreateClient("AzureFunctions");
            var orders = await client.GetFromJsonAsync<List<OrderMessage>>("api/GetQueueMessage");
            return orders;
        }
    }
}
