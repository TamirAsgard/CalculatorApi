# Calculator API

A production-ready ASP.NET Core 8.0 REST API for mathematical calculations with JWT authentication, built with MongoDB and Redis.

## Features

- ✅ **RESTful API** - Mathematical operations (add, subtract, multiply, divide)
- 🔐 **JWT Authentication** - Secure token-based authentication
- 📊 **MongoDB** - User data persistence
- ⚡ **Redis** - Token caching for performance
- 📝 **Serilog** - Structured logging
- 🧪 **Comprehensive Testing** - 41 unit and integration tests
- 🐳 **Docker Support** - Development and production environments
- 📖 **Swagger/OpenAPI** - Interactive API documentation (dev only)

## Quick Start

### Using Docker (Recommended)

**Development:**
```bash
docker-compose -f docker-compose.dev.yml up -d
```
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

**Production:**
```bash
docker-compose up -d
```
- API: http://localhost:5001
- MongoDB: admin/admin
- Redis password: admin

See [DOCKER.md](DOCKER.md) for detailed Docker instructions.

### Local Development

**Prerequisites:**
- .NET 8.0 SDK
- MongoDB (localhost:27017)
- Redis (localhost:6379)

**Run:**
```bash
dotnet restore
dotnet run --project src/IO.Swagger
```

## API Endpoints

### Authentication
- `POST /v1/auth/login` - Login or register user

### Calculations
- `POST /v1/calculations` - Perform calculation
  - Header: `X-Operation: add|subtract|multiply|divide`
  - Body: `{ "number1": 5.0, "number2": 3.0 }`

### Development Only
- `GET /v1/dev/health` - Health check
- `GET /v1/dev/config` - Configuration status

## Architecture

This project follows clean architecture principles with clear separation of concerns:

- **Controllers** - HTTP request handling
- **Services** - Business logic
- **Repositories** - Data access
- **Infrastructure** - External dependencies (MongoDB, Redis)
- **Security** - Authentication and password hashing

## Configuration

Configuration is managed through `appsettings.json` and environment variables:

```json
{
  "Jwt": {
    "Issuer": "CalculatorApi",
    "Audience": "CalculatorApiClients",
    "SigningKey": "your-secret-key-here",
    "ExpirationMinutes": 30
  },
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "calculator_dev",
    "UsersCollection": "users"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstancePrefix": "calcapi:dev:"
  }
}
```

## Testing

**Run all tests:**
```bash
dotnet test
```

**Test Coverage:**
- 29 Unit Tests (PasswordHasher, CalculatorService, AuthService)
- 12 Integration Tests (API endpoints)
- See [IO.Swagger.Tests/UT.md](IO.Swagger.Tests/UT.md) for detailed test documentation

## Security

- ✅ **Password Hashing** - PBKDF2-SHA256 with unique salts
- ✅ **JWT Tokens** - Secure token-based authentication
- ✅ **Token Caching** - Redis-based token reuse
- ✅ **Environment-based Config** - Secrets via environment variables
- ✅ **Production Hardening** - Swagger disabled, secure defaults

## Project Structure

```
aspnetcore-server-generated/
├── src/IO.Swagger/
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Infrastructure/       # Data access & external services
│   │   ├── Context/          # MongoDB context
│   │   ├── Entities/         # Database models
│   │   └── Repositories/     # Data repositories
│   ├── Security/             # Authentication & hashing
│   ├── Configuration/        # Configuration classes
│   ├── Extensions/           # Service extensions
│   ├── Models/               # DTOs and request/response models
│   └── Exceptions/           # Custom exceptions
├── IO.Swagger.Tests/
│   ├── Unit/                 # Unit tests
│   └── Integration/          # Integration tests
├── docker-compose.yml        # Production Docker setup
├── docker-compose.dev.yml    # Development Docker setup
├── DOCKER.md                 # Docker documentation
├── ARCHITECTURE.md           # Architecture documentation
└── README.md                 # This file
```

## Development

**Build:**
```bash
dotnet build
```

**Run with hot reload:**
```bash
dotnet watch run --project src/IO.Swagger
```

**Format code:**
```bash
dotnet format
```

## Deployment

### Docker (Recommended)

See [DOCKER.md](DOCKER.md) for complete Docker deployment instructions.

### Manual Deployment

1. Update `appsettings.Production.json` with production values
2. Build: `dotnet publish -c Release -o ./publish`
3. Deploy the `publish` folder to your server
4. Set environment variables for secrets
5. Run: `dotnet IO.Swagger.dll`

## License

This project is licensed under the MIT License.

## Documentation

- [DOCKER.md](DOCKER.md) - Docker setup and deployment
- [IO.Swagger.Tests/UT.md](IO.Swagger.Tests/UT.md) - Test documentation