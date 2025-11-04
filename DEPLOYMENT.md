# BotanicalBuddy Deployment Guide

This guide walks you through deploying BotanicalBuddy to Fly.io with PostgreSQL.

## Prerequisites

1. Install Fly CLI: https://fly.io/docs/hands-on/install-flyctl/
2. Sign up for a Fly.io account: https://fly.io/app/sign-up
3. Install .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

## Local Development Setup

### 1. Install PostgreSQL

#### macOS (using Homebrew)
```bash
brew install postgresql@15
brew services start postgresql@15
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install postgresql-15
sudo systemctl start postgresql
```

#### Windows
Download and install from: https://www.postgresql.org/download/windows/

### 2. Create Local Database

```bash
createdb botanicalbuddy
```

### 3. Update Connection String

Edit `appsettings.Development.json` (create if doesn't exist):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=botanicalbuddy;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "your-local-dev-secret-key-must-be-at-least-32-characters-long-for-security",
    "Issuer": "BotanicalBuddy.API",
    "Audience": "BotanicalBuddy.API"
  },
  "Trefle": {
    "ApiToken": "your-trefle-api-token-from-https://trefle.io"
  },
  "WeatherApi": {
    "ApiKey": "your-openweathermap-api-key",
    "BaseUrl": "https://api.openweathermap.org/data/2.5"
  }
}
```

### 4. Run Migrations

```bash
cd BotanicalBuddy.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at: https://localhost:5001 (or http://localhost:5000)
Swagger UI: https://localhost:5001

## Deploying to Fly.io

### 1. Login to Fly.io

```bash
flyctl auth login
```

### 2. Create Fly App

```bash
flyctl apps create botanicalbuddy
```

### 3. Create PostgreSQL Database

```bash
# Create a PostgreSQL cluster
flyctl postgres create --name botanicalbuddy-db --region sea

# Attach it to your app
flyctl postgres attach --app botanicalbuddy botanicalbuddy-db
```

This will automatically set the `DATABASE_URL` secret.

### 4. Set Secrets

```bash
# JWT Secret (generate a strong random key)
flyctl secrets set Jwt__Secret="$(openssl rand -base64 32)" --app botanicalbuddy

# Trefle API Token (get from https://trefle.io)
flyctl secrets set Trefle__ApiToken="your-trefle-api-token" --app botanicalbuddy

# OpenWeatherMap API Key (get from https://openweathermap.org/api)
flyctl secrets set WeatherApi__ApiKey="your-weather-api-key" --app botanicalbuddy

# Connection String (if not auto-set by postgres attach)
flyctl secrets set ConnectionStrings__DefaultConnection="your-postgres-connection-string" --app botanicalbuddy
```

### 5. Deploy

```bash
# From the root directory
flyctl deploy --app botanicalbuddy
```

### 6. Run Migrations on Production

After first deployment, run migrations:

```bash
# SSH into your Fly.io machine
flyctl ssh console --app botanicalbuddy

# Inside the container, run migrations
cd /app
dotnet ef database update --project BotanicalBuddy.API.dll
```

Or use this one-liner from your local machine:

```bash
flyctl ssh console --app botanicalbuddy -C "cd /app && dotnet ef database update"
```

### 7. Open Your App

```bash
flyctl open --app botanicalbuddy
```

## Environment Variables

The following environment variables are required:

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | Auto-set by Fly.io |
| `Jwt__Secret` | Secret key for JWT tokens (min 32 chars) | Random base64 string |
| `Jwt__Issuer` | JWT issuer | `BotanicalBuddy.API` |
| `Jwt__Audience` | JWT audience | `BotanicalBuddy.API` |
| `Trefle__ApiToken` | Trefle.io API token | Get from trefle.io |
| `WeatherApi__ApiKey` | OpenWeatherMap API key | Get from openweathermap.org |
| `WeatherApi__BaseUrl` | Weather API base URL | `https://api.openweathermap.org/data/2.5` |

## Monitoring

### View Logs

```bash
flyctl logs --app botanicalbuddy
```

### Check Status

```bash
flyctl status --app botanicalbuddy
```

### Scale Your App

```bash
# Scale to 2 instances
flyctl scale count 2 --app botanicalbuddy

# Scale VM resources
flyctl scale vm shared-cpu-1x --app botanicalbuddy
```

## Database Backups

Fly.io PostgreSQL clusters automatically create daily backups.

To create a manual backup:

```bash
flyctl postgres backup --app botanicalbuddy-db
```

To restore from backup:

```bash
flyctl postgres restore --app botanicalbuddy-db --backup-id <backup-id>
```

## Troubleshooting

### Check Database Connection

```bash
flyctl postgres connect --app botanicalbuddy-db
```

### View App Secrets

```bash
flyctl secrets list --app botanicalbuddy
```

### Restart App

```bash
flyctl apps restart botanicalbuddy
```

### Check Health

```bash
curl https://botanicalbuddy.fly.dev/health
```

## CI/CD with GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Fly.io

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Fly.io
        uses: superfly/flyctl-actions/setup-flyctl@master

      - name: Deploy to Fly.io
        run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

Get your Fly.io API token:

```bash
flyctl auth token
```

Add it to your GitHub repository secrets as `FLY_API_TOKEN`.

## Cost Estimation

Fly.io Pricing (as of 2024):

- **PostgreSQL**: ~$5-15/month depending on size
- **App instances**: Free tier available (shared-cpu-1x, 256MB RAM)
- **Outbound bandwidth**: First 100GB free, then $0.02/GB

For production, recommended minimum:
- 1x PostgreSQL cluster (1GB): ~$15/month
- 2x App instances (shared-cpu-1x, 512MB): ~$10/month
- **Total: ~$25/month**

## Security Checklist

- [ ] Set strong JWT secret (min 32 characters)
- [ ] Enable HTTPS only in production
- [ ] Rotate API keys regularly
- [ ] Set up monitoring and alerting
- [ ] Configure CORS for specific origins
- [ ] Enable rate limiting
- [ ] Set up database backups
- [ ] Review and limit database permissions
- [ ] Keep dependencies updated

## Next Steps

1. Set up custom domain
2. Configure CDN for static assets
3. Set up monitoring (Sentry, Application Insights)
4. Implement rate limiting
5. Set up email service for user verification
6. Configure Stripe for subscriptions
7. Add comprehensive logging
8. Set up automated backups
