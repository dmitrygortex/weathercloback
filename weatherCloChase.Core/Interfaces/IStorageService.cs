namespace weatherCloChase.Core.Interfaces;

public interface IStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);
    Task<Stream> DownloadImageAsync(string key);
    Task DeleteImageAsync(string key);
    string GetPublicUrl(string key);
}