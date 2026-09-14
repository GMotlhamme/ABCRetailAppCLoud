using Azure;
using Azure.Data.Tables;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AzureFunctions;

public class CustomerFunction
{
    private readonly TableClient customerTableClient;
    private readonly ILogger<CustomerFunction> _logger;

    public CustomerFunction(ILogger<CustomerFunction> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = Environment.GetEnvironmentVariable("AzureStorage_ConnectionString");

        var customerTableName = Environment.GetEnvironmentVariable("AzureStorage_CustomerTableName");

        var serviceClient = new TableServiceClient(connectionString);

        customerTableClient = serviceClient.GetTableClient(customerTableName);

        customerTableClient.CreateIfNotExists();
    }

    [Function("AddCustomer")]
    public async Task<IActionResult> AddCustomer([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Customer")]
        HttpRequest req)
    {
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var customer = JsonSerializer.Deserialize<Customer>(
                            requestBody,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

            if (customer == null)
            {
                return new BadRequestObjectResult("Invalid customer data.");
            }

            customer.PartitionKey = "Customers";
            customer.RowKey = Guid.NewGuid().ToString();

            await customerTableClient.AddEntityAsync(customer);

            _logger.LogInformation("Customer {CustomerId} added successfully.", customer.RowKey);

            return new OkObjectResult(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customer.");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("GetCustomers")]
    public async Task<IActionResult> GetCustomers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequest req)
    {
        
            List<Customer> listOfCustomers = [];
            await foreach (Customer customer in customerTableClient.QueryAsync<Customer>())
            {
                listOfCustomers.Add(customer);
            }
            return new OkObjectResult(listOfCustomers);
        
    }
    [Function("GetCustomer")]
    public async Task<IActionResult> GetCustomer([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{partitionKey}/{rowKey}"
    )] HttpRequest req,string partitionKey, string rowKey)
    { 
        var specificCustomer = await customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
        return new OkObjectResult(specificCustomer.Value);

    }


    [Function("RemoveCustomer")]
    public async Task<IActionResult> RemoveCustomer([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{partitionKey}/{rowKey}"
    )] HttpRequest req,string partitionKey, string rowKey)
    { 
         await customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
         return new OkObjectResult("Customer deleted successfully.");
    }


    [Function("EditCustomer")]
    public async Task<IActionResult> EditCustomer([HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{partitionKey}/{rowKey}"
    )] HttpRequest req, string partitionKey, string rowKey)
    {
        try { 
        var specificCustomer = await customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return new BadRequestObjectResult("Request body cannot be empty.");
        }

        var customer = JsonSerializer.Deserialize<Customer>(requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

        if (customer == null)
        {
            return new BadRequestObjectResult("Invalid customer data.");
        }

        if (string.IsNullOrWhiteSpace(customer.FullName) || string.IsNullOrWhiteSpace(customer.Email))
        {
            return new BadRequestObjectResult("FullName and Email are required.");
        }

        specificCustomer.Value.Email = customer.Email;
        specificCustomer.Value.Address = customer.Address;
        specificCustomer.Value.City = customer.City;
        specificCustomer.Value.FullName = customer.FullName;

        await customerTableClient.UpsertEntityAsync(specificCustomer.Value);

        return new OkObjectResult(specificCustomer.Value);
    }
    catch (RequestFailedException ex) when(ex.Status == 404)
    {
        return new NotFoundObjectResult(
            "Customer could not be found.");
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
}