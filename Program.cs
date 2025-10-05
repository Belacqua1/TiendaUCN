using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using Hangfire;
using Hangfire.SQLite;

using Microsoft.Data.Sqlite;

/// <summary>
/// Entry point of the Tienda UCN API application.
/// Configures services, database, identity, email service, and the HTTP request pipeline.
/// </summary>
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SqliteDatabase") ?? throw new InvalidOperationException("Connection string SqliteDatabase no configurado");
// Add services to the container.

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IFileService, FileService>();

// OpenAPI
#region OpenAPI Configuration
/// <summary>
/// Adds OpenAPI (Swagger) support for automatic API documentation.
/// </summary>
builder.Services.AddOpenApi();
#endregion

#region Logging Configuration
/// <summary>
/// Configures Serilog as the logging provider.
/// Reads settings from configuration and integrates with dependency injection.
/// </summary>
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);
#endregion

#region Database Configuration
/// <summary>
/// Configures the application database context to use SQLite.
/// The connection string is read from appsettings.json.
/// </summary>
Log.Information("Configurando base de datos SQLite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDatabase"))
);
#endregion

#region Identity Configuration
/// <summary>
/// Configures ASP.NET Core Identity for user and role management.
/// Customizes password requirements and ensures unique email addresses.
/// </summary>
Log.Information("Configurando Identity");
builder
    .Services.AddIdentity<User, Role>(options =>
    {
        options.User.RequireUniqueEmail = true; // Ensure unique email addresses for users
        options.Password.RequireDigit = false; // No digit required in password
        options.Password.RequiredLength = 6; // Minimum password length
        options.Password.RequireNonAlphanumeric = false; // No special character required
        options.Password.RequireUppercase = false; // Uppercase not required
    })
    .AddEntityFrameworkStores<DataContext>() // Use EF Core for identity persistence
    .AddDefaultTokenProviders(); // Provides tokens for password reset, email confirmation, etc.
#endregion

#region Email Service Configuration
/// <summary>
/// Configures the Resend email service for sending verification and welcome emails.
/// </summary>
Log.Information("Configurando servicio de Email");
builder.Services.AddOptions(); // Enable options pattern
builder.Services.AddHttpClient<ResendClient>(); // Register HTTP client for Resend
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken =
        builder.Configuration["ResendAPIKey"] // Read API token from configuration
        ?? throw new InvalidOperationException("El token de API de Resend no está configurado.");
});
builder.Services.AddTransient<IResend, ResendClient>(); // Inject Resend client
#endregion

#region Application Services
/// <summary>
/// Registers application-level services for dependency injection.
/// </summary>
builder.Services.AddMemoryCache(); // In-memory caching support
builder.Services.AddScoped<IEmailService, EmailService>(); // Email operations
builder.Services.AddScoped<IVerificationService, VerificationService>(); // Verification logic
builder.Services.AddScoped<IUserService, UserService>(); // User management logic
builder.Services.AddScoped<IAuthService, AuthService>(); // Authentication logic
#endregion

// Controllers
/// <summary>
/// Adds support for API controllers.
/// </summary>
builder.Services.AddControllers();
#region Hangfire Configuration
Log.Information("Configurando los trabajos en segundo plano de Hangfire");
var cronExpression = builder.Configuration["Jobs:CronJobDeleteUnconfirmedUsers"] ?? throw new InvalidOperationException("La expresión cron para eliminar usuarios no confirmados no está configurada.");
var timeZone = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Jobs:TimeZone"] ?? throw new InvalidOperationException("La zona horaria para los trabajos no está configurada."));
builder.Services.AddHangfire(configuration =>
{
    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);
    var databasePath = connectionStringBuilder.DataSource;
    configuration.UseSQLiteStorage(databasePath);
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    configuration.UseSimpleAssemblyNameTypeSerializer();
    configuration.UseRecommendedSerializerSettings();
    configuration.UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();


#endregion

var app = builder.Build();

app.UseHangfireDashboard(builder.Configuration["HangfireDashboard:DashboardPath"] ?? throw new InvalidOperationException("La ruta de hangfire no ha sido declarada"), new DashboardOptions
{
    StatsPollingInterval = builder.Configuration.GetValue<int?>("HangfireDashboard:StatsPollingInterval") ?? throw new InvalidOperationException("El intervalo de actualización de estadísticas del panel de control de Hangfire no está configurado."),
    DashboardTitle = builder.Configuration["HangfireDashboard:DashboardTitle"] ?? throw new InvalidOperationException("El título del panel de control de Hangfire no está configurado."),
    DisplayStorageConnectionString = builder.Configuration.GetValue<bool?>("HangfireDashboard:DisplayStorageConnectionString") ?? throw new InvalidOperationException("La configuración 'HangfireDashboard:DisplayStorageConnectionString' no está definida."),
});



#region Database Seeder
/// <summary>
/// Applies database migrations and seeds initial data.
/// </summary>
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}
#endregion
#region Database Migration and jobs Configuration
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}    
#endregion
// Configure HTTP request pipeline

#region HTTP Request Pipeline
/// <summary>
/// Configures the HTTP request pipeline.
/// Enables OpenAPI in development and maps controllers.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // OpenAPI UI only in development
}

app.MapControllers(); // Map API controller endpoints
app.Run(); // Run the application
#endregion
