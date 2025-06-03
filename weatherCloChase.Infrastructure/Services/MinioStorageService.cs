using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using weatherCloChase.Core.Interfaces;

namespace weatherCloChase.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public MinioStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["Minio:Endpoint"]!;
        var accessKey = configuration["Minio:AccessKey"]!;
        var secretKey = configuration["Minio:SecretKey"]!;
        _bucketName = configuration["Minio:BucketName"]!;
        _publicUrl = configuration["Minio:PublicUrl"]!;

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();

        // Создаем bucket если не существует
        Task.Run(async () => await EnsureBucketExists());
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";
        
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(key)
            .WithStreamData(imageStream)
            .WithObjectSize(imageStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);
        
        return key;
    }

    public async Task<Stream> DownloadImageAsync(string key)
    {
        var memoryStream = new MemoryStream();
        
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(key)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getObjectArgs);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public async Task DeleteImageAsync(string key)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(key);

        await _minioClient.RemoveObjectAsync(removeObjectArgs);
    }

    public string GetPublicUrl(string key)
    {
        return $"{_publicUrl}/{_bucketName}/{key}";
    }

    private async Task EnsureBucketExists()
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
        var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs);
        
        if (!exists)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs);
        }
    }
}