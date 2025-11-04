# BotanicalBuddy Production Transformation Plan

## Overview
Transform BotanicalBuddy into a production-ready SaaS application deployed on Fly.io with PostgreSQL, in-memory caching, subscription tiers, and comprehensive user plant tracking with location-based insights.

## Phase 1: Database & Data Architecture

### 1.1 PostgreSQL Setup with Entity Framework Core
- Add EF Core packages (Npgsql.EntityFrameworkCore.PostgreSQL)
- Create DbContext with entities:
  - **Users**: Authentication, profiles, subscription tiers
  - **Addresses**: Location data with geolocation (lat/long)
  - **UserPlants**: User's plant collection linked to addresses
  - **PlantCareLog**: Watering schedules, care events, notes
  - **WeatherData**: Cached weather data per location
  - **Subscriptions**: Tier info, limits, billing status

### 1.2 Migration System
- Set up EF Core migrations
- Create initial migration with all tables
- Add seed data for subscription tiers

## Phase 2: Enhanced Features

### 2.1 User Management & Authentication
- Implement proper user registration/login (replace demo API key)
- Add password hashing (BCrypt/Identity)
- JWT tokens with user ID and subscription tier claims
- Email verification flow

### 2.2 User Plant Profiles
- CRUD endpoints for user's plant collection
- Link plants to addresses with geolocation
- Plant care tracking (watering schedules, fertilizing, observations)
- Upload plant photos (local storage or cloud - Azure Blob/S3)

### 2.3 Location & Weather Integration
- Integrate weather API (OpenWeatherMap or similar)
- Store weather data per address location
- Calculate watering recommendations based on:
  - Plant type (from Trefle data)
  - Local weather conditions
  - Soil moisture patterns
  - Historical care logs

### 2.4 Pattern Analysis & Recommendations
- Aggregate data queries for plant care patterns
- Best practices by plant type + location
- Watering frequency recommendations
- Seasonal care adjustments

## Phase 3: Caching Layer

### 3.1 In-Memory Caching Implementation
- Add `Microsoft.Extensions.Caching.Memory`
- Cache Trefle API responses (TTL: 24 hours)
- Cache weather data (TTL: 1 hour)
- Cache user plant lists (invalidate on updates)
- Add cache statistics endpoint for monitoring

## Phase 4: Subscription & Monetization

### 4.1 Subscription Tiers
- **Free Tier**: 5 plants max, basic care tracking
- **Basic Tier**: 25 plants, weather integration, $4.99/mo
- **Premium Tier**: Unlimited plants, advanced analytics, pattern insights, priority support, $14.99/mo

### 4.2 Stripe Integration
- Add Stripe SDK
- Create checkout sessions
- Webhook handling for subscription events
- Subscription status syncing
- Rate limiting middleware based on tier

### 4.3 Usage Tracking
- Middleware to track API usage per user
- Enforce tier-based limits
- Billing portal integration

## Phase 5: Docker & Fly.io Deployment

### 5.1 Dockerization
- Create optimized multi-stage Dockerfile
- Use ASP.NET Core 8.0 runtime image
- Configure for production (HTTPS, health checks)
- Set up docker-compose for local dev (app + PostgreSQL)

### 5.2 Fly.io Configuration
- Create `fly.toml` configuration
- Set up Fly PostgreSQL database
- Configure secrets (JWT secret, API keys, Stripe keys)
- Set up health check endpoints
- Configure scaling rules

### 5.3 CI/CD Pipeline
- GitHub Actions workflow for automated deployments
- Build Docker image on push to main
- Run tests before deployment
- Deploy to Fly.io
- Database migration automation

## Phase 6: API Enhancements

### 6.1 New Endpoints
- User registration/login/profile management
- Address management (CRUD with geocoding)
- User plants CRUD with photo upload
- Care log entries (watering, fertilizing, observations)
- Recommendations engine (watering schedules based on weather)
- Analytics dashboard (plant health trends, patterns)
- Subscription management (upgrade/downgrade, billing portal)

