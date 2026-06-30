namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<byte[]> DownloadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
        Task<List<string>> ListFilesAsync();
    }
}
