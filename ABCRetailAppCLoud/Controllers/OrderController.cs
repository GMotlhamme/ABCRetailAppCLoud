using ABCRetailAppCLoud.Models;
using ABCRetailAppCLoud.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAppCLoud.Controllers
{
    public class OrderController : Controller
    {
        private readonly AzureQueueService azureQueueService;
        private readonly AzureTableService azureTableService;

        public OrderController(AzureQueueService azureQueueService, AzureTableService azureTableService)
        {
            this.azureQueueService = azureQueueService;
            this.azureTableService = azureTableService;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await azureQueueService.GetMessagesAsync();
            return View(messages);
        }

       

        public async Task<IActionResult> Create(OrderMessage message)
        {
            if (!ModelState.IsValid)
            {
                var products = await azureTableService.GetProductsAsync();
                var customers = await azureTableService.GetCustomersAsync();

                ViewBag.Products = products;
                ViewBag.Customers = customers;

                return View(message);
            }

            try
            {
                var orderId = Guid.NewGuid().ToString();

                var orderMessage = new OrderMessage
                {
                    OrderId = orderId,
                    CustomerId = message.CustomerId,
                    ProductId = message.ProductId,
                    Action = "ProcessOrder",
                    Quantity = message.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                var inventoryMessage = new OrderMessage
                {
                    OrderId = orderId,
                    CustomerId = message.CustomerId,
                    ProductId = message.ProductId,
                    Action = "UpdateInventory",
                    Quantity = message.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                await azureQueueService.SendMessageAsync(orderMessage);
                await azureQueueService.SendMessageAsync(inventoryMessage);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to process the order.");
                return View(message);
            }
        }
    }
}
