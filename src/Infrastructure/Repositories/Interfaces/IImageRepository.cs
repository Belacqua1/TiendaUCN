namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<string> UploadImageAsync(IFormFile formFile, string fileName);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
