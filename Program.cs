using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Tienda_UCN_api.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Implements; // <-- Asegúrate de usar tu namespace correcto
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

#region Logging Configuration
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);
#endregion

#region Database Configuration
Log.Information("Configurando base de datos SQlite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value)
);
#endregion

#region Identity Configuration
Log.Information("Configurando Identity");
builder
    .Services.AddIdentity<User, Role>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
#endregion

#region Application Services
// Registrar tus servicios de aplicación aquí
builder.Services.AddScoped<IUserService, UserService>();
#endregion

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

#region Database Seeder
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
