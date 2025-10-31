using TiendaUCN.src.Infrastructure.Repositories.Interfaces;
namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class ImageRepository : IImageRepository
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImageRepository(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile formFile, string fileName)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            var imageUrl = $"/uploads/{fileName}";
            return imageUrl;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
    }
}
