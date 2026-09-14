using ABCRetailAppCLoud.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace ABCRetailAppCLoud.Services
{
    public class AzureBlobService
    {
        
        private readonly IHttpClientFactory http;

        public AzureBlobService(IHttpClientFactory http)
        {
            this.http = http;
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var client = http.CreateClient("AzureFunctions");
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync("api/UploadImage", content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
