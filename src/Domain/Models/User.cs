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
        public DateTime? LastLoginDate { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? PendingEmail { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpires { get; set; }
    }
}