### 6.2 API Improvements
- Implement proper pagination (cursor-based)
- Add filtering and sorting
- Response compression
- API versioning (v1, v2)
- Rate limiting per subscription tier

## Phase 7: Documentation & Developer Experience

### 7.1 API Documentation
- Update Swagger/OpenAPI specs
- Add comprehensive examples for all endpoints
- Document authentication flows
- Rate limit documentation
- Error code reference

### 7.2 User Documentation
- Getting started guide
- Subscription tier comparison
- API integration guide
- Best practices for plant care tracking
- FAQ section

### 7.3 Developer Documentation
- Setup instructions (local development)
- Database schema documentation
- Architecture decision records
- Deployment guide
- Contributing guidelines

## Phase 8: Production Hardening

### 8.1 Security
- CORS policy refinement (specific origins)
- API key rotation mechanism
- Secret management (Fly.io secrets)
- Input validation and sanitization
- SQL injection prevention (parameterized queries)
- Rate limiting and DDoS protection

### 8.2 Monitoring & Observability
- Structured logging (Serilog)
- Application Insights or similar APM
- Error tracking (Sentry)
- Health check endpoints
- Metrics collection (response times, cache hit rates)

### 8.3 Testing
- Unit tests for services
- Integration tests for API endpoints
- Database tests with test containers
- End-to-end tests for critical flows

## Deliverables

1. Fully functional PostgreSQL database with migrations
2. User authentication and plant profile management
3. Location-based weather integration and care recommendations
4. In-memory caching for performance
5. Stripe subscription management (3 tiers)
6. Docker image ready for Fly.io
7. Complete deployment configuration
8. Comprehensive API and user documentation
9. CI/CD pipeline for automated deployments
10. Production-ready security and monitoring

## Estimated Implementation Order

1. **Database setup (Phase 1)** - Foundation
2. **User management (Phase 2.1)** - Core functionality
3. **User plants & locations (Phase 2.2)** - Key features
4. **Caching (Phase 3)** - Performance
5. **Weather integration (Phase 2.3)** - Enhanced value
6. **Subscription tiers (Phase 4.1-4.2)** - Monetization
7. **Docker & Fly.io (Phase 5)** - Deployment
8. **Documentation (Phase 7)** - Customer-ready
9. **Production hardening (Phase 8)** - Enterprise-ready

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Client Layer                         │
│              (Web App, Mobile App, API Clients)              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            │ HTTPS
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    ASP.NET Core 8.0 API                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              JWT Authentication Middleware              │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │            Rate Limiting Middleware (by tier)           │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                    API Controllers                      │ │
│  │  Auth│Plants│Users│Addresses│Care│Analytics│Subscriptions│
│  └───────────┬────────────────────────────────────────────┘ │
│              │                                                │
│  ┌───────────▼────────────────────────────────────────────┐ │
│  │                   Service Layer                         │ │
│  │  • TrefleApiService    • WeatherService                 │ │
│  │  • JwtTokenService     • PlantCareService               │ │
│  │  • StripeService       • RecommendationEngine           │ │
│  │  • GeocodingService    • UserService                    │ │
│  └───────────┬─────────────────────┬──────────────────────┘ │
└──────────────┼─────────────────────┼────────────────────────┘
               │                     │
    ┌──────────▼──────────┐  ┌──────▼─────────────────────┐
    │  In-Memory Cache    │  │   PostgreSQL Database       │
    │                     │  │                             │
    │ • Trefle API data   │  │ • Users & Auth              │
    │ • Weather data      │  │ • Addresses & Geolocation   │
    │ • User plant lists  │  │ • User Plants Collection    │
    │ • Recommendations   │  │ • Plant Care Logs           │
    └─────────────────────┘  │ • Weather History           │
                             │ • Subscriptions & Billing   │
                             └─────────────────────────────┘
               │                     │
    ┌──────────▼──────────┐  ┌──────▼─────────────────────┐
    │   Trefle.io API     │  │    Weather API              │
    │   (Plant Database)  │  │    (OpenWeatherMap)         │
    └─────────────────────┘  └─────────────────────────────┘
               │                     │
    ┌──────────▼──────────────────────▼─────────────────────┐
    │                   Stripe API                           │
    │            (Payment & Subscription Management)         │
    └────────────────────────────────────────────────────────┘
