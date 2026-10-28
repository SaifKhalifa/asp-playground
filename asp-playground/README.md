# Authentication & Authorization API

ASP.NET Core (.NET 10) Web API with registration, login, JWT bearer authentication and role-based authorization (User / Admin). Backend only, SQL Server via EF Core.

## Endpoints

| Method | Route | Access | Responses |
|--------|-------|--------|-----------|
| POST | `/api/auth/register` | Public | 201 / 400 |
| POST | `/api/auth/login` | Public | 200 (returns JWT) / 401 |
| GET | `/api/profile` | Any authenticated user | 200 / 401 |
| GET | `/api/user` | User or Admin | 200 / 401 |
| GET | `/api/admin` | Admin only | 200 / 401 / 403 |

## Running the app

1. Requirements: .NET 10 SDK, SQL Server LocalDB (installed with Visual Studio) or any SQL Server.
2. Configure the database in `appsettings.json` -> `ConnectionStrings:DefaultConnection`.
   The default uses LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=AuthTaskDb;Trusted_Connection=True;TrustServerCertificate=True`
3. Create the database from the migration (pick one):
   - Visual Studio: *Tools > NuGet Package Manager > Package Manager Console* -> `Update-Database`
   - CLI: `dotnet ef database update`
4. Run (F5 in Visual Studio, or `dotnet run --launch-profile http`). The app listens on `http://localhost:5234`.
5. Swagger UI: `http://localhost:5234/swagger`. Call `/api/auth/login`, click **Authorize**, paste the token (no `Bearer ` prefix), then try the protected endpoints.

JWT settings (`Jwt` section in `appsettings.json`): `Key`, `Issuer`, `Audience`, `ExpiryMinutes`. The key there is for development only; use user-secrets or an environment variable in a real deployment.

## Postman tests

1. Import `Postman/AuthApi.postman_collection.json`.
2. Make sure the app is running and `baseUrl` (collection variable) matches your port.
3. Run the collection with the Collection Runner (or `newman run Postman/AuthApi.postman_collection.json`). Run the folders in order: registration first, since it stores the tokens used later.
4. Each request has a test asserting the expected status code. Registration emails are fixed (`user@test.com`, `admin@test.com`), so on a re-run change `userEmail` / `adminEmail` or clear the table; otherwise the first two registrations return 400.

Covered cases: register user and admin, duplicate email (400), login success, login failure (401), profile with token (200), no token / invalid token (401), user on `/api/user` (200), user on `/api/admin` (403), admin on `/api/admin` (200).

## Design choices

- **Password hashing:** BCrypt (`BCrypt.Net-Next`), which adds a random salt per password and stores it inside the hash. Plain text is never stored.
- **Unique email:** emails are lower-cased and have a unique index in the database, plus a check in the service so a duplicate returns a clean 400.
- **JWT:** created in `JwtTokenService` with HS256, carrying id, name, email and role claims. Issuer, audience, signature and expiry are all validated on every request.
- **Pipeline:** `UseAuthentication()` runs before `UseAuthorization()`. Missing/invalid token gives 401; a valid token with the wrong role gives 403 via `[Authorize(Roles = ...)]`.
- **Roles:** stored on the user as an enum (saved as text in the DB). `/api/profile` reads its data from the token claims.
- **Role at registration (demo only):** the register request accepts an optional `role` so the Postman suite can create an Admin without extra tooling. A production app should not let callers pick their own role; admins would be seeded or promoted by another admin.
- **Excluded on purpose:** UI, password reset, email verification, refresh tokens, social login, 2FA.

## Project layout

- `Controllers/` - `AuthController` (register/login), `ProfileController` (profile/user/admin)
- `Services/` - `IUserService` + `UserServiceImpl` (users and password check), `JwtTokenService`
- `Data/` - `AppDbcontext`; `Migrations/` - EF Core migration
- `Entities/`, `DTOs/`, `Settings/` - models, request objects, JWT config class
