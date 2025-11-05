# Docker Compose Quick Start Guide

This guide helps you run BotanicalBuddy locally using Docker Compose without installing PostgreSQL.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for running migrations)

## Quick Start

### 1. Clone and Setup

```bash
# Clone the repository
git clone https://github.com/thisguymartin/BotnyBuddy.git
cd BotnyBuddy

# Copy environment variables
cp .env.example .env
```

### 2. Configure API Keys (Optional)

Edit `.env` and add your API keys:

```bash
TREFLE_API_TOKEN=your-trefle-api-token-here
WEATHER_API_KEY=your-weather-api-key-here
```

Get API keys:
- Trefle.io: https://trefle.io/
- OpenWeatherMap: https://openweathermap.org/api

### 3. Start Database Only

For local development where you run the API locally but want a database:

```bash
# Start PostgreSQL
docker-compose up -d postgres

# Check it's running
docker-compose ps
```

The database will be available at:
- **Host**: localhost
- **Port**: 5432
- **Database**: botanicalbuddy
- **Username**: postgres
- **Password**: postgres

### 4. Run Migrations

```bash
cd BotanicalBuddy.API

# Install EF Core tools (one-time)
dotnet tool install --global dotnet-ef

# Create and run migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the API Locally

```bash
# Run the API
dotnet run

# API will be available at:
# - https://localhost:5001
# - http://localhost:5000
# - Swagger UI: https://localhost:5001
```

## Alternative: Full Stack with Docker

Run both database and API in Docker:

```bash
# Build and start everything
docker-compose --profile fullstack up -d --build

# View logs
docker-compose logs -f api

# API will be available at:
# - http://localhost:8080
# - Swagger UI: http://localhost:8080
```

## Optional: Database Management with pgAdmin

Start pgAdmin for a visual database interface:

```bash
# Start with pgAdmin
docker-compose --profile tools up -d postgres pgadmin

# Access pgAdmin at: http://localhost:5050
# Email: admin@botanicalbuddy.com
# Password: admin
```

### Connect to Database in pgAdmin

1. Open http://localhost:5050
2. Login with credentials above
3. Add New Server:
   - **General > Name**: BotanicalBuddy
   - **Connection > Host**: postgres
   - **Connection > Port**: 5432
   - **Connection > Database**: botanicalbuddy
   - **Connection > Username**: postgres
   - **Connection > Password**: postgres

## Common Commands

```bash
# Start services
docker-compose up -d postgres                    # Database only
docker-compose --profile tools up -d             # Database + pgAdmin
docker-compose --profile fullstack up -d         # Everything

# View logs
docker-compose logs -f postgres                  # Database logs
docker-compose logs -f api                       # API logs

# Stop services
docker-compose down                              # Stop all
docker-compose down -v                           # Stop and remove volumes (data)

# Check status
docker-compose ps

# Restart a service
docker-compose restart postgres

# Execute SQL directly
docker-compose exec postgres psql -U postgres -d botanicalbuddy
```

## Database Connection Strings

### For Local .NET Development
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=botanicalbuddy;Username=postgres;Password=postgres"
  }
}
```

### For Docker API Container
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=botanicalbuddy;Username=postgres;Password=postgres"
  }
}
```

## Troubleshooting

### Port 5432 Already in Use

If you have PostgreSQL installed locally:

```bash
# Stop local PostgreSQL (macOS)
brew services stop postgresql@15

# Stop local PostgreSQL (Linux)
sudo systemctl stop postgresql

# Or change the port in docker-compose.yml
ports:
  - "5433:5432"  # Use port 5433 instead
```

### Database Connection Refused

```bash
# Check if PostgreSQL is healthy
docker-compose ps

# Wait for database to be ready
docker-compose exec postgres pg_isready -U postgres

# Check logs
docker-compose logs postgres
```

### Reset Database

```bash
# Stop and remove all data
docker-compose down -v

# Start fresh
docker-compose up -d postgres

# Run migrations again
cd BotanicalBuddy.API
dotnet ef database update
```

### API Can't Connect to Database

If running the API locally and can't connect to Docker PostgreSQL:

1. Check the connection string uses `localhost` not `postgres`
2. Verify port 5432 is exposed: `docker-compose ps`
3. Test connection: `docker-compose exec postgres psql -U postgres -d botanicalbuddy`

## Data Persistence

Database data is stored in a Docker volume named `postgres_data`. This persists between restarts.

To backup your data:

```bash
# Create backup
docker-compose exec postgres pg_dump -U postgres botanicalbuddy > backup.sql

# Restore backup
docker-compose exec -T postgres psql -U postgres botanicalbuddy < backup.sql
```

## Environment Variables

You can override defaults by creating a `.env` file:

```env
TREFLE_API_TOKEN=your-token
WEATHER_API_KEY=your-key
POSTGRES_DB=mydb
POSTGRES_USER=myuser
POSTGRES_PASSWORD=mypassword
```

## Next Steps

1. Start developing with hot reload: `dotnet watch run`
2. Make API calls: See examples in README.md
3. View API docs: Navigate to https://localhost:5001
4. Deploy to production: See DEPLOYMENT.md

## Cleanup

Remove everything (containers, volumes, networks):

```bash
docker-compose down -v
docker-compose --profile tools down -v
docker-compose --profile fullstack down -v
```

## Production Note

⚠️ This setup is for **local development only**. For production:
- Use strong passwords
- Use environment-based configuration
- Follow security best practices in DEPLOYMENT.md
