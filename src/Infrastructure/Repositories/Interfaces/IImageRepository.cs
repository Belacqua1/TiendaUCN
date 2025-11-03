using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interface for the image file repository.
    /// Defines methods to handle operations related to image files.
    /// </summary>
    public interface IImageRepository
    {
        /// <summary>
        /// Creates an image file in the database.
        /// </summary>
        /// <param name="file">The image file to create.</param>
        /// <returns>True if the file was created successfully, otherwise false and null if the image already exists.</returns>
        Task<bool?> CreateAsync(Image file);

        /// <summary>
        /// Deletes an image file from the database.
        /// </summary>
        /// <param name="publicId">The public identifier of the file to delete.</param>
        /// <returns>True if the file was deleted successfully, otherwise false and null if the image does not exist.</returns>
        Task<bool?> DeleteAsync(string publicId);
    }
}
