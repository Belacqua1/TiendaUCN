using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Product;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<GenericResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Status = "Activo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var created = await _repository.CreateAsync(product);

            var response = new ProductResponseDto
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                Price = created.Price,
                Stock = created.Stock,
                CategoryName = created.Category.Name,
                BrandName = created.Brand.Name,
            };

            return new GenericResponse<ProductResponseDto>(
                "Producto creado correctamente",
                response,
                true
            );
        }

        public async Task<GenericResponse<ProductResponseDto>> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                return new GenericResponse<ProductResponseDto>(
                    "Producto no encontrado.",
                    null,
                    false
                );

            var dto = new ProductResponseDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = product.Category.Name,
                BrandName = product.Brand.Name,
            };

            return new GenericResponse<ProductResponseDto>(
                "Producto creado correctamente",
                null,
                true
            );
        }
    }
}
