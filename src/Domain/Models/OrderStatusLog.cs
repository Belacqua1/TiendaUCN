namespace TiendaUCN.src.Domain.Models
{
    public class OrderStatusLog
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int ChangedByAdminId { get; set; }
    }
}
