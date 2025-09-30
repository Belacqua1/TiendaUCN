using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserService, UserService>();

builder
    .Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenApi();
#region Loggin Configuration
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.Run();
