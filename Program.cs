using System.Text;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Serilog;
using Tienda_UCN_api.src.Infrastructure.Data;
using TiendaUCN.src.Application.Jobs.Interface;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

/// <summary>
/// Entry point of the Tienda UCN API application.
/// Configures services, database, identity, email service, and the HTTP request pipeline.
/// </summary>
var builder = WebApplication.CreateBuilder(args);
var connectionStrings =
    builder.Configuration.GetConnectionString("SqliteDatabase")
    ?? throw new InvalidOperationException(
        "La cadena de conexion a la base de datos no esta configurada"
    );

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
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connectionStrings));
#endregion

#region Hanfirer Configuration
Log.Information("Configurando los trabajos de segundo plano de Hanfire");
var cronExpression =
    builder.Configuration["Jobs:CronJobDeleteUnconfirmedUsers"]
    ?? throw new InvalidOperationException("La exprecion cron no esta configurada");
#pragma warning disable CS8604 // Possible null reference argument.
var timeZone =
    TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Jobs:TimeZone"])
    ?? throw new InvalidOperationException("La zona horaria para los trabajos no esta configurada");

// Default to daily at midnight if not set
builder.Services.AddHangfire(configuration =>
{
    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionStrings);
    var databasePath = connectionStringBuilder.DataSource;

    configuration.UseSQLiteStorage(databasePath);
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    configuration.UseSimpleAssemblyNameTypeSerializer();
    configuration.UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();
#pragma warning restore CS8604 // Possible null reference argument.
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

#region JWT Authentication Configuration
/// <summary>
/// Configures JWT Bearer authentication for API endpoints.
/// </summary>
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        };
    });
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
builder.Services.AddScoped<IImageService, ImageService>(); // Image management logic
builder.Services.AddScoped<IPublicProductService, PublicProductService>(); // Public product catalog logic
builder.Services.AddScoped<IProductAdminService, ProductAdminService>(); // Admin product management logic
builder.Services.AddScoped<ICategoryAdminService, CategoryAdminService>(); // Admin category management logic
builder.Services.AddScoped<IUserAdminService, UserAdminService>(); // Admin user management logic
builder.Services.AddScoped<IOrderAdminService, OrderAdminService>(); // Admin order management logic
#endregion

#region Application Repositories
builder.Services.AddScoped<
    TiendaUCN.src.Infrastructure.Repositories.Interfaces.IImageRepository,
    TiendaUCN.src.Infrastructure.Repositories.Implements.ImageRepository
>();
#endregion
// Controllers
/// <summary>
/// Adds support for API controllers.
/// </summary>
builder.Services.AddControllers();

var app = builder.Build();

// Hangfire Dashboard

app.UseHangfireDashboard(
    builder.Configuration["HangfireDashboard:DashboardPath"]
        ?? throw new InvalidOperationException(
            "La ruta del dashboard de Hangfire no esta configurado"
        ),
    new DashboardOptions
    {
        StatsPollingInterval =
            builder.Configuration.GetValue<int?>("HangfireDashboard:StatsPollingInterval")
            ?? throw new InvalidOperationException(
                "El intervalo de actualización de estadísticas del panel de control de Hangfire no está configurado."
            ),
        DashboardTitle =
            builder.Configuration["HangfireDashboard:DashboardTitle"]
            ?? throw new InvalidOperationException(
                "El título del panel de control de Hangfire no está configurado."
            ),
        DisplayStorageConnectionString =
            builder.Configuration.GetValue<bool?>(
                "HangfireDashboard:DisplayStorageConnectionString"
            )
            ?? throw new InvalidOperationException(
                "La configuración 'HangfireDashboard:DisplayStorageConnectionString' no está definida."
            ),
    }
);

#region Database Seeder and Jobs Setup
/// <summary>
/// Applies database migrations and seeds initial data.
/// </summary>
Log.Information("Aplicando migraciones a la base de datos");
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
    var jobId = nameof(UserJob.DeleteUnconfirmedAsync);
    RecurringJob.AddOrUpdate<IUserJob>(
        jobId,
        job => job.DeleteUnconfirmedAsync(),
        cronExpression,
        new RecurringJobOptions { TimeZone = timeZone }
    );
    Log.Information(
        "Trabajo recurrente '{JobId}' configurado con expresión CRON '{CronExpression}' en zona horaria '{TimeZone}'",
        jobId,
        cronExpression,
        timeZone.Id
    );
}
#endregion

#region HTTP Request Pipeline
/// <summary>
/// Configures the HTTP request pipeline.
/// Enables OpenAPI in development and maps controllers.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // OpenAPI UI only in development
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // Map API controller endpoints
app.Run(); // Run the application
#endregion
