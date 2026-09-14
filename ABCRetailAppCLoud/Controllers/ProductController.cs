using ABCRetailAppCLoud.Models;
using ABCRetailAppCLoud.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAppCLoud.Controllers
{
    public class ProductController : Controller
    {
        private readonly AzureTableService azureTableService;
        private readonly AzureBlobService azureBlobService;

        public ProductController(AzureTableService azureTableService, AzureBlobService azureBlobService)
        {
            this.azureTableService = azureTableService;
            this.azureBlobService = azureBlobService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allProducts = await azureTableService.GetProductsAsync();
            return View(allProducts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Missing information, bad request");
                return View(product);
            }
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a product image.");
                return View(product);
            }
            try
            {
                product.PartitionKey = "Product";
                product.RowKey = Guid.NewGuid().ToString();
                var image = await azureBlobService.UploadImageAsync(file);
                product.ProductImage = image;
                await azureTableService.AddOrUpdateProductAsync(product);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Could not upload product, please try again later");
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var singleProduct = await azureTableService.GetProductAsync(partitionKey, rowKey);
                return View(singleProduct);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var product = await azureTableService.GetProductAsync(partitionKey, rowKey);
                return View(product);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Missing information, bad request");
                return View(product);
            }
            try
            {
                if(file != null)
                {
                    var image = await azureBlobService.UploadImageAsync(file);
                    product.ProductImage = image;
                }
                await azureTableService.EditProductAsync(product);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to Edit the Product's details ");
                return View(product);
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                await azureTableService.DeleteProductAsync(partitionKey, rowKey);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return NotFound();
            }

        }
    }
}
