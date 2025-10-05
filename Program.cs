using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Application.Services.Implements;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using Hangfire;
using Hangfire.SQLite;
using TiendaUCN.src.Application.Jobs.Interfaces;
using TiendaUCN.src.Application.Jobs.Implements;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SqliteDatabase") ?? throw new InvalidOperationException("Connection string SqliteDatabase no configurado");
// Add services to the container.

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IFileService, FileService>();


builder.Services.AddScoped<IUserJob, UserJob>();
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
    var jobId = nameof(UserJob.DeleteUnconfirmedAsync);
    RecurringJob.AddOrUpdate<UserJob>(
        jobId,
        job => job.DeleteUnconfirmedAsync(),
        cronExpression,
        new RecurringJobOptions
        {
            TimeZone = timeZone
        }
    );
    Log.Information($"Job recurrente '{jobId}' configurado con cron: {cronExpression} en zona horaria: {timeZone.Id}");
    //MapperExtensions.ConfigureMapster(scope.ServiceProvider);
}
#endregion
// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
