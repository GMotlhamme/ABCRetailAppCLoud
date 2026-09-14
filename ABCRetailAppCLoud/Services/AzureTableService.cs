using ABCRetailAppCLoud.Models;
using Azure.Data.Tables;
using static System.Net.WebRequestMethods;

namespace ABCRetailAppCLoud.Services
{
    public class AzureTableService
    {
        private readonly TableClient customerTableClient;
        private readonly TableClient productTableClient;
        private readonly IHttpClientFactory http;

        public AzureTableService(IHttpClientFactory http)
        {
            //var connectionString = configuration.GetRequiredSection("AzureStorage:ConnectionString").Value; ;

            //var customerTableName = configuration.GetRequiredSection("AzureStorage:CustomerTableName").Value;

            //var productTableName = configuration.GetRequiredSection("AzureStorage:ProductTableName").Value;

            ////create a client for azure storage
            //var serviceClient = new TableServiceClient(connectionString);

            ////connect the specified table
            //customerTableClient = serviceClient.GetTableClient(customerTableName);
            ////checking if our table exists, if it doesnt create the table
            //customerTableClient.CreateIfNotExists();

            //productTableClient = serviceClient.GetTableClient(productTableName);
            //productTableClient.CreateIfNotExists();
            this.http = http;
        }

        public async Task AddOrUpdateCustomerAsync(Customer customer)
        {
            //await customerTableClient.UpsertEntityAsync(customer);
            var client = http.CreateClient("AzureFunctions");
            using HttpResponseMessage httpResponse = await client.PostAsJsonAsync("api/Customer", new Customer
            {
                FullName = customer.FullName,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City
            });

            httpResponse.EnsureSuccessStatusCode();

        }

        public async Task EditCustomerAsync(Customer customer)
        {
            var client = http.CreateClient("AzureFunctions");

            var response = await client.PutAsJsonAsync( $"api/customers/{customer.PartitionKey}/{customer.RowKey}", new Customer
                {
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Address = customer.Address,
                    City = customer.City
                });

            response.EnsureSuccessStatusCode();
        }


        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var client = http.CreateClient("AzureFunctions");
            var specificCustomer = await client.GetFromJsonAsync<Customer>($"api/customers/{partitionKey}/{rowKey}");
            return specificCustomer;
        }

        public async Task<List<Customer>?> GetCustomersAsync()
        {
            var client = http.CreateClient("AzureFunctions");

            var clients = await client.GetFromJsonAsync<List<Customer>>("api/customers");

            return clients;
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            
            var client = http.CreateClient("AzureFunctions");

            var response = await client.DeleteAsync($"api/customers/{partitionKey}/{rowKey}");

            response.EnsureSuccessStatusCode();
        
        }



        public async Task AddOrUpdateProductAsync(Product product)
        {
            var client = http.CreateClient("AzureFunctions");
            using HttpResponseMessage httpResponse = await client.PostAsJsonAsync("api/products", new Product
            {
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                ProductImage = product.ProductImage
            });

            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task EditProductAsync(Product product)
        {
            var client = http.CreateClient("AzureFunctions");

            var response = await client.PutAsJsonAsync($"api/products/{product.PartitionKey}/{product.RowKey}", new Product
            {
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                ProductImage = product.ProductImage
            });

            response.EnsureSuccessStatusCode();
        }
        public async Task<Product> GetProductAsync(string partitionKey, string rowKey) 
        {
            var client = http.CreateClient("AzureFunctions");
            var httpResponse = await client.GetFromJsonAsync<Product>($"api/products/{partitionKey}/{rowKey}");
            return httpResponse;
        }

        public async Task<List<Product>?> GetProductsAsync()
        {
            var client = http.CreateClient("AzureFunctions");
            var products = await client.GetFromJsonAsync<List<Product>>("api/products");
            return products;
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            var client = http.CreateClient("AzureFunctions");
            await client.DeleteAsync($"api/deleteProduct/{partitionKey}/{rowKey}");
        }
    }
}
