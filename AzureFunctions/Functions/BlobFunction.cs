using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions;

public class BlobFunction
{
    private readonly ILogger<BlobFunction> _logger;

    public BlobFunction(ILogger<BlobFunction> logger)
    {
        _logger = logger;
    }

    [Function("BlobFunction")]
    public async Task<IActionResult> UploadImage([HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadImage")] HttpRequest req)
    {
        var form = await req.ReadFormAsync(); 
        var file = form.Files["file"];

        if (file == null || file.Length == 0) throw new ArgumentException("File is invalid");

        var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureStorage_ConnectionString"));
        var containerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("AzureStorage_ContainerName"));
        await containerClient.CreateIfNotExistsAsync();

        //Generate a unique filename
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var blobClient = containerClient.GetBlobClient(fileName);

        //Upload file with content type
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });
        }

        
        return new OkObjectResult(blobClient.Uri.ToString());
    }
}