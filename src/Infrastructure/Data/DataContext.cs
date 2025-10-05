using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Data;

public class DataContext : IdentityDbContext<User, Role, int>
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    //public DbSet<Order> Orders { get; set; } = null!;
    //public DbSet<OrderItem> OrderItems { get; set; } = null!;
    //public DbSet<Cart> Carts { get; set; } = null!;
    //public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<VerificationCode> VerificationCodes { get; set; } = null!;
}
