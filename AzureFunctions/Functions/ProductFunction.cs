using Azure;
using Azure.Data.Tables;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzureFunctions.Functions;

public class ProductFunction
{
    private readonly TableClient productTableClient;
    private readonly ILogger<ProductFunction> _logger;

    public ProductFunction(ILogger<ProductFunction> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = Environment.GetEnvironmentVariable("AzureStorage_ConnectionString");

        var productTableName = Environment.GetEnvironmentVariable("AzureStorage_ProductTableName");

        var serviceClient = new TableServiceClient(connectionString);

        productTableClient = serviceClient.GetTableClient(productTableName);

        productTableClient.CreateIfNotExists();
    }

    [Function("AddProduct")]
    public async Task<IActionResult> AddProduct([HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (product == null)
        {
            return new BadRequestObjectResult( "Invalid product data.");
        }

        product.PartitionKey = "Product";
        product.RowKey = Guid.NewGuid().ToString();

        await productTableClient.AddEntityAsync(product);

        _logger.LogInformation("Product {ProductId} added successfully.", product.RowKey);

        return new OkObjectResult(product);
    }

    [Function("GetProducts")]
    public async Task<IActionResult> GetProducts([HttpTrigger( AuthorizationLevel.Function,"get", Route = "products")] HttpRequest req)
    {
        List<Product> products = [];

        await foreach (Product product in productTableClient.QueryAsync<Product>())
        {
            products.Add(product);
        }

        return new OkObjectResult(products);
    }


    [Function("GetProduct")]
    public async Task<IActionResult> GetProduct([HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{partitionKey}/{rowKey}" )] HttpRequest req,string partitionKey, string rowKey)
    {
        var specificProduct = await productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);

        return new OkObjectResult(specificProduct.Value);
    }


    [Function("EditProduct")]
    public async Task<IActionResult> EditProduct([HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{partitionKey}/{rowKey}" )] HttpRequest req, string partitionKey, string rowKey)
    {
        try
        {
            var specificProduct = await productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult("Request body cannot be empty.");
            }

            var product = JsonSerializer.Deserialize<Product>(requestBody,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

            if (product == null)
            {
                return new BadRequestObjectResult("Invalid customer data.");
            }

            if (string.IsNullOrWhiteSpace(product.ProductName) || string.IsNullOrWhiteSpace(product.Description))
            {
                return new BadRequestObjectResult("Product Name and Description are required.");
            }

            specificProduct.Value.ProductName = product.ProductName;
            specificProduct.Value.Description = product.Description;
            specificProduct.Value.Price = product.Price;
            specificProduct.Value.ProductImage = product.ProductImage;

            await productTableClient.UpsertEntityAsync(specificProduct.Value);

            return new OkObjectResult(specificProduct.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new NotFoundObjectResult(
                "Product could not be found.");
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(
                "Invalid JSON format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating customer.");

            return new StatusCodeResult(
                StatusCodes.Status500InternalServerError);
        }
    }

    [Function("DeleteProduct")]
    public async Task DeleteProduct([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "deleteProduct/{partitionKey}/{rowKey}")] HttpRequestMessage req, string partitionKey, string rowKey)
    {
        await productTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }
}