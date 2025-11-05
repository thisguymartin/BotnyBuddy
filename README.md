# BotanicalBuddy 🌱

A comprehensive plant care and tracking SaaS platform built with **ASP.NET Core 8.0**. Track your plant collection, get location-based care recommendations, and learn optimal watering schedules based on local weather patterns.

## Features

### Core Features (Implemented)

- **User Authentication & Management**
  - User registration with email and password
  - Secure login with BCrypt password hashing
  - JWT-based authentication with subscription tier claims
  - User profile management

- **Plant Collection Management**
  - Track unlimited plants (tier-dependent)
  - Add plants with photos, nicknames, and notes
  - Link plants to specific addresses
  - Integration with Trefle.io for 500,000+ plant species

- **Address Management**
  - Multiple address support per user
  - Geolocation support (latitude/longitude)
  - Weather tracking per location

- **Plant Care Logging**
  - Track watering, fertilizing, pruning, and custom care events
  - Historical care logs with timestamps
  - Care statistics and patterns

- **Weather Integration**
  - OpenWeatherMap API integration
  - Automatic weather data caching
  - Historical weather data storage
  - Location-based weather tracking

- **Performance & Caching**
  - In-memory caching for Trefle API responses (24h TTL)
  - Weather data caching (1h TTL)
  - Optimized database queries with Entity Framework Core

- **Subscription Tiers**
  - **Free**: 5 plants max, basic care tracking
  - **Basic**: 25 plants, weather integration ($4.99/mo)
  - **Premium**: Unlimited plants, advanced analytics ($14.99/mo)

- **Production Ready**
  - Docker containerization
  - Fly.io deployment configuration
  - PostgreSQL database with Entity Framework Core
  - Health check endpoints
  - Comprehensive API documentation with Swagger

### Planned Features (Future)

- Stripe payment integration
- Email verification and notifications
- Advanced plant care recommendations
- Pattern analysis and AI-powered insights
- Photo upload and storage (cloud)
- Rate limiting middleware
- Analytics dashboard
- Mobile app support

See the complete [transformation plan](./CLAUDE_PLAN.md) for detailed feature roadmap.

## Quick Start

### Option 1: Docker Compose (Recommended)

The easiest way to get started without installing PostgreSQL locally:

```bash
# 1. Start PostgreSQL with Docker
docker-compose up -d postgres

# 2. Run migrations
cd BotanicalBuddy.API
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. Run the API
dotnet run

# 4. Access Swagger UI at https://localhost:5001
```

See [DOCKER_QUICKSTART.md](./DOCKER_QUICKSTART.md) for detailed instructions.

### Option 2: Manual Setup

#### Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 15+ installed locally
- Trefle.io API token ([get one here](https://trefle.io/))
- OpenWeatherMap API key ([get one here](https://openweathermap.org/api))

#### Setup

1. **Create database**:
   ```bash
   createdb botanicalbuddy
   ```

2. **Configure settings**:
   Edit `BotanicalBuddy.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=botanicalbuddy;Username=postgres;Password=yourpassword"
     },
     "Jwt": {
       "Secret": "your-secret-key-at-least-32-characters-long"
     },
     "Trefle": {
       "ApiToken": "your-trefle-api-token-here"
     },
     "WeatherApi": {
       "ApiKey": "your-weather-api-key-here"
     }
   }
   ```

3. **Run migrations**:
   ```bash
   cd BotanicalBuddy.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**:
   Navigate to `https://localhost:5001` to see the interactive API documentation.

## API Documentation

### Authentication Endpoints

- **POST** `/api/auth/register` - Register a new user
- **POST** `/api/auth/login` - Login with email and password
- **GET** `/api/auth/me` - Get current user profile (requires auth)
- **POST** `/api/auth/token` - Generate JWT token (legacy)
- **POST** `/api/auth/refresh` - Refresh JWT token
- **GET** `/api/auth/verify` - Verify JWT token

### User Plants Endpoints (JWT Required)

- **GET** `/api/userplants` - Get all user's plants
- **GET** `/api/userplants/{id}` - Get specific plant
- **POST** `/api/userplants` - Create new plant
- **PUT** `/api/userplants/{id}` - Update plant
- **DELETE** `/api/userplants/{id}` - Delete plant

### Addresses Endpoints (JWT Required)

- **GET** `/api/addresses` - Get all user's addresses
- **GET** `/api/addresses/{id}` - Get specific address
- **POST** `/api/addresses` - Create new address
- **PUT** `/api/addresses/{id}` - Update address
- **DELETE** `/api/addresses/{id}` - Delete address

### Plant Care Logs Endpoints (JWT Required)

- **GET** `/api/plantcarelogs/plant/{plantId}` - Get all care logs for a plant
- **GET** `/api/plantcarelogs/{id}` - Get specific care log
- **POST** `/api/plantcarelogs` - Create new care log
- **DELETE** `/api/plantcarelogs/{id}` - Delete care log
- **GET** `/api/plantcarelogs/plant/{plantId}/statistics` - Get care statistics

### Trefle Plant Database Endpoints (JWT Required)

- **GET** `/api/plants` - List all plants (paginated)
- **GET** `/api/plants/search?q={query}` - Search plants
- **GET** `/api/plants/{id}` - Get plant details by ID
- **GET** `/api/plants/filter/common-name?name={name}` - Filter by common name

### Health Check

- **GET** `/health` - Application health status

For detailed API documentation with examples, see the Swagger UI at `/` when running the application.

## Tech Stack

### Backend
- **ASP.NET Core 8.0**: Web API framework
- **C# 12**: Programming language
- **Entity Framework Core 8.0**: ORM for database access
- **PostgreSQL**: Production database
- **BCrypt.Net**: Password hashing
- **JWT Bearer Authentication**: Secure token-based auth

### External Services
- **Trefle.io API**: 500,000+ plant species database
- **OpenWeatherMap API**: Weather data integration

### Infrastructure
- **Docker**: Containerization
- **Fly.io**: Cloud hosting platform
- **In-Memory Cache**: Response caching

### Developer Tools
- **Swagger/OpenAPI**: Interactive API documentation
- **Newtonsoft.Json**: JSON serialization

## Project Structure

```
BotanicalBuddy.API/
├── Controllers/              # API controllers
│   ├── AuthController.cs           # Authentication & user management
│   ├── PlantsController.cs         # Trefle plant database
│   ├── UserPlantsController.cs     # User plant collection
│   ├── AddressesController.cs      # Address management
│   ├── PlantCareLogsController.cs  # Plant care tracking
│   └── HealthController.cs         # Health checks
├── Services/                 # Business logic services
│   ├── TrefleApiService.cs         # Trefle API integration
│   ├── CachedTrefleApiService.cs   # Cached Trefle API
│   ├── JwtTokenService.cs          # JWT token generation
│   ├── UserService.cs              # User management
│   └── WeatherService.cs           # Weather API integration
├── Data/                     # Database layer
│   ├── ApplicationDbContext.cs     # EF Core context
│   └── Entities/                   # Database entities
│       ├── User.cs
│       ├── Address.cs
│       ├── UserPlant.cs
│       ├── PlantCareLog.cs
│       ├── WeatherData.cs
│       └── Subscription.cs
├── Models/                   # DTOs and API models
│   ├── TrefleModels.cs
│   ├── AuthModels.cs
│   ├── PlantModels.cs
│   └── ApiResponse.cs
├── Program.cs                # Application entry point
└── appsettings.json          # Configuration
```

## Configuration

### Database
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection string

### JWT Settings
- `Jwt:Secret`: Secret key for JWT token signing (min 32 chars)
- `Jwt:Issuer`: Token issuer name
- `Jwt:Audience`: Token audience name

### External APIs
- `Trefle:ApiToken`: Your Trefle.io API token ([get one here](https://trefle.io/))
- `WeatherApi:ApiKey`: OpenWeatherMap API key ([get one here](https://openweathermap.org/api))
- `WeatherApi:BaseUrl`: Weather API base URL

### Legacy Authentication
- `Auth:ApiKey`: Simple API key for legacy token generation endpoint

## Example Usage

```bash
# 1. Register a new user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# 2. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'

# 3. Create an address (use token from login)
curl -X POST http://localhost:5000/api/addresses \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "addressLine1": "123 Main St",
    "city": "Seattle",
    "state": "WA",
    "country": "USA",
    "postalCode": "98101"
  }'

# 4. Add a plant to your collection
curl -X POST http://localhost:5000/api/userplants \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "commonName": "Monstera",
    "scientificName": "Monstera deliciosa",
    "nickname": "My Monstera",
    "addressId": "ADDRESS_ID_FROM_STEP_3",
    "datePlanted": "2024-01-15",
    "location": "Living room"
  }'

# 5. Log plant care
curl -X POST http://localhost:5000/api/plantcarelogs \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "userPlantId": "PLANT_ID_FROM_STEP_4",
    "careType": "Watering",
    "amount": "250ml",
    "notes": "Regular watering"
  }'

# 6. Search Trefle plant database
curl -X GET "http://localhost:5000/api/plants/search?q=rose" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Development

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run database migrations
cd BotanicalBuddy.API
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run in development mode
dotnet run --project BotanicalBuddy.API

# Run tests (when available)
dotnet test
```

### Docker Development

```bash
# Build Docker image
docker build -t botanicalbuddy .

# Run with Docker Compose (with PostgreSQL)
docker-compose up
```

## Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for comprehensive deployment instructions including:
- Local development setup
- Fly.io deployment
- Database migrations
- Environment variables
- Monitoring and scaling
- CI/CD with GitHub Actions

## Subscription Tiers

| Feature | Free | Basic ($4.99/mo) | Premium ($14.99/mo) |
|---------|------|------------------|---------------------|
| Max Plants | 5 | 25 | Unlimited |
| Care Tracking | ✓ | ✓ | ✓ |
| Weather Integration | ✗ | ✓ | ✓ |
| Watering Recommendations | Basic | Advanced | AI-Powered |
| Pattern Analysis | ✗ | ✗ | ✓ |
| Historical Data | 30 days | 6 months | Unlimited |
| Photo Storage | 1 per plant | 5 per plant | Unlimited |
| API Rate Limit | 100/day | 1000/day | 10000/day |
| Priority Support | ✗ | ✗ | ✓ |

## Contributing

Contributions are welcome! Please read the contributing guidelines before getting started.

## License

MIT
