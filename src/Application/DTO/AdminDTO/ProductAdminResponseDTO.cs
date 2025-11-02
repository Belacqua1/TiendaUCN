namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    public class ProductAdminResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
        public bool IsAvailable { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
