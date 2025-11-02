using System.ComponentModel.DataAnnotations;
using TiendaUCN.src.Application.DTO.Public;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    // DTO para la lista de pedidos
    public class OrderAdminListDto
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    // DTO para el detalle del pedido
    public class OrderAdminDetailDto : OrderAdminListDto
    {
        public string CustomerFullName { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderStatusLogDto> History { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }

    public class OrderStatusLogDto
    {
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChangedByAdminId { get; set; }
    }

    // DTO para la entrada de actualización de estado
    public class OrderStatusUpdateDto
    {
        [Required]
        public string NewStatus { get; set; }
    }

    // DTO para los parámetros de consulta
    public class OrderQueryParams : ProductQueryParams
    {
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
