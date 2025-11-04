# BotanicalBuddy.NET API Documentation

Comprehensive API documentation for the BotanicalBuddy plant tracking and identification API.

## Base URL

```
http://localhost:5000/api
https://localhost:5001/api
```

## Authentication

All plant endpoints require JWT authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Getting a JWT Token

**Endpoint:** `POST /api/auth/token`

**Request Body:**
```json
{
  "username": "your-username",
  "apiKey": "demo-api-key"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "24 hours",
  "tokenType": "Bearer",
  "usage": "Include in Authorization header as: Bearer <token>"
}
```

**Example using curl:**
```bash
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "john_doe", "apiKey": "demo-api-key"}'
```

**Example using C#:**
```csharp
using System.Net.Http.Json;

var client = new HttpClient();
var request = new { username = "john_doe", apiKey = "demo-api-key" };

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/auth/token",
    request
);

var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
Console.WriteLine($"Token: {result.Token}");
```

---

## Authentication Endpoints

### 1. Generate Token

Generate a new JWT token for API access.

- **URL:** `/api/auth/token`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| username | string | Yes | Your username |
| apiKey | string | No | API key (default: demo-api-key) |

**Success Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": "24 hours",
  "tokenType": "Bearer",
  "usage": "Include in Authorization header as: Bearer <token>"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": "Missing or invalid username"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "error": "Invalid API key"
}
```

---

### 2. Refresh Token

Refresh an existing JWT token.

- **URL:** `/api/auth/refresh`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
```json
{
  "username": "your-username"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "token": "new-jwt-token-string",
  "expiresIn": "24 hours",
  "tokenType": "Bearer"
}
```

**Example using PowerShell:**
```powershell
$body = @{
    username = "john_doe"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/refresh" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

---

### 3. Verify Token

Check if a JWT token is valid.

- **URL:** `/api/auth/verify`
- **Method:** `GET`
- **Auth Required:** Yes (Bearer token in header)

**Success Response (200 OK):**
```json
{
  "success": true,
  "valid": true,
  "message": "Token is valid"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "valid": false,
  "error": "Missing or invalid authorization header"
}
```

---

## Plant Endpoints (Trefle API Integration)

All plant endpoints require JWT authentication via the `Authorization: Bearer <token>` header.

### 1. List Plants

Get a paginated list of all plants.

- **URL:** `/api/plants`
- **Method:** `GET`
- **Auth Required:** Yes

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number for pagination |

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 123456,
      "commonName": "European Silver Fir",
      "scientificName": "Abies alba",
      "familyCommonName": "Pine family",
      "imageUrl": "https://bs.plantnet.org/image/o/...",
      "slug": "abies-alba"
    }
  ],
  "meta": {
    "total": 500000
  },
  "links": {
    "self": "https://trefle.io/api/v1/plants?page=1",
    "first": "https://trefle.io/api/v1/plants?page=1",
    "next": "https://trefle.io/api/v1/plants?page=2",
    "last": "https://trefle.io/api/v1/plants?page=25000"
  }
}
```

**Example using curl:**
```bash
curl -X GET "http://localhost:5000/api/plants?page=1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Example using C#:**
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync("http://localhost:5000/api/plants?page=1");
var plants = await response.Content.ReadFromJsonAsync<ApiResponse<List<Plant>>>();
```

---

### 2. Search Plants

Search for plants by name or other criteria.

- **URL:** `/api/plants/search`
- **Method:** `GET`
- **Auth Required:** Yes

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| q | string | Yes | - | Search query (common name, scientific name, etc.) |
| page | integer | No | 1 | Page number for pagination |

**Success Response (200 OK):**
```json
{
  "success": true,
  "query": "rose",
  "data": [
    {
      "id": 234567,
      "commonName": "Rose",
      "scientificName": "Rosa",
      "familyCommonName": "Rose family",
      "imageUrl": "https://...",
      "slug": "rosa"
    }
  ],
  "meta": {
    "total": 150
  },
  "links": {
    "self": "https://trefle.io/api/v1/plants/search?q=rose&page=1",
    "first": "https://trefle.io/api/v1/plants/search?q=rose&page=1",
    "last": "https://trefle.io/api/v1/plants/search?q=rose&page=8"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": "Missing required parameter: q (query)"
}
```

**Example using curl:**
```bash
curl -X GET "http://localhost:5000/api/plants/search?q=rose&page=1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

### 3. Get Plant Details

Get detailed information about a specific plant by ID.

- **URL:** `/api/plants/{id}`
- **Method:** `GET`
- **Auth Required:** Yes

**URL Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | integer | Yes | The Trefle plant ID |

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 234567,
    "commonName": "Rose",
    "scientificName": "Rosa",
    "family": "Rosaceae",
    "genus": "Rosa",
    "observations": "Beautiful flowering plant",
    "vegetable": false
  },
  "meta": {
    "lastModified": "2024-01-15T10:30:00.000Z"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": "Invalid plant ID"
}
```

**Example using curl:**
```bash
curl -X GET "http://localhost:5000/api/plants/234567" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Example using C#:**
```csharp
var plantId = 234567;
var response = await client.GetAsync($"http://localhost:5000/api/plants/{plantId}");
var plantDetail = await response.Content.ReadFromJsonAsync<ApiResponse<PlantDetail>>();
```

