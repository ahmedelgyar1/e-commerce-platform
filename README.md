# Multi-Vendor E-Commerce Platform API

A production-ready backend service for a multi-vendor e-commerce platform built with **ASP.NET Core 8**, **Entity Framework Core**, **PostgreSQL**, and **ASP.NET Identity**. The platform enables merchants to manage products with a flexible, scalable product variant system using the **Entity-Attribute-Value (EAV)** pattern. Product images are managed via **Cloudinary** for optimized cloud-based storage and delivery.

---

## Table of Contents

- [Live Deployment](#live-deployment)
- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Environment Configuration](#environment-configuration)
- [Database Design](#database-design)
  - [ERD Diagram](#erd-diagram)
  - [Entity Mapping & Relationships](#entity-mapping--relationships)
  - [Design Justification](#design-justification)
- [API Documentation](#api-documentation)
  - [Authentication Endpoints](#authentication-endpoints)
  - [Product Endpoints](#product-endpoints)
  - [Product Attribute Endpoints](#product-attribute-endpoints)
  - [Product Variant Endpoints](#product-variant-endpoints)
- [Features & Bonus Points](#features--bonus-points)
- [Scalability Considerations](#scalability-considerations)

---

## Live Deployment

The API is deployed and publicly accessible on **Monster ASP.NET Hosting**:

| | URL |
|---|---|
| **Swagger UI** | `https://e-commerce-cloud-co.runasp.net/swagger/index.html` |
| **API base URL** | `https://e-commerce-cloud-co.runasp.net` |

---

## Architecture Overview

The project follows a **Layered Architecture** pattern organized into four distinct layers within a single ASP.NET Core project:

```
┌─────────────────────────────────────────────┐
│                  Api Layer                  │
│         Controllers, Middlewares            │
├─────────────────────────────────────────────┤
│             Application Layer               │
│     Services, Interfaces, DTOs              │
├─────────────────────────────────────────────┤
│              Domain Layer                   │
│   Entities, Enums, Common, Interfaces       │
├─────────────────────────────────────────────┤
│           Infrastructure Layer              │
│   DbContext, Repositories, Migrations       │
└─────────────────────────────────────────────┘
```

**Key Design Decisions:**
- **Repository Pattern** abstracts data access behind interfaces, enabling unit testing with mocks.
- **Service Layer** encapsulates all business logic, keeping controllers thin.
- **Global Exception Handling Middleware** intercepts all unhandled exceptions and returns standardized JSON error responses.
- **Soft Delete** is implemented transparently via EF Core Global Query Filters and `SaveChangesAsync` interception — deleted records are never physically removed.

---

## Technology Stack

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Language | C# 12 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 8 |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer Tokens + Refresh Tokens |
| Caching | In-Memory Cache (`IMemoryCache`) |
| Image Storage | Cloudinary (via `CloudinaryDotNet` SDK) |
| Documentation | Swagger / OpenAPI with XML Comments |
| Testing | xUnit + Moq |
| Rate Limiting | ASP.NET Core Built-in Rate Limiter |

---

## Project Structure

```
e-commerce-platform/
├── src/
│   └── e-commerce-platform/
│       ├── Api/
│       │   ├── Controllers/          # REST API controllers
│       │   └── Middlewares/          # Global exception handling
│       ├── Application/
│       │   ├── Interfaces/           # Service contracts (IProductService, etc.)
│       │   ├── Services/             # Business logic implementations
│       │   └── DTOs/                 # Request/Response data transfer objects
│       │       ├── Auth/             # Auth DTOs (Login, Register, etc.)
│       │       └── Product/          # Product/Variant/Attribute DTOs
│       ├── Domain/
│       │   ├── Entities/             # Core domain models
│       │   ├── Enums/                # Status enumerations
│       │   ├── Common/               # Shared interfaces (ISoftDelete)
│       │   └── Interfaces/           # Repository contracts
│       ├── Infrastructure/
│       │   ├── Data/                 # EF Core DbContext & configurations
│       │   ├── Repositories/         # Repository implementations
│       │   ├── Services/             # External service implementations (Cloudinary)
│       │   └── Migrations/           # EF Core database migrations
│       ├── Settings/                 # Configuration POCOs (JwtSettings, MailSettings, CloudinarySettings)
│       ├── Program.cs               # Application entry point & DI configuration
│       └── appsettings.json          # Application configuration
├── test/
│   └── e-commerce-platform.UnitTests/
│       ├── Products/                 # ProductService unit tests
│       └── ProductVariants/          # ProductVariantService unit tests
└── README.md
```

---

## Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [PostgreSQL](https://www.postgresql.org/download/) 14+
- [Git](https://git-scm.com/)

### 1. Clone the Repository
```bash
git clone https://github.com/<your-username>/e-commerce-platform.git
cd e-commerce-platform
```

### 2. Configure the Environment
Copy the connection string template and update it with your PostgreSQL credentials (see [Environment Configuration](#environment-configuration)).

### 3. Apply Database Migrations
```bash
cd src/e-commerce-platform
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```
The API will be available at `https://localhost:5242` (or the port configured in `launchSettings.json`).

### 5. Access Swagger UI
Navigate to:
```
https://localhost:5242/swagger
```

### 6. Run Unit Tests
```bash
cd test/e-commerce-platform.UnitTests
dotnet test
```

---

## Environment Configuration

All configuration is managed through `appsettings.json`. Update the following sections before running the application:

### Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce_db;Username=postgres;Password=YOUR_PASSWORD"
}
```

### JWT Settings
```json
"JwtSettings": {
  "SecretKey": "YOUR_SUPER_SECRET_KEY_MUST_BE_AT_LEAST_32_CHARACTERS_LONG!!",
  "Issuer": "e-commerce-platform",
  "Audience": "e-commerce-platform-clients",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

### Mail Settings (Email Verification via Gmail)
To send verification emails using Gmail's SMTP server:
1. Enable **2-Step Verification** on your Google Account.
2. Generate an **App Password** (go to Google Account Security -> App Passwords, generate a password for "Mail").
3. Update `appsettings.json` with your credentials:

```json
"MailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your_email@gmail.com",
  "Password": "your_16_character_app_password",
  "FromEmail": "your_email@gmail.com",
  "FromName": "E-Commerce Platform"
}
```

---

## Database Design

### ERD Diagram

<img width="3259" height="8192" alt="image" src="https://github.com/user-attachments/assets/70b588df-e6c5-4d49-9aac-7797e714b618" />


### Entity Mapping & Relationships

| Relationship | Type | Description |
|---|---|---|
| `Merchant` → `Product` | One-to-Many | A merchant owns multiple products |
| `Product` → `Attribute` | One-to-Many | A product defines its own set of attributes (e.g., Color, Size) |
| `Attribute` → `AttributeValue` | One-to-Many | Each attribute has multiple possible values (e.g., Red, Blue) |
| `Product` → `ProductVariant` | One-to-Many | A product can have multiple variants (e.g., Red/Small, Blue/Large) |
| `ProductVariant` ↔ `AttributeValue` | Many-to-Many | A variant is defined by a combination of attribute values, linked through `ProductVariantAttributeValue` |
| `ApplicationUser` → `RefreshToken` | One-to-Many | A user can have multiple active refresh tokens |
| `ApplicationUser` → `Merchant` | TPH Inheritance | Merchant inherits from ApplicationUser using Table-Per-Hierarchy |

### Design Justification

#### Why EAV (Entity-Attribute-Value) for Product Variants?

The core challenge is supporting **products with completely different attribute structures** — a T-Shirt has Color and Size, while a Laptop has RAM and Storage. This eliminates any approach based on hardcoded columns.

**The EAV approach** was selected because:

1. **Unlimited Flexibility:** Any product can define any number of attributes with any number of values. No schema changes are needed when a new product type is introduced.
2. **No Hardcoded Columns:** The attribute names and values are stored as data, not as database columns. Adding "Material" to a T-Shirt or "Screen Size" to a Laptop requires zero code changes.
3. **Scalable Variant Combinations:** Variants are created by selecting exactly one value from each attribute. The system validates completeness (all attributes covered) and uniqueness (no duplicate combinations) at the service layer.
4. **Normalized Storage:** The join table `ProductVariantAttributeValue` avoids data duplication and supports efficient querying.

**Trade-offs:**
- **Query Complexity:** Filtering variants by attribute values requires joins across multiple tables, which is more complex than querying flat columns. This is mitigated by indexing and pagination.
- **Validation at Application Layer:** Since the database schema doesn't enforce attribute completeness, the business logic layer validates that each variant covers all defined attributes.

#### Why In-Memory Cache Instead of Redis?

We intentionally chose `IMemoryCache` over Redis for the current stage of the project:

1. **Single-Instance Deployment:** The application currently runs as a single instance. In-memory cache provides the fastest possible access (no network round-trip, no serialization overhead) for this topology.
2. **Zero Infrastructure Overhead:** Redis requires a separate server/container, connection management, and monitoring. `IMemoryCache` works out of the box with no additional infrastructure.
3. **Simplicity & Cost:** For a project at this scale, introducing Redis adds operational complexity without meaningful benefit. In-memory cache is free and requires zero configuration.
4. **Sufficient for Current Needs:** We cache individual product reads with a 10-minute sliding expiration and invalidate on writes. This pattern works perfectly with in-memory cache.
5. **Easy Migration Path:** The caching logic is encapsulated in the service layer. Migrating to Redis (via `IDistributedCache`) in the future requires minimal code changes — the service interfaces remain the same.

**When to switch to Redis:**
- When the application scales to **multiple instances** (load-balanced), requiring a shared cache.
- When cache size exceeds available memory on the application server.
- When **cache persistence** across application restarts becomes a requirement.

#### Why Soft Delete?

- **Data Recovery:** Accidentally deleted products can be restored without backups.
- **Audit Trail:** The `DeletedAt` timestamp provides a clear audit trail.
- **Referential Integrity:** Orders or analytics that reference deleted products won't break.
- **Implementation:** EF Core Global Query Filters automatically append `WHERE IsDeleted = false` to all queries, making soft delete transparent to the application code.

#### Why TPH (Table-Per-Hierarchy) for User Inheritance?

- **Performance:** Single table = single query for authentication, no JOINs needed.
- **Simplicity:** ASP.NET Identity integrates seamlessly with a single user table.
- **Extensibility:** Adding new roles (e.g., Admin) requires only a new class inheriting from `ApplicationUser`.

---

## API Documentation

### Swagger / OpenAPI

All endpoints are documented with **XML comments** and **strongly-typed `[ProducesResponseType]` attributes**. The Swagger UI is available at:

```
https://e-commerce-cloud-co.runasp.net/swagger/index.html
```

Swagger provides:
- Interactive endpoint testing
- Request/Response schema definitions
- JWT Bearer authentication support (click "Authorize" and paste your token)

---

### Authentication Endpoints

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register a new merchant account | No |
| `POST` | `/api/auth/verify-email` | Verify email with OTP code | No |
| `POST` | `/api/auth/login` | Login and receive JWT + Refresh Token | No |
| `POST` | `/api/auth/refresh` | Refresh an expired access token | No |
| `POST` | `/api/auth/logout` | Revoke the refresh token (logout) | No |

**Rate Limit:** 10 requests/minute (Fixed Window)

---

### Product Endpoints

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/products` | Create a new product | Merchant |
| `GET` | `/api/products` | List all products (paginated, filterable, searchable) | Public |
| `GET` | `/api/products/{id}` | Get product details by ID | Public |
| `PATCH` | `/api/products/{id}` | Update product details | Merchant (Owner) |
| `DELETE` | `/api/products/{id}` | Soft delete a product | Merchant (Owner) |
| `POST` | `/api/products/{id}/image` | Upload a product image to Cloudinary | Merchant (Owner) |

**Query Parameters for `GET /api/products`:**

| Parameter | Type | Description |
|---|---|---|
| `search` | `string` | Search by name or description |
| `minPrice` | `decimal` | Minimum base price filter |
| `maxPrice` | `decimal` | Maximum base price filter |
| `status` | `enum` | Filter by product status (Draft, Active, Archived) |
| `merchantId` | `Guid` | Filter by merchant |
| `page` | `int` | Page number (default: 1) |
| `pageSize` | `int` | Items per page (default: 10) |

**Rate Limit:** 200 requests/minute (Fixed Window)

---

### Product Attribute Endpoints

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/products/{productId}/attributes` | Add attribute with values | Merchant (Owner) |
| `GET` | `/api/products/{productId}/attributes` | List all attributes for a product | Public |
| `PATCH` | `/api/products/{productId}/attributes/{attributeId}` | Update attribute details | Merchant (Owner) |
| `DELETE` | `/api/products/{productId}/attributes/{attributeId}` | Delete attribute and its values | Merchant (Owner) |
| `POST` | `/api/products/{productId}/attributes/{attributeId}/values` | Add a value to an attribute | Merchant (Owner) |
| `PATCH` | `/api/products/{productId}/attributes/{attributeId}/values/{valueId}` | Update an attribute value | Merchant (Owner) |
| `DELETE` | `/api/products/{productId}/attributes/{attributeId}/values/{valueId}` | Delete an attribute value | Merchant (Owner) |

---

### Product Variant Endpoints

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/products/{productId}/variants` | Create a variant with attribute value combination | Merchant (Owner) |
| `GET` | `/api/products/{productId}/variants` | List variants (paginated, filterable, searchable) | Public |
| `GET` | `/api/products/{productId}/variants/{variantId}` | Get variant details | Public |
| `PATCH` | `/api/products/{productId}/variants/{variantId}` | Update variant (quantity, price, status) | Merchant (Owner) |
| `DELETE` | `/api/products/{productId}/variants/{variantId}` | Soft delete a variant | Merchant (Owner) |

**Variant Fields:**

| Field | Type | Description |
|---|---|---|
| `sku` | `string` | Globally unique stock keeping unit |
| `quantity` | `int` | Available stock quantity |
| `priceOverride` | `decimal?` | Override price (falls back to product base price if null) |
| `isActive` | `bool` | Whether the variant is available for purchase |
| `attributeValueIds` | `Guid[]` | Exactly one value per defined attribute |

**Validation Rules:**
- SKU must be globally unique across all products
- Must provide exactly one value for each attribute defined on the product
- Attribute values must belong to the target product
- No duplicate variant combinations allowed

---

## Features & Bonus Points

| Feature | Status | Details |
|---|---|---|
| JWT Authentication | ✅ | Access Token + Refresh Token + Logout |
| Docker Support | ✅ | Multi-stage Dockerfile and Docker Compose containerization |
| Email Verification | ✅ | OTP-based email verification via SMTP |
| Product CRUD | ✅ | Full CRUD with pagination, filtering, and search |
| Product Image Upload | ✅ | Cloudinary integration with auto quality/format optimization (max 5MB, JPEG/PNG/WebP/GIF) |
| Flexible Variant System | ✅ | EAV pattern — unlimited attributes, values, and combinations |
| Swagger Documentation | ✅ | XML comments + ProducesResponseType annotations |
| Unit Tests | ✅ | 12 tests covering ProductService and ProductVariantService (xUnit + Moq) |
| Caching | ✅ | In-memory caching for product reads with automatic invalidation ([why not Redis?](#why-in-memory-cache-instead-of-redis)) |
| Soft Delete | ✅ | EF Core Global Query Filters for Products and Variants |
| Rate Limiting | ✅ | Fixed-window rate limiting (200 req/min general, 10 req/min auth) |
| Global Exception Handling | ✅ | Centralized middleware returning consistent JSON error responses |
| Logging | ✅ | Structured logging via `ILogger<T>` across all services and controllers |

---

## Docker Containerization

To run the entire application stack (API and PostgreSQL database) inside containers:

### 1. Start the Containers
Run the following command at the project root:
```bash
docker compose up --build
```
This command builds the API docker image and spins up two services:
* **web-api:** Running the ASP.NET Core application on port `5242`.
* **db:** Running PostgreSQL 15 on port `5432` with an automated health check.

### 2. Access the Application (Local)
* **Swagger UI:** `http://localhost:5242/swagger/index.html`
* **API base URL:** `http://localhost:5242`

---

## Scalability Considerations

1. **EAV Pattern:** The database schema scales horizontally — adding new product types with entirely different attribute structures requires zero schema changes or migrations.
2. **Pagination on All List Endpoints:** All collection endpoints return paginated results, preventing memory issues with large datasets.
3. **Caching Strategy:** Individual product reads are cached with a 10-minute sliding expiration. Cache is automatically invalidated on update/delete operations to prevent stale data.
4. **Rate Limiting:** Protects the API from abuse and brute-force attacks. Auth endpoints have stricter limits (10 req/min).
5. **Soft Delete with Query Filters:** Deletes are O(1) (no cascading physical deletes) and the query filter ensures deleted records are transparently excluded without any code changes.
6. **Repository Pattern:** Abstracts data access, making it straightforward to swap PostgreSQL for another database or add read replicas.
7. **Indexed SKU Uniqueness:** SKU uniqueness is validated at the application layer and enforced at the database level, ensuring data integrity at scale.
