using ABCRetailAppCLoud.Models;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABCRetailAppCLoud.Services
{
    public class AzureFileService
    {
        private readonly ShareClient _shareClient;

        public AzureFileService(IConfiguration configuration)
        {

            //go to app settings and fetch the connection string value
            var connectionString = configuration["AzureStorage:ConnectionString"];

            //connect to the Azure File share
            _shareClient = new ShareClient(connectionString, "retailfiles");

            //checking if our table exists, if it doesnt create the table
            _shareClient.CreateIfNotExists();

        }

        //create- upload a new file
        public async Task UploadFileAsync(IFormFile file)
        {
            // Get the root directory of the File share
            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

            //create a reference to the file
            ShareFileClient fileClient = directory.GetFileClient(file.FileName);

            //open the upload file
            using Stream stream = file.OpenReadStream();

            //create the file in Azure
            await fileClient.CreateAsync(stream.Length);

            //upload the contents of the file
            await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
        }

        //READ get all the files
        public async Task<List<FileUpload>> GetFilesAsync()
        {
            List<FileUpload> files = [];

            //get the root directory 

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

            //retrieve all files in the directory
            await foreach (ShareFileItem item in directory.GetFilesAndDirectoriesAsync())
            {
                //ignore directories
                if (!item.IsDirectory)
                {
                    FileUpload studentFile = new FileUpload();

                    studentFile.FileName = item.Name;

                    if (item.FileSize.HasValue)
                    {
                        studentFile.FileSize = item.FileSize.Value;
                    }
                    files.Add(studentFile);
                }
            }
            return files;
        }

        //download a file
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

            ShareFileClient fileClient = directory.GetFileClient(fileName);

            //download the file from azure
            ShareFileDownloadInfo download = await fileClient.DownloadAsync();

            return download.Content;
        }
    }
}
