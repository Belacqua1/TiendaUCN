using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly DataContext _context;

        public ProductAdminService(DataContext context)
        {
            _context = context;
        }

        public async Task<ProductAdminResponseDTO> CreateAsync(ProductCreateDTO createDto)
        {
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == createDto.BrandId);
            if (!brandExists)
                throw new ArgumentException("BrandId no es válido.");

            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.Id == createDto.CategoryId
            );
            if (!categoryExists)
                throw new ArgumentException("CategoryId no es válido.");

            var product = new Product
            {
                Title = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                Price = createDto.Price,
                Stock = createDto.Stock,
                BrandId = createDto.BrandId,
                CategoryId = createDto.CategoryId,
                IsAvailable = true,
                Status = "Activo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return (await GetByIdAdminAsync(product.Id))!;
        }

        public async Task<ProductAdminResponseDTO?> GetByIdAdminAsync(int id)
        {
            var product = await _context
                .Products.Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            return new ProductAdminResponseDTO
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Discount = product.discount,
                Stock = product.Stock,
                Status = product.Status,
                IsAvailable = product.IsAvailable,
                CategoryName = product.Category.Name,
                BrandName = product.Brand.Name,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
            };
        }

        public async Task<PagedResponse<ProductAdminResponseDTO>> GetAllAdminAsync(
            ProductQueryParams queryParams
        )
        {
            var query = _context
                .Products.Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // Aquí se podrían añadir filtros para el admin, por ejemplo, para ver solo los eliminados.
            // if (queryParams.IncludeDeleted) { ... }

            var totalCount = await query.CountAsync();
            var pagedProducts = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var productDtos = pagedProducts
                .Select(p => new ProductAdminResponseDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Discount = p.discount,
                    Stock = p.Stock,
                    Status = p.Status,
                    IsAvailable = p.IsAvailable,
                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                })
                .ToList();

            return new PagedResponse<ProductAdminResponseDTO>(
                productDtos,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalCount
            );
        }

        public async Task<bool> UpdateAsync(int id, ProductUpdateDTO updateDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.Title = updateDto.Title;
            product.Description = updateDto.Description ?? product.Description;
            product.Price = updateDto.Price;
            product.Stock = updateDto.Stock;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.Status = "Eliminado";
            product.IsAvailable = false; // Un producto eliminado no puede estar disponible
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isAvailable)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Status == "Eliminado")
                return false;

            product.IsAvailable = isAvailable;
            product.Status = isAvailable ? "Activo" : "Inactivo";
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateDiscountAsync(int id, int discount)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.discount = discount;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
