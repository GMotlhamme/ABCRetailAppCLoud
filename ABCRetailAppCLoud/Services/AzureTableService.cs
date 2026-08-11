using ABCRetailAppCLoud.Models;
using Azure.Data.Tables;

namespace ABCRetailAppCLoud.Services
{
    public class AzureTableService
    {
        private readonly TableClient customerTableClient;
        private readonly TableClient productTableClient;

        public AzureTableService(IConfiguration configuration)
        {
            var connectionString = configuration.GetRequiredSection("AzureStorage:ConnectionString").Value; ;

            var customerTableName = configuration.GetRequiredSection("AzureStorage:CustomerTableName").Value;

            var productTableName = configuration.GetRequiredSection("AzureStorage:ProductTableName").Value;

            //create a client for azure storage
            var serviceClient = new TableServiceClient(connectionString);

            //connect the specified table
            customerTableClient = serviceClient.GetTableClient(customerTableName);
            //checking if our table exists, if it doesnt create the table
            customerTableClient.CreateIfNotExists();

            productTableClient = serviceClient.GetTableClient(productTableName);
            productTableClient.CreateIfNotExists();
        }

        public async Task AddOrUpdateCustomerAsync(Customer customer)
        {
            await customerTableClient.UpsertEntityAsync(customer);
        }

        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey) 
        {
            var specificCustomer = await customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return specificCustomer.Value;
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            List<Customer> listOfCustomers = [];
            await foreach(Customer customer in customerTableClient.QueryAsync<Customer>())
            {
                listOfCustomers.Add(customer);
            }
            return listOfCustomers;
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }



        public async Task AddOrUpdateProductAsync(Product product)
        {
            await productTableClient.UpsertEntityAsync(product);
        }
        public async Task<Product> GetProductAsync(string partitionKey, string rowKey) 
        {
            var specificProduct = await productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return specificProduct.Value;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            List<Product> listOfProducts = [];
            await foreach(Product product in productTableClient.QueryAsync<Product>())
            {
                listOfProducts.Add(product);
            }
            return listOfProducts;
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
