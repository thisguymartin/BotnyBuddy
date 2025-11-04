# BotanicalBuddy.NET

A plant tracking and identification API built with **ASP.NET Core 8.0**, integrating with the Trefle.io plant database.

## Features

- **Trefle API Integration**: Access to 500,000+ plant species data
- **JWT Authentication**: Secure API authentication with Bearer tokens
- **Plant Search**: Search plants by name, common name, or scientific name
- **Plant Details**: Get comprehensive information about specific plants
- **RESTful API**: Clean and well-documented REST endpoints with Swagger UI
- **Built with .NET 8.0**: Modern, high-performance web API

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Trefle.io API token ([get one here](https://trefle.io/))

### Setup

1. **Configure settings**:
   Edit `BotanicalBuddy.API/appsettings.json`:
   ```json
   {
     "Jwt": {
       "Secret": "your-secret-key-at-least-32-characters-long"
     },
     "Auth": {
       "ApiKey": "demo-api-key"
     },
     "Trefle": {
       "ApiToken": "your-trefle-api-token-here"
     }
   }
   ```

2. **Run the application**:
   ```bash
   cd BotanicalBuddy.API
   dotnet restore
   dotnet run
   ```

3. **Access Swagger UI**:
   Navigate to `http://localhost:5000` to see the interactive API documentation.

## API Documentation

### Authentication Endpoints

**POST** `/api/auth/token` - Generate JWT token
**POST** `/api/auth/refresh` - Refresh JWT token
**GET** `/api/auth/verify` - Verify JWT token

### Plant Endpoints (JWT Required)

**GET** `/api/plants` - List all plants (paginated)
**GET** `/api/plants/search?q={query}` - Search plants
**GET** `/api/plants/{id}` - Get plant details by ID
**GET** `/api/plants/filter/common-name?name={name}` - Filter by common name

For detailed API documentation with examples, see [API_DOCS.md](./API_DOCS.md)

## Tech Stack

- **ASP.NET Core 8.0**: Web API framework
- **C# 12**: Programming language
- **JWT Bearer Authentication**: Secure token-based auth
- **Trefle.io API**: Comprehensive plant database
- **Swagger/OpenAPI**: Interactive API documentation
- **Newtonsoft.Json**: JSON serialization

## Project Structure

```
BotanicalBuddy.API/
├── Controllers/         # API controllers
│   ├── AuthController.cs
│   └── PlantsController.cs
├── Services/           # Business logic services
│   ├── TrefleApiService.cs
│   └── JwtTokenService.cs
├── Models/            # Data models
│   ├── TrefleModels.cs
│   ├── AuthModels.cs
│   └── ApiResponse.cs
├── Program.cs         # Application entry point
└── appsettings.json   # Configuration
```

## Configuration

### JWT Settings
- `Jwt:Secret`: Secret key for JWT token signing (min 32 chars)
- `Jwt:Issuer`: Token issuer name
- `Jwt:Audience`: Token audience name

### Authentication
- `Auth:ApiKey`: Simple API key for token generation

### Trefle API
- `Trefle:ApiToken`: Your Trefle.io API token

## Example Usage

```bash
# 1. Get JWT token
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test_user", "apiKey": "demo-api-key"}'

# 2. Search for plants (use token from step 1)
curl -X GET "http://localhost:5000/api/plants/search?q=rose" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# 3. Get plant details
curl -X GET "http://localhost:5000/api/plants/123456" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Development

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run in development mode
dotnet run --project BotanicalBuddy.API

# Run tests (when available)
dotnet test
```

## License

MIT
