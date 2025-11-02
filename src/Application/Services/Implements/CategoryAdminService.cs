using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Exceptions;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class CategoryAdminService : ICategoryAdminService
    {
        private readonly DataContext _context;

        public CategoryAdminService(DataContext context)
        {
            _context = context;
        }

        public async Task<TaxonomyResponseDto> CreateAsync(TaxonomyCreateDto dto)
        {
            var slug = GenerateSlug(dto.Name);
            if (
                await _context.Categories.AnyAsync(c =>
                    c.Name.ToLower() == dto.Name.ToLower() || c.Slug == slug
                )
            )
            {
                throw new BusinessRuleException("Ya existe una categoría con este nombre o slug.");
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToResponseDto(category, 0);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new NotFoundException("Categoría no encontrada.");

            var productCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
            {
                throw new ConflictException(
                    $"No se puede eliminar la categoría, tiene {productCount} productos asociados."
                );
            }

            category.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResponse<TaxonomyResponseDto>> GetAllAsync(
            ProductQueryParams queryParams
        )
        {
            var query = _context.Categories.Where(c => !c.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(queryParams.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var categories = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(c => new TaxonomyResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id),
                })
                .ToListAsync();

            return new PagedResponse<TaxonomyResponseDto>(
                categories,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalCount
            );
        }

        public async Task<TaxonomyResponseDto> UpdateAsync(int id, TaxonomyUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new NotFoundException("Categoría no encontrada.");

            var newSlug = GenerateSlug(dto.Name);
            if (
                await _context.Categories.AnyAsync(c =>
                    c.Id != id && (c.Name.ToLower() == dto.Name.ToLower() || c.Slug == newSlug)
                )
            )
            {
                throw new BusinessRuleException("Ya existe otra categoría con este nombre o slug.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = newSlug;

            await _context.SaveChangesAsync();

            var productCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            return MapToResponseDto(category, productCount);
        }

        private string GenerateSlug(string name)
        {
            var slug = name.ToLower().Trim();
            slug = Regex.Replace(slug, @"\s+", "-"); // Replace spaces with -
            slug = Regex.Replace(slug, @"[^a-z0-9-]", ""); // Remove invalid chars
            return slug;
        }

        private TaxonomyResponseDto MapToResponseDto(Category category, int productCount) =>
            new()
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                ProductCount = productCount,
            };
    }
}
