using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    public class ProductDiscountUpdateDTO
    {
        [Required]
        [Range(0, 100)]
        public int Discount { get; set; }
    }
}
