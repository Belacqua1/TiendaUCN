using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaUCN.src.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Created"; // Ej: Created, Paid, Shipped, Delivered, Cancelled
        public string ShippingAddress { get; set; } = string.Empty;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusLog> StatusHistory { get; set; } = new List<OrderStatusLog>();
    }
}