```

## Database Schema

```sql
-- Users Table
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    SubscriptionTier VARCHAR(50) DEFAULT 'Free',
    StripeCustomerId VARCHAR(255),
    EmailVerified BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- Addresses Table
CREATE TABLE Addresses (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    AddressLine1 VARCHAR(255) NOT NULL,
    AddressLine2 VARCHAR(255),
    City VARCHAR(100) NOT NULL,
    State VARCHAR(100),
    Country VARCHAR(100) NOT NULL,
    PostalCode VARCHAR(20),
    Latitude DECIMAL(10, 8),
    Longitude DECIMAL(11, 8),
    Timezone VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- UserPlants Table
CREATE TABLE UserPlants (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    AddressId UUID REFERENCES Addresses(Id),
    TreflePlantId INT,
    CommonName VARCHAR(255),
    ScientificName VARCHAR(255),
    Nickname VARCHAR(100),
    DatePlanted DATE,
    Location VARCHAR(255),
    Notes TEXT,
    PhotoUrl VARCHAR(500),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- PlantCareLog Table
CREATE TABLE PlantCareLog (
    Id UUID PRIMARY KEY,
    UserPlantId UUID REFERENCES UserPlants(Id),
    CareType VARCHAR(50) NOT NULL, -- 'Watering', 'Fertilizing', 'Pruning', etc.
    DateTime TIMESTAMP NOT NULL,
    Amount VARCHAR(50),
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- WeatherData Table
CREATE TABLE WeatherData (
    Id UUID PRIMARY KEY,
    AddressId UUID REFERENCES Addresses(Id),
    Date DATE NOT NULL,
    Temperature DECIMAL(5, 2),
    Humidity INT,
    Precipitation DECIMAL(5, 2),
    Conditions VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UNIQUE(AddressId, Date)
);

-- Subscriptions Table
CREATE TABLE Subscriptions (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    Tier VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL, -- 'active', 'cancelled', 'past_due'
    StripeSubscriptionId VARCHAR(255),
    CurrentPeriodStart TIMESTAMP,
    CurrentPeriodEnd TIMESTAMP,
    CancelAtPeriodEnd BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);
```

## Subscription Tier Details

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

## Technology Stack Summary

### Backend
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core Web API
- **Language**: C# 12
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 15+
- **Caching**: In-Memory Cache (IMemoryCache)
- **Authentication**: JWT Bearer Tokens

### External Services
- **Plant Database**: Trefle.io API
- **Weather**: OpenWeatherMap API
- **Payments**: Stripe
- **Geocoding**: Google Maps API or similar

### Deployment
- **Platform**: Fly.io
- **Container**: Docker
- **CI/CD**: GitHub Actions
- **Database**: Fly PostgreSQL

### Monitoring & Observability
- **Logging**: Serilog
- **APM**: Application Insights / Sentry
- **Health Checks**: ASP.NET Core Health Checks

## Next Steps

1. Review and approve this plan
2. Set up project tracking (GitHub Projects or similar)
3. Begin Phase 1: Database setup
4. Iterate through phases sequentially
5. Test thoroughly at each phase
6. Deploy to staging environment on Fly.io
7. Beta testing with select users
8. Production launch

## Questions & Considerations

- **Email Service**: Which email provider for verification/notifications? (SendGrid, AWS SES, Mailgun)
- **Photo Storage**: Local storage initially or cloud? (Azure Blob, AWS S3, Cloudinary)
- **Weather API**: OpenWeatherMap (free tier available) or alternative?
- **Geocoding**: Google Maps API, Mapbox, or OpenStreetMap?
- **Domain Name**: Custom domain for production deployment?
- **SSL/TLS**: Fly.io provides automatic HTTPS
- **Backup Strategy**: Database backup frequency and retention?
- **Scaling Strategy**: Horizontal scaling plan for high traffic?

---

**Document Status**: Draft
**Created**: 2025-11-04
**Last Updated**: 2025-11-04
**Version**: 1.0
