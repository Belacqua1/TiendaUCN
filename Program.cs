using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using Tienda_UCN_api.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Implements;
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
Log.Information("Configurando base de datos SQLite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDatabase"))
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

#region Email Service Configuration
Log.Information("Configurando servicio de Email");
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken =
        builder.Configuration["ResendAPIKey"]
        ?? throw new InvalidOperationException("El token de API de Resend no está configurado.");
});
builder.Services.AddTransient<IResend, ResendClient>();
#endregion

#region Application Services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
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
