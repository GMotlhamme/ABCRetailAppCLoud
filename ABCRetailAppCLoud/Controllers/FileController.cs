using ABCRetailAppCLoud.Models;
using ABCRetailAppCLoud.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAppCLoud.Controllers
{
    public class FileController : Controller
    {
        private readonly AzureFileService azureFileService;

        public FileController(AzureFileService azureFileService)
        {
            this.azureFileService = azureFileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listOfFiles = await azureFileService.GetFilesAsync();
            return View(listOfFiles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create( IFormFile file)
        {
            
            try
            {
                if(file == null || file.Length == 0)
                {
                    ModelState.AddModelError("file", "Please select a file.");
                    return View();
                }
                await azureFileService.UploadFileAsync(file);
                return RedirectToAction("Index");

            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to upload your file");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var file = await azureFileService.DownloadFileAsync(fileName);
                return File(file, "application/octet-stream", fileName);
        }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to download your file");
                return View(fileName);
    }
}
    }
}
