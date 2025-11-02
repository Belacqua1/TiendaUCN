using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Exceptions;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class OrderAdminService : IOrderAdminService
    {
        private readonly DataContext _context;
        private readonly Dictionary<string, List<string>> _validTransitions;

        public OrderAdminService(DataContext context)
        {
            _context = context;

            // R123: Definición de la Máquina de Estados
            _validTransitions = new Dictionary<string, List<string>>
            {
                {
                    "Created",
                    new List<string> { "Paid", "Cancelled" }
                },
                {
                    "Paid",
                    new List<string> { "Shipped", "Cancelled" }
                },
                {
                    "Shipped",
                    new List<string> { "Delivered" }
                },
                { "Delivered", new List<string>() }, // Estado final
                { "Cancelled", new List<string>() }, // Estado final
            };
        }

        public async Task<PagedResponse<OrderAdminListDto>> GetAllAsync(
            OrderQueryParams queryParams
        )
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();

            // R115: Filtros
            if (!string.IsNullOrWhiteSpace(queryParams.Status))
                query = query.Where(o => o.Status == queryParams.Status);
            if (queryParams.StartDate.HasValue)
                query = query.Where(o => o.OrderDate >= queryParams.StartDate.Value);
            if (queryParams.EndDate.HasValue)
                query = query.Where(o => o.OrderDate <= queryParams.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(queryParams.CustomerEmail))
                query = query.Where(o => o.User.Email.Contains(queryParams.CustomerEmail));

            // R116: Ordenamiento
            query =
                queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(o => o.OrderDate)
                    : query.OrderBy(o => o.OrderDate);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(o => new OrderAdminListDto
                {
                    Id = o.Id,
                    CustomerEmail = o.User.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                })
                .ToListAsync();

            return new PagedResponse<OrderAdminListDto>(
                orders,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalCount
            );
        }

        public async Task<OrderAdminDetailDto?> GetByIdAsync(int orderId)
        {
            var order = await _context
                .Orders.Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return new OrderAdminDetailDto
            {
                Id = order.Id,
                CustomerEmail = order.User.Email,
                CustomerFullName = $"{order.User.FirstName} {order.User.LastName}",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                Items = order
                    .OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Title,
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                    })
                    .ToList(),
                History = order
                    .StatusHistory.Select(h => new OrderStatusLogDto
                    {
                        OldStatus = h.OldStatus,
                        NewStatus = h.NewStatus,
                        Timestamp = h.Timestamp,
                        ChangedByAdminId = h.ChangedByAdminId,
                    })
                    .OrderByDescending(h => h.Timestamp)
                    .ToList(),
            };
        }

        public async Task UpdateStatusAsync(int orderId, string newStatus, int adminId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new NotFoundException("Pedido no encontrado.");

            var oldStatus = order.Status;

            // R123: Forzar transiciones de estado válidas
            if (
                !_validTransitions.ContainsKey(oldStatus)
                || !_validTransitions[oldStatus].Contains(newStatus)
            )
            {
                throw new ConflictException(
                    $"Transición de estado inválida de '{oldStatus}' a '{newStatus}'."
                );
            }

            order.Status = newStatus;

            // R125: Auditoría
            var statusLog = new OrderStatusLog
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByAdminId = adminId,
            };
            _context.OrderStatusLogs.Add(statusLog);

            // R126: No se modifica el stock aquí

            await _context.SaveChangesAsync();

            Log.Information(
                "AUDIT: El admin {AdminId} cambió el estado del pedido {OrderId} de {OldStatus} a {NewStatus}",
                adminId,
                orderId,
                oldStatus,
                newStatus
            );
        }
    }
}
