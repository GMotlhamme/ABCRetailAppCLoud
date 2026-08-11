using ABCRetailAppCLoud.Models;
using ABCRetailAppCLoud.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace ABCRetailAppCLoud.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureTableService azureTableService;

        public CustomerController(AzureTableService azureTableService)
        {
            this.azureTableService = azureTableService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allCustomers = await azureTableService.GetCustomersAsync();
            return View(allCustomers);
        }

        [HttpGet]
        public IActionResult Create() 
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid) 
            {
                ModelState.AddModelError("", "Missing information, bad request");
                return View(customer);
            }
            try
            {
                customer.PartitionKey = "Customer";
                customer.RowKey = Guid.NewGuid().ToString();
                await azureTableService.AddOrUpdateCustomerAsync(customer);
                return RedirectToAction("Index");
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", "Unable to create record: " + ex.Message);
                return View(customer);
            }
        }
        

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var customerDetails = await azureTableService.GetCustomerAsync(partitionKey, rowKey);
                return View(customerDetails);
            }catch(Exception)
            {
                return NotFound();
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await azureTableService.GetCustomerAsync(partitionKey, rowKey);
                return View(customer);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Missing information, bad request");
                return View(customer);
            }
            try
            {
                await azureTableService.AddOrUpdateCustomerAsync(customer);
                return RedirectToAction("Index");
            }catch(Exception)
            {
                ModelState.AddModelError("", "Unable to Edit the customer's details ");
                return View(customer);
            }
        }

        

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                await azureTableService.DeleteCustomerAsync(partitionKey, rowKey); 
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return NotFound();
            }

        }
    }
}
