using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions;

public class FileFunction
{
    private readonly ShareClient _shareClient;

    public FileFunction(ILogger<FileFunction> logger)
    {
        //go to app settings and fetch the connection string value
        var connectionString = Environment.GetEnvironmentVariable("AzureStorage_ConnectionString");

        //connect to the Azure File share
        _shareClient = new ShareClient(connectionString, "retailfiles");

        // Create the file share if it doesn't exist
        _shareClient.CreateIfNotExists();
    }

    [Function("UploadFileAsync")]
    public async Task<IActionResult> UploadFileAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadFileAsync")] HttpRequest req)
    {
        var form = await req.ReadFormAsync();
        var file = form.Files["file"];

        // Get the root directory of the File share
        ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

        if (file == null || file.Length == 0)
        {
            return new BadRequestObjectResult("File is invalid.");
        }

        //create a reference to the file
        ShareFileClient fileClient = directory.GetFileClient(file.FileName);

        //open the upload file
        using Stream stream = file.OpenReadStream();

        //create the file in Azure
        await fileClient.CreateAsync(stream.Length);

        //upload the contents of the file
        await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

        return new OkObjectResult("File uploaded successfully");
    }


    [Function("GetFiles")]
    public async Task<IActionResult> GetFiles([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetFiles")] HttpRequest req)
    {
        List<FileUpload> files = [];

        ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

        await foreach ( ShareFileItem item in directory.GetFilesAndDirectoriesAsync())
        {
            // Ignore directories
            if (!item.IsDirectory)
            {
                FileUpload file = new FileUpload();

                file.FileName = item.Name;

                if (item.FileSize.HasValue)
                {
                    file.FileSize = item.FileSize.Value;
                }

                files.Add(file);
            }
        }

        return new OkObjectResult(files);
    }

    [Function("DownloadFile")]
    public async Task<IActionResult> DownloadFile(
    [HttpTrigger(AuthorizationLevel.Function,"get", Route = "DownloadFile/{fileName}")] HttpRequest req, string fileName)
    {
        ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

        ShareFileClient fileClient = directory.GetFileClient(fileName);

        try
        {
            ShareFileDownloadInfo download = await fileClient.DownloadAsync();

            return new FileStreamResult( download.Content, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }
        catch (Azure.RequestFailedException ex)
            when (ex.Status == 404)
        {
            return new NotFoundObjectResult("File could not be found.");
        }
    }
}