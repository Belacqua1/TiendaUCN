using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Exceptions;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryAdminService _categoryAdminService;

        public AdminCategoriesController(ICategoryAdminService categoryAdminService)
        {
            _categoryAdminService = categoryAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
        {
            var result = await _categoryAdminService.GetAllAsync(queryParams);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaxonomyCreateDto dto)
        {
            try
            {
                var result = await _categoryAdminService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaxonomyUpdateDto dto)
        {
            try
            {
                var result = await _categoryAdminService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryAdminService.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
