using Minio.DataModel.Args;
using Minio;
using PetShop_Upgrade.Services.Interfaces;
using Minio.ApiEndpoints;

namespace PetShop_Upgrade.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioService> _logger;
        private readonly string _bucketName;

        public MinioService(IMinioClient minioClient, ILogger<MinioService> logger, IConfiguration config)
        {
            _minioClient = minioClient;
            _logger = logger;
            _bucketName = config["MinIO:BucketName"];
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                // Kiểm tra bucket tồn tại, nếu không thì tạo mới
                var bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(_bucketName));

                if (!bucketExists)
                {
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(_bucketName));
                    _logger.LogInformation($"Bucket '{_bucketName}' created successfully");
                }

                // Tạo tên file duy nhất để tránh trùng
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();
                var args = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(args);
                _logger.LogInformation($"File '{fileName}' uploaded successfully");

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream(async stream => await stream.CopyToAsync(memoryStream));

                await _minioClient.GetObjectAsync(args);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file '{fileName}'");
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(args);
                _logger.LogInformation($"File '{fileName}' deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file '{fileName}'");
                throw;
            }
        }

        public async Task<List<string>> ListFilesAsync()
        {
            try
            {
                var files = new List<string>();
                var tcs = new TaskCompletionSource<bool>();

                var observable = _minioClient.ListObjectsAsync(
                    new ListObjectsArgs()
                        .WithBucket(_bucketName)
                        .WithRecursive(true)
                );
                observable.Subscribe(
                    item => files.Add(item.Key),
                    ex => tcs.SetException(ex),
                    () => tcs.SetResult(true)
                );

                await tcs.Task;
                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                throw;
            }
        }
    }
}