---

### 4. Filter Plants by Common Name

Filter plants by their common name.

- **URL:** `/api/plants/filter/common-name`
- **Method:** `GET`
- **Auth Required:** Yes

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| name | string | Yes | - | Common name to filter by |
| page | integer | No | 1 | Page number for pagination |

**Success Response (200 OK):**
```json
{
  "success": true,
  "filter": {
    "common_name": "fern"
  },
  "data": [
    {
      "id": 345678,
      "commonName": "Fern",
      "scientificName": "Pteridium aquilinum",
      "familyCommonName": "Fern family",
      "imageUrl": "https://...",
      "slug": "pteridium-aquilinum"
    }
  ],
  "meta": {
    "total": 45
  },
  "links": {
    "self": "https://trefle.io/api/v1/plants?filter[common_name]=fern&page=1"
  }
}
```

**Example using curl:**
```bash
curl -X GET "http://localhost:5000/api/plants/filter/common-name?name=fern&page=1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
Invalid request parameters.

```json
{
  "success": false,
  "error": "Missing required parameter: q (query)"
}
```

### 401 Unauthorized
Missing or invalid authentication token.

```json
{
  "success": false,
  "error": "Unauthorized",
  "message": "Missing or invalid authorization header"
}
```

### 500 Internal Server Error
Server-side error occurred.

```json
{
  "success": false,
  "error": "Failed to retrieve plants",
  "message": "Connection timeout"
}
```

---

## Setup Instructions

### 1. Configure appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "BotanicalBuddy.API",
    "Audience": "BotanicalBuddy.API"
  },
  "Auth": {
    "ApiKey": "demo-api-key"
  },
  "Trefle": {
    "ApiToken": "your-trefle-api-token-here"
  }
}
```

### 2. Run the Application

```bash
cd BotanicalBuddy.API
dotnet restore
dotnet run
```

### 3. Access Swagger UI

Navigate to `http://localhost:5000` for interactive API documentation.

### 4. Quick Start Example (C#)

```csharp
using System.Net.Http.Json;
using System.Net.Http.Headers;

// 1. Get JWT token
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var tokenRequest = new { username = "test_user", apiKey = "demo-api-key" };
var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", tokenRequest);
var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

// 2. Configure authentication header
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token.Token);

// 3. Search for plants
var searchResponse = await client.GetAsync("/api/plants/search?q=rose");
var plants = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<List<Plant>>>();

Console.WriteLine($"Found {plants.Meta.Total} plants matching 'rose'");
foreach (var plant in plants.Data)
{
    Console.WriteLine($"- {plant.CommonName} ({plant.ScientificName})");
}
```

### 5. Quick Start Example (Bash)

```bash
# 1. Get JWT token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test_user", "apiKey": "demo-api-key"}' \
  | jq -r '.token')

# 2. Search for plants
curl -X GET "http://localhost:5000/api/plants/search?q=rose" \
  -H "Authorization: Bearer $TOKEN" | jq

# 3. Get plant details
curl -X GET "http://localhost:5000/api/plants/123456" \
  -H "Authorization: Bearer $TOKEN" | jq
```

---

## Rate Limiting

The Trefle API has rate limits. Please refer to [Trefle.io documentation](https://trefle.io/) for current rate limits.

## Support

For issues or questions, please open an issue on the GitHub repository.

## Additional Resources

- [Trefle.io API Documentation](https://docs.trefle.io/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [JWT Authentication in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/)
