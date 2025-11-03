# TiendaUCN API

A comprehensive e-commerce API built with ASP.NET Core, designed for managing an online store for the Universidad Católica del Norte (UCN). This project provides a full-featured backend for product management, user authentication, order processing, and administrative functions.

## Features

### Core Functionality
- **User Management**: Registration, authentication, and role-based access control
- **Product Catalog**: Comprehensive product management with categories and brands
- **Order Processing**: Complete order lifecycle management
- **Admin Panel**: Administrative functions for managing users, products, categories, and orders
- **Email Services**: Automated email notifications using Resend
- **Background Jobs**: Scheduled tasks for maintenance using Hangfire

### Technical Features
- **JWT Authentication**: Secure token-based authentication
- **SQLite Database**: Lightweight, file-based database
- **Serilog Logging**: Structured logging for monitoring and debugging
- **OpenAPI Documentation**: Interactive API documentation with Swagger
- **Hangfire Dashboard**: Background job monitoring and management

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog
- **Background Jobs**: Hangfire
- **Email Service**: Resend API
- **Documentation**: OpenAPI/Swagger

## Project Structure

```
src/
├── Api/
│   └── Controllers/
│       ├── Admin/          # Administrative controllers
│       └── AuthController.cs
├── Application/
│   ├── DTO/                # Data Transfer Objects
│   ├── Exceptions/         # Custom exceptions
│   ├── Jobs/               # Background job interfaces
│   └── Services/           # Business logic services
├── Domain/
│   └── Models/             # Entity models
└── Infrastructure/
    ├── Data/               # Database context and seeding
    └── Repositories/       # Data access layer
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd TiendaUCN
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Configure the application:
   - Copy `appsettings.json` and modify as needed
   - Ensure the database connection string is correct
   - Configure JWT settings, email service, and other environment-specific values

4. Run database migrations:
```bash
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or the configured port).

## Configuration

### Key Configuration Sections

#### Database
```json
{
  "ConnectionStrings": {
    "SqliteDatabase": "Data Source=app.db"
  }
}
```

#### JWT Authentication
```json
{
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "TiendaUCN",
    "Audience": "TiendaUCN"
  }
}
```

#### Email Service (Resend)
```json
{
  "ResendAPIKey": "your-resend-api-key"
}
```

#### Hangfire Dashboard
```json
{
  "HangfireDashboard": {
    "DashboardPath": "/hangfire",
    "StatsPollingInterval": 2000,
    "DashboardTitle": "TiendaUCN Jobs",
    "DisplayStorageConnectionString": false
  }
}
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/verify-email` - Email verification

### Public Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID

### Admin Endpoints
- `GET /api/admin/products` - Manage products
- `GET /api/admin/users` - Manage users
- `GET /api/admin/orders` - Manage orders
- `GET /api/admin/categories` - Manage categories

## Background Jobs

The application includes scheduled background jobs:
- **User Cleanup**: Removes unconfirmed users after a specified period

Jobs are configured in `Program.cs` and can be monitored via the Hangfire dashboard.

## Development

### Running in Development Mode
```bash
dotnet run --environment Development
```

### API Documentation
When running in development, visit `https://localhost:5001/swagger` for interactive API documentation.

### Hangfire Dashboard
Access the background job dashboard at `https://localhost:5001/hangfire` (configured path).

## Testing

### Running Tests
```bash
dotnet test
```

### HTTP File Testing
Use the included `TiendaUCN.http` file for testing API endpoints directly from VS Code.

## Deployment

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### Environment Variables
Configure the following environment variables for production:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__SqliteDatabase=Data Source=/path/to/prod.db`
- `Jwt__Key=your-production-jwt-key`
- `ResendAPIKey=your-production-resend-key`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please open an issue in the repository or contact the development team.
