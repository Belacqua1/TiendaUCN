using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.Exceptions;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderAdminService _orderAdminService;

        public AdminOrdersController(IOrderAdminService orderAdminService)
        {
            _orderAdminService = orderAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderQueryParams queryParams)
        {
            var result = await _orderAdminService.GetAllAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderAdminService.GetByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            [FromBody] OrderStatusUpdateDto dto
        )
        {
            try
            {
                var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _orderAdminService.UpdateStatusAsync(id, dto.NewStatus, adminId);
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
