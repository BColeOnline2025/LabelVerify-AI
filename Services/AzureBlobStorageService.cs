using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using LabelVerify.Web.Models;
using LabelVerify.Web.Options;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadAsync(Stream stream, string container, string fileName);
        Task<byte[]> DownloadAsync(string container, string blobName);
    }

    public class AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options, IConfiguration configuration)
    {
        private readonly AzureBlobStorageOptions _options = options.Value;
        private readonly IConfiguration _configuration = configuration;

        private string GetConnectionString()
        {
            return _options.ConnectionString
                ?? _configuration["AzureBlobStorageConnectionString"]
                ?? _configuration["AzureBlobStorage:ConnectionString"]
                ?? string.Empty;
        }

        public async Task<BlobUploadResult> UploadAsync(Stream stream, string containerName, string blobName, string contentType)
        {
            var connectionString = GetConnectionString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage is not configured. Missing AzureBlobStorageConnectionString.");
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = containerClient.GetBlobClient(blobName);

            stream.Position = 0;

            await blobClient.UploadAsync(stream, new BlobHttpHeaders{ContentType = contentType});

            return new BlobUploadResult
            {
                ContainerName = containerName,
                BlobName = blobName,
                BlobUrl = blobClient.Uri.ToString()
            };
        }

        public async Task<MemoryStream> DownloadAsync(string containerName, string blobName)
        {
            var connectionString = GetConnectionString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage is not configured. Missing AzureBlobStorageConnectionString.");
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob not found: {containerName}/{blobName}");
            }

            var stream = new MemoryStream();

            await blobClient.DownloadToAsync(stream);

            stream.Position = 0;

            return stream;
        }

        public async Task<MemoryStream> DownloadAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);

            var pathParts = uri.AbsolutePath.Trim('/').Split('/', 2);

            if (pathParts.Length != 2)
            {
                throw new InvalidOperationException($"Invalid blob URL: {blobUrl}");
            }

            var containerName = pathParts[0];
            var blobName = Uri.UnescapeDataString(pathParts[1]);

            return await DownloadAsync(containerName, blobName);
        }

        public string GenerateReadSasUrl(string containerName, string blobName, int expirationMinutes = 15)
        {
            var containerClient = new BlobContainerClient(_options.ConnectionString, containerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient
                .GenerateSasUri(sasBuilder)
                .ToString();
        }

        public string GenerateReadSasUrl(string blobUrl, int expirationMinutes = 15)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                return string.Empty;
            }

            var uri = new Uri(blobUrl);
            var path = uri.AbsolutePath.TrimStart('/');
            var firstSlash = path.IndexOf('/');

            if (firstSlash < 0)
            {
                throw new InvalidOperationException("Invalid blob URL. Unable to determine container and blob name.");
            }

            var containerName = path[..firstSlash];
            var blobName = path[(firstSlash + 1)..];

            return GenerateReadSasUrl(containerName, blobName, expirationMinutes);
        }
    }
}