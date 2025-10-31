namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile formFile, string fileName);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
