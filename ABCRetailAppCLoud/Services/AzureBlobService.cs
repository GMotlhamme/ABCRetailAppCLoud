using ABCRetailAppCLoud.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace ABCRetailAppCLoud.Services
{
    public class AzureBlobService
    {
        //UserDelegationKey to get the connection string and container name from appsettings
        private readonly AzureBlobStorageConfigs _setting;

        public AzureBlobService(IOptions<AzureBlobStorageConfigs> setting)
        {
            _setting = setting.Value;
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is invalid");

            var blobServiceClient = new BlobServiceClient(_setting.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_setting.ContainerName);
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

            return blobClient.Uri.ToString();
        }
    }
}
