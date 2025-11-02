using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    public class ProductStatusUpdateDTO
    {
        [Required]
        public bool IsAvailable { get; set; }
    }
}
