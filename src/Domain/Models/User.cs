using Bogus.DataSets;
using Microsoft.AspNetCore.Identity;

namespace TiendaUCN.src.Domain.Models
{
    public class User : IdentityUser<int>
    {
        public required string Rut { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Gender { get; set; }
        public required DateTime BirthDate { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Indica si el usuario ha aceptado los términos y condiciones.
        /// </summary>
        public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();

        /// <summary>
    }
}
