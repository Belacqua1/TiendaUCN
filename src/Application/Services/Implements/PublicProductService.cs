using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class PublicProductService : IPublicProductService
    {
        private readonly DataContext _context;

        public PublicProductService(DataContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ProductListDto>> GetAllAsync(ProductQueryParams queryParams)
        {
            // R66: Visibilidad - Solo productos activos y no eliminados
            var query = _context
                .Products.Where(p => p.IsAvailable && p.Status != "Eliminado") // Asumiendo IsAvailable y Status para la visibilidad
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // R68: Filtros
            if (queryParams.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);

            if (queryParams.BrandId.HasValue)
                query = query.Where(p => p.BrandId == queryParams.BrandId.Value);

            if (queryParams.MinPrice.HasValue)
                query = query.Where(p =>
                    (p.Price - (p.Price * p.discount / 100)) >= queryParams.MinPrice.Value
                );

            if (queryParams.MaxPrice.HasValue)
                query = query.Where(p =>
                    (p.Price - (p.Price * p.discount / 100)) <= queryParams.MaxPrice.Value
                );

            // R69: Búsqueda
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchTermLower)
                    || p.Description.ToLower().Contains(searchTermLower)
                );
            }

            // R70: Ordenamiento seguro (whitelist)
            var sortOrder = queryParams.SortOrder?.ToLower() == "desc" ? "desc" : "asc";
            switch (queryParams.SortBy?.ToLower())
            {
                case "price":
                    query =
                        sortOrder == "asc"
                            ? query.OrderBy(p => p.Price)
                            : query.OrderByDescending(p => p.Price);
                    break;
                case "name":
                    query =
                        sortOrder == "asc"
                            ? query.OrderBy(p => p.Title)
                            : query.OrderByDescending(p => p.Title);
                    break;
                default:
                    query = query.OrderBy(p => p.CreatedAt); // Orden por defecto
                    break;
            }

            // R67: Paginación
            var totalCount = await query.CountAsync();
            var pagedProducts = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // R71 & R72: Mapeo a DTO público con precio final calculado
            var productDtos = pagedProducts
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    FinalPrice = p.Price - (p.Price * p.discount / 100),
                    MainImageUrl = p.Images.FirstOrDefault()?.ImageUrl ?? "/images/placeholder.png",
                    CategoryName = p.Category.Name,
                })
                .ToList();

            return new PagedResponse<ProductListDto>(
                productDtos,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalCount
            );
        }

        public async Task<ProductDetailDto?> GetByIdAsync(int id)
        {
            // R75: Visibilidad - Solo producto activo y no eliminado
            var product = await _context
                .Products.Where(p => p.Id == id && p.IsAvailable && p.Status != "Eliminado")
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null; // Devuelve null para que el controlador retorne 404 Not Found
            }

            // R76 & R77: Mapeo a DTO de detalle con precio final y datos de relaciones
            return new ProductDetailDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Discount = product.discount,
                FinalPrice = product.Price - (product.Price * product.discount / 100),
                CategoryName = product.Category.Name,
                BrandName = product.Brand.Name,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
            };
        }
    }
}
