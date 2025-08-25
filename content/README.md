# MyWebApi

A .NET 9 Web API built with Vertical Slice Architecture (VSA).

## Features

- **Vertical Slice Architecture**: Features organized by use case rather than technical concerns
- **Minimal APIs**: Lightweight endpoint definitions with built-in validation
- **PostgreSQL**: Production-ready database with Entity Framework Core
- **ASP.NET Core Identity**: Complete authentication system with:
  - User registration and email confirmation
  - Login/logout with cookie authentication
  - Password reset functionality
  - Two-factor authentication (2FA)
  - Role-based authorization
- **Docker Support**: Ready-to-run with docker-compose
- **Structured Logging**: Serilog integration
- **Request Validation**: FluentValidation integration
- **API Documentation**: Swagger/OpenAPI support

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose

### Running with Docker

1. Clone and navigate to the project:
```bash
git clone <repository-url>
cd MyWebApi
```

2. Start the application:
```bash
docker-compose up -d
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: http://localhost:5001
- Scalar UI: http://localhost:5000/

### Running Locally

1. Update connection strings in `appsettings.Development.json`

2. Install EF Core tools:
```bash
dotnet tool install --global dotnet-ef
```

3. Run database migrations:
```bash
cd src/MyWebApi
dotnet ef database update --context AppDbContext
dotnet ef database update --context IdentityDbContext
```

4. Run the application:
```bash
dotnet run
```

## Architecture

This project follows Vertical Slice Architecture principles:

### Project Structure

```
src/MyWebApi/
├── Authentication/          # Authentication feature
│   ├── Data/               # Identity database context
│   ├── Endpoints/          # Auth-related endpoints
│   ├── Models/             # User models
│   └── Services/           # Email service, etc.
├── Common/                 # Shared utilities
│   ├── Api/                # API infrastructure
│   └── Extensions/         # Extension methods
├── Data/                   # Main application data
│   ├── Types/              # Entity interfaces
│   └── AppDbContext.cs     # Main database context
├── Features/               # Your business features go here
└── Program.cs              # Application entry point
```

### Key Principles

1. **Feature Organization**: Each feature is self-contained with its own endpoints, models, and logic
2. **Minimal APIs**: Lightweight endpoint definitions with integrated validation
3. **Request/Response Contracts**: Each endpoint defines its own contracts
4. **Validation**: FluentValidation for request validation
5. **Database Context Separation**: Identity and business data use separate contexts

## Adding New Features

1. Create a new folder under `Features/` (e.g., `Features/Products/`)
2. Add your endpoints in `Features/Products/Endpoints/`
3. Register endpoints in `Endpoints.cs`
4. Add entities to `AppDbContext` if needed

Example feature endpoint:

```csharp
public class GetProducts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithSummary("Get all products");

    private static async Task<Ok<List<Product>>> Handle(
        AppDbContext database, 
        CancellationToken cancellationToken)
    {
        var products = await database.Products.ToListAsync(cancellationToken);
        return TypedResults.Ok(products);
    }
}
```

## Authentication

The API includes a complete authentication system using ASP.NET Core Identity:

### Available Endpoints

- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `POST /auth/logout` - User logout  
- `GET /auth/confirm-email` - Email confirmation
- `POST /auth/forgot-password` - Request password reset
- `POST /auth/reset-password` - Reset password
- `GET /auth/user-info` - Get current user info
- `POST /auth/enable-2fa` - Enable two-factor authentication
- `POST /auth/disable-2fa` - Disable two-factor authentication

### Roles

The system includes two default roles:
- `Admin` - Administrative access
- `User` - Standard user access

### Default Admin User

A default admin user is created with:
- Email: `AdminEmail`
- Password: `AdminPassword`
- Role: Admin

### Email Service

The template includes an `IEmailService` interface that you need to implement for:
- Email confirmation
- Password reset
- Two-factor authentication codes

Update the `EmailService` class in `Authentication/Services/EmailService.cs` with your preferred email provider.

## Database

The application uses PostgreSQL with Entity Framework Core.

### Migrations

To add a new migration:

```bash
# For main database
dotnet ef migrations add MigrationName --context AppDbContext

# For identity database  
dotnet ef migrations add MigrationName --context IdentityDbContext
```

### Database Schema

- **MyWebApi_Identity**: Contains all ASP.NET Core Identity tables (Users, Roles, Claims, etc.)
- **MyWebApi_Main**: Contains your application's business data

## Configuration

Key configuration sections in `appsettings.json`:

- `ConnectionStrings`: Database connection strings
- `Serilog`: Logging configuration
- `EmailSettings`: Email service configuration

## Contributing

1. Follow the Vertical Slice Architecture principles
2. Each endpoint should be self-contained
3. Use FluentValidation for request validation
4. Write tests for your endpoints
5. Update this README when adding new features

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.