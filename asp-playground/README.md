# Donation Management API

ASP.NET Core (.NET 10) REST API for ProGineer's *Backend Technical Task 5 – CRUD*. It manages donations and **never exposes the donor name of anonymous donations**. Backend only, no authentication (per the brief).

## The task in short

| Requirement | Where it is implemented |
|---|---|
| Donation: amount, donor name, anonymous flag, creation time | `Entities/Donation.cs` |
| Anonymous donor name never returned | `DTOs/DonationResponse.cs` (single mapping point) |
| CRUD at `/api/donations` | `Controllers/DonationsController.cs` |
| `GET /api/health` | `Controllers/HealthController.cs` |
| No auth | Nothing registered; every endpoint is public |
| Swagger/OpenAPI | `/swagger` (UI) and `/openapi/v1.json` |
| Persistence + migrations | EF Core + SQL Server (LocalDB), `Data/Migrations` |
| Postman tests | `postman/DonationApi.postman_collection.json` |

## Project structure

```
Controllers/   HTTP layer only: routes, status codes, Swagger annotations
Services/      Business logic (IDonationService / DonationService)
DTOs/          Request (validated) and response (masked) models
Entities/      Database entity
Data/          AppDbContext (schema config) and Migrations/
postman/       Exported Postman collection
Program.cs     Composition root
```

Flow: `Controller -> Service -> DbContext`. Entities never leave the service; controllers only see DTOs.

## Setup and run (without Docker)

Requirements: .NET 10 SDK and SQL Server LocalDB (installed with Visual Studio's *Data storage and processing* workload) or any SQL Server instance.

```bash
dotnet run --launch-profile http
```

- API: http://localhost:5234
- Swagger UI: http://localhost:5234/swagger
- OpenAPI JSON: http://localhost:5234/openapi/v1.json

## Run with Docker Compose

Runs the API and a SQL Server 2022 container together. The only requirement is Docker (with Compose v2); no .NET SDK or local SQL Server is needed.

1. **Configure credentials.** Copy `.env.example` to `.env` and set `DB_PASSWORD` (8+ characters with upper, lower, digit and symbol). `.env` is git-ignored; no credentials live in the source.

   ```bash
   cp .env.example .env
   ```

2. **Build and start** both containers (the API waits until SQL Server is healthy, then applies migrations on startup):

   ```bash
   docker compose up -d --build
   ```

3. **Use it:** Swagger UI at http://localhost:8080/swagger, health at http://localhost:8080/api/health. Check status or logs with `docker compose ps` / `docker compose logs -f api`.

4. **Stop:**

   ```bash
   docker compose down        # stop and remove containers, keep the data
   docker compose down -v     # also delete the database volume
   ```

| Variable (`.env`) | Purpose | Default |
|---|---|---|
| `DB_PASSWORD` | SQL Server `sa` password (required) | none |
| `DB_NAME` | Database name | `DonationDb` |
| `API_PORT` | Host port for the API | `8080` |

`compose.yaml` passes the connection string to the API as `ConnectionStrings__Default`, which overrides `appsettings.json`. The API reaches the database over the Compose network at host `db`. Data persists in the `sqldata` volume. The Postman collection works against Docker with `baseUrl` set to `http://localhost:8080`.

### Database

SQL Server database `DonationDb`. The default connection string (`ConnectionStrings:Default` in `appsettings.json`) targets LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=DonationDb;Trusted_Connection=True;TrustServerCertificate=True`. Change it to point at another instance. Pending migrations are applied automatically at startup (`Database.Migrate()`), so there is no manual step.

To inspect the data in Visual Studio: **View > SQL Server Object Explorer > (localdb)\MSSQLLocalDB > Databases > DonationDb > Tables > Donations** (right-click > *View Data*).

To manage migrations yourself:

```bash
dotnet tool install -g dotnet-ef
dotnet ef migrations add <Name> -o Data/Migrations
dotnet ef database update
```

#### Schema: `Donations`

| Column | Type | Constraints |
|---|---|---|
| `Id` | int | Primary key, identity |
| `Amount` | decimal(18,2) | NOT NULL, `CHECK (Amount > 0)` |
| `DonorName` | nvarchar(100) | NOT NULL |
| `IsAnonymous` | bit | NOT NULL |
| `CreatedAt` | datetime2 (UTC) | NOT NULL, indexed (list is ordered newest first) |

## Endpoints

| Method | Route | Success | Errors |
|---|---|---|---|
| GET | `/api/health` | 200 | |
| GET | `/api/donations?page=1&pageSize=10` | 200 | 400 invalid paging |
| GET | `/api/donations/{id}` | 200 | 400 invalid id, 404 |
| POST | `/api/donations` | 201 + `Location` | 400 validation |
| PUT | `/api/donations/{id}` | 200 | 400, 404 |
| DELETE | `/api/donations/{id}` | 204 | 400 invalid id, 404 |

Request body (POST/PUT):

```json
{ "amount": 50.0, "donorName": "John Doe", "isAnonymous": false }
```

Response for an anonymous donation: `{ "id": 2, "amount": 75.5, "donorName": null, "isAnonymous": true, "createdAt": "..." }`.

Paging (list endpoint only; a single-item GET has nothing to page): `page` is 1-based and defaults to 1, `pageSize` defaults to 10 and must be 1-100. Invalid values return 400. The response wraps the items with the paging metadata:

```json
{ "items": [ ... ], "page": 1, "pageSize": 10, "totalCount": 25, "totalPages": 3 }
```

Errors use RFC 7807 `ProblemDetails` (validation errors list each failing field).

## Postman

1. Start the API.
2. Import `postman/DonationApi.postman_collection.json` (variable `baseUrl` defaults to `http://localhost:5234`).
3. Run the collection in order, or from the CLI: `npx newman run postman/DonationApi.postman_collection.json`.

It has 30 requests / 47 assertions in six folders: Health, Create, Read, Update, Delete, Validation & Errors. These cover normal and anonymous flows, name masking (including a check that the secret name is absent from the raw body), zero/negative/non-numeric amounts, missing fields, invalid IDs, and non-existent IDs for GET/PUT/DELETE. The last run had all assertions passing.

## Key design decisions

- **Anonymity enforced in one place.** `DonationResponse.From` is the only mapping from entity to response. It nulls the name when `IsAnonymous` is true, so every endpoint (list, get, create, update) is covered and no endpoint can forget.
- **Name still stored for anonymous donations.** It is required on input and kept in the database (useful for audit), but it is never serialized.
- **Nullable request fields + `[Required]`.** `decimal?`/`bool?` let validation tell "missing" apart from `0`/`false`, so missing fields return 400 instead of silently defaulting.
- **Amount > 0** is validated in the API and backed by a DB `CHECK` constraint.
- **IDs are plain `int` route parameters** (no `:int` constraint), so `/api/donations/abc` returns 400 rather than a misleading 404.
- **DELETE returns 204**, and `ExecuteDeleteAsync` removes the row in a single query.
- **`CreatedAt` is set server-side in UTC** and is not changed by updates; clients cannot set it.
- **Swagger:** built-in .NET OpenAPI document generation plus Swagger UI. Status codes and models come from `[ProducesResponseType]` and XML doc comments.
- **Pagination** uses `Skip`/`Take` over a stable order (`CreatedAt` desc, then `Id` desc), so pages never overlap. The page size is capped at 100 to bound response size.
- **Simplicity:** a thin service layer and EF Core directly, with no repository or AutoMapper, since neither adds value at this size. Methods are written step by step (explicit types, `if` checks, no chained one-liners) to stay easy to read.
