using ABCRetailAppCLoud.Models;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Net.Http.Headers;

namespace ABCRetailAppCLoud.Services
{
    public class AzureFileService
    {
        private readonly IHttpClientFactory http;
        private readonly ShareClient _shareClient;

        public AzureFileService(IHttpClientFactory http )
        {
            this.http = http;
        }

        //create- upload a new file
        public async Task UploadFileAsync(IFormFile file)
        {
            var client = http.CreateClient("AzureFunctions");
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync("api/UploadFileAsync", content);

            response.EnsureSuccessStatusCode();
        }

        //READ get all the files
        public async Task<List<FileUpload>> GetFilesAsync()
        {
            var client = http.CreateClient("AzureFunctions");
            var httpResponse = await client.GetFromJsonAsync<List<FileUpload>>("api/GetFiles");
            return httpResponse;
        }

        //download a file
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var client = http.CreateClient("AzureFunctions");

            var response = await client.GetAsync($"api/DownloadFile/{Uri.EscapeDataString(fileName)}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
