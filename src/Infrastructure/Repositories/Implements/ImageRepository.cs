using System.Net;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class ImageRepository : IImageRepository
    {
        private readonly DataContext _context;

        public ImageRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates an image file in the database.
        /// </summary>
        /// <param name="file">The image file to create.</param>
        /// <returns>True if the file was created successfully, otherwise false and null if the image already exists.</returns>
        public async Task<bool?> CreateAsync(Image file)
        {
            var existsImage = await _context.Images.AnyAsync(i => i.PublicId == file.PublicId);
            if (!existsImage)
            {
                _context.Images.Add(file);
                return await _context.SaveChangesAsync() > 0;
            }
            return null;
        }

        /// <summary>
        /// Deletes an image file from the database.
        /// </summary>
        /// <param name="publicId">The public identifier of the file to delete.</param>
        /// <returns>True if the file was deleted successfully, otherwise false and null if the image does not exist.</returns>
        public async Task<bool?> DeleteAsync(string publicId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.PublicId == publicId);
            if (image != null)
            {
                _context.Images.Remove(image);
                return await _context.SaveChangesAsync() > 0;
            }
            return null;
        }
    }
}
