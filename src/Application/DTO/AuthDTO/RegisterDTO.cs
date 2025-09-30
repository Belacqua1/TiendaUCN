namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    public class Register
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Rut { get; set; }
        public required string Gender { get; set; }
        public required DateTime BirthDate { get; set; }
    }
}
