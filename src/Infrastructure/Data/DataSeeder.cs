using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.Extensions.UnitedKingdom;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace Tienda_UCN_api.src.Infrastructure.Data
{
    public class DataSeeder
    {
        /// <summary>
        /// Method to initialize the database with test data.
        /// </summary>
        /// <param name="serviceProvider">Service provider to get the data context and other services.</param>
        /// <returns>Asynchronous task representing the initialization operation.</returns>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await context.Database.MigrateAsync();
                var genders =
                    configuration.GetSection("Genders").Get<string[]>()
                    ?? throw new InvalidOperationException(
                        "The gender configuration is not complete"
                    );

                // Role creation
                if (!context.Roles.Any())
                {
                    var roles = new List<Role>
                    {
                        new Role { Name = "Admin", NormalizedName = "ADMIN" },
                        new Role { Name = "Cliente", NormalizedName = "Cliente" },
                    };
                    foreach (var role in roles)
                    {
                        var result = roleManager.CreateAsync(role).GetAwaiter().GetResult();
                        if (!result.Succeeded)
                        {
                            Log.Error(
                                "Error creando rol {RoleName}: {Errors}",
                                role.Name,
                                string.Join(", ", result.Errors.Select(e => e.Description))
                            );
                            throw new InvalidOperationException(
                                $"No se pudo crear el rol {role.Name}."
                            );
                        }
                    }
                    Log.Information("Roles created successfully.");
                }

                // Category creation
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Electronics", Slug = "electronics" },
                        new Category { Name = "Clothing", Slug = "clothing" },
                        new Category { Name = "Home Appliances", Slug = "home-appliances" },
                        new Category { Name = "Books", Slug = "books" },
                        new Category { Name = "Sports", Slug = "sports" },
                    };
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Log.Information("Categories created successfully.");
                }

                // Brand creation
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                    {
                        new Brand { Name = "Sony", Slug = "sony" },
                        new Brand { Name = "Apple", Slug = "apple" },
                        new Brand { Name = "HP", Slug = "hp" },
                    };
                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                    Log.Information("Brands created successfully.");
                }

                // Creación de usuarios
                if (!await context.Users.AnyAsync())
                {
                    Role customerRole =
                        await context.Roles.FirstOrDefaultAsync(r => r.Name == "Cliente")
                        ?? throw new InvalidOperationException(
                            "The customer role is not configured."
                        );
                    Role adminRole =
                        await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin")
                        ?? throw new InvalidOperationException(
                            "The administrator role is not configured."
                        );

                    // Administrator user creation
                    User adminUser = new User
                    {
                        FirstName =
                            configuration["User:AdminUser:FirstName"]
                            ?? throw new InvalidOperationException(
                                "The administrator user name is not configured."
                            ),
                        LastName =
                            configuration["User:AdminUser:LastName"]
                            ?? throw new InvalidOperationException(
                                "The administrator user last name is not configured."
                            ),
                        Email =
                            configuration["User:AdminUser:Email"]
                            ?? throw new InvalidOperationException(
                                "The administrator user email is not configured."
                            ),
                        EmailConfirmed = true,
                        Gender =
                            configuration["User:AdminUser:Gender"]
                            ?? throw new InvalidDataException(
                                "The administrator user gender is not configured."
                            ),
                        Rut =
                            configuration["User:AdminUser:Rut"]
                            ?? throw new InvalidOperationException(
                                "The administrator user RUT is not configured."
                            ),
                        BirthDate = DateTime.Parse(
                            configuration["User:AdminUser:BirthDate"]
                                ?? throw new InvalidOperationException(
                                    "The administrator user birth date is not configured."
                                )
                        ),
                        PhoneNumber =
                            configuration["User:AdminUser:PhoneNumber"]
                            ?? throw new InvalidOperationException(
                                "The administrator user phone number is not configured."
                            ),
                    };
                    adminUser.UserName = adminUser.Email;
                    var adminPassword =
                        configuration["User:AdminUser:Password"]
                        ?? throw new InvalidOperationException(
                            "The administrator user password is not configured."
                        );
                    var adminResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (adminResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(
                            adminUser,
                            adminRole.Name!
                        );
                        if (!roleResult.Succeeded)
                        {
                            Log.Error(
                                "Error asignando rol de administrador: {Errors}",
                                string.Join(", ", roleResult.Errors.Select(e => e.Description))
                            );
                            throw new InvalidOperationException(
                                "Could not assign the administrator role to the user."
                            );
                        }
                        Log.Information("Administrator user created successfully.");
                    }
                    else
                    {
                        Log.Error(
                            "Error creando usuario administrador: {Errors}",
                            string.Join(", ", adminResult.Errors.Select(e => e.Description))
                        );
                        throw new InvalidOperationException(
                            "Could not create the administrator user."
                        );
                    }
                    // Fixed user creation for testing
                    var testUser = new User
                    {
                        FirstName = "Juan",
                        LastName = "Pérez",
                        Email = "juan.perez@example.com",
                        EmailConfirmed = false, // Para probar el resend verification
                        Gender = "Masculino",
                        Rut = "12345678-9",
                        BirthDate = new DateTime(1990, 1, 1),
                        PhoneNumber = "+569 12345678",
                        UserName = "juan.perez@example.com",
                        RegisteredAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        VerificationCode = "123456", // Código de verificación fijo para pruebas
                        VerificationCodeExpires = DateTime.Now.AddHours(24), // Expira en 24 horas
                    };

                    var testPassword = "Test123!";
                    var testResult = await userManager.CreateAsync(testUser, testPassword);
                    if (testResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(
                            testUser,
                            customerRole.Name!
                        );
                        if (!roleResult.Succeeded)
                        {
                            Log.Error(
                                "Error assigning role to test user: {Errors}",
                                string.Join(", ", roleResult.Errors.Select(e => e.Description))
                            );
                        }
                        else
                        {
                            Log.Information("Test user created: juan.perez@example.com");
                        }
                    }
                    else
                    {
                        Log.Error(
                            "Error creando usuario de prueba: {Errors}",
                            string.Join(", ", testResult.Errors.Select(e => e.Description))
                        );
                    }

                    // Creación de usuarios aleatorios
                    var randomPassword =
                        configuration["User:RandomUserPassword"]
                        ?? throw new InvalidOperationException(
                            "The password for random users is not configured."
                        );

                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                        .RuleFor(u => u.LastName, f => f.Name.LastName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.EmailConfirmed, f => true)
                        .RuleFor(u => u.Gender, f => f.PickRandom(genders))
                        .RuleFor(u => u.Rut, f => RandomRut())
                        .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                        .RuleFor(u => u.PhoneNumber, f => RandomPhoneNumber())
                        .RuleFor(u => u.UserName, (f, u) => u.Email);
                    var users = userFaker.Generate(98); // Reducido a 98 para incluir el usuario fijo
                    foreach (var user in users)
                    {
                        var result = await userManager.CreateAsync(user, randomPassword);

                        if (result.Succeeded)
                        {
                            var roleResult = await userManager.AddToRoleAsync(
                                user,
                                customerRole.Name!
                            );
                            if (!roleResult.Succeeded)
                            {
                                Log.Error(
                                    "Error assigning role to {Email}: {Errors}",
                                    user.Email,
                                    string.Join(", ", roleResult.Errors.Select(e => e.Description))
                                );
                                throw new InvalidOperationException(
                                    $"Could not assign the customer role to user {user.Email}."
                                );
                            }
                        }
                        else
                        {
                            Log.Error(
                                "Error creating user {Email}: {Errors}",
                                user.Email,
                                string.Join(", ", result.Errors.Select(e => e.Description))
                            );
                        }
                    }
                    Log.Information("Users created successfully.");
                }

                // Product creation
                if (!await context.Products.AnyAsync())
                {
                    var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
                    var brandIds = await context.Brands.Select(b => b.Id).ToListAsync();

                    if (categoryIds.Any() && brandIds.Any())
                    {
                        var productFaker = new Faker<Product>()
                            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                            .RuleFor(p => p.Price, f => f.Random.Int(1000, 100000))
                            .RuleFor(p => p.Stock, f => f.Random.Int(1, 100))
                            .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                            .RuleFor(p => p.BrandId, f => f.PickRandom(brandIds))
                            .RuleFor(p => p.Status, f => "Nuevo");

                        var products = productFaker.Generate(50);
                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();
                        Log.Information("Products created successfully.");
                    }
                    //pendiente*****
                    // Creación de imágenes:
                    //if (!await context.Images.AnyAsync())
                    //{
                    //    var productIds = await context.Products.Select(p => p.Id).ToListAsync();
                    //    var imageFaker = new Faker<Image>()
                    //        .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl())
                    //        .RuleFor(i => i.PublicId, f => f.Random.Guid().ToString())
                    //        .RuleFor(i => i.ProductId, f => f.PickRandom(productIds));

                    //    var images = imageFaker.Generate(20);
                    //    await context.Images.AddRangeAsync(images);
                    //    await context.SaveChangesAsync();
                    //    Log.Information("Imágenes creadas con éxito.");
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing the database: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Method to generate a random Chilean RUT.
        /// </summary>
        /// <returns>A RUT in format "XXXXXXXX-X".</returns>
        private static string RandomRut()
        {
            var faker = new Faker();
            var rut = faker.Random.Int(1000000, 99999999).ToString();
            var dv = faker.Random.Int(0, 9).ToString();
            return $"{rut}-{dv}";
        }

        /// <summary>
        /// Method to generate a random Chilean phone number.
        /// </summary>
        /// <returns>A phone number in format "+569 XXXXXXXX".</returns>
        private static string RandomPhoneNumber()
        {
            var faker = new Faker();
            string firstPartNumber = faker.Random.Int(1000, 9999).ToString();
            string secondPartNumber = faker.Random.Int(1000, 9999).ToString();
            return $"+569 {firstPartNumber}{secondPartNumber}";
        }
    }
}
