# TECH 4263 — Lab Assignment 7
## DB Authentication & Role-Based Authorization with EquipmentAPI

**Course:** TECH 4263 — Server Application Technologies
**Points:** 20
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

In this lab you will add database-backed authentication and role-based authorization to your `EquipmentAPI`. Users will be stored in a `Users` table in `EquipmentDb`, passwords will be hashed, and different roles will have different levels of access to the API endpoints.

Use the `StudentAPI` from today's lecture as your reference. The pattern is identical — only the table name and endpoint rules are different.

---

## Part 1 — Create the Users Table

Open SSMS, connect to `EquipmentDb`, and run the following:

```sql
USE EquipmentDb;
GO

CREATE TABLE Users (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role         NVARCHAR(50)  NOT NULL
);
GO
```

Do not insert any users yet — you will generate the correct password hashes in Part 5 and insert them then.

---

## Part 2 — Add the User Entity

Create `Models/User.cs` following the same structure as the `StudentAPI` lecture.

The `User` model needs four properties: `Id`, `Username`, `PasswordHash`, and `Role`.

---

## Part 3 — Update AppDbContext

Add a `DbSet<User>` property to your existing `AppDbContext` alongside `DbSet<Equipment>`.

---

## Part 4 — Add the Password Hasher

Create a `Helpers/` folder and add `PasswordHasher.cs`. Use SHA-256 hashing — the same helper from the lecture. This class should have one static method `Hash(string password)` that returns the hex-encoded hash.

---

## Part 5 — Generate Password Hashes and Insert Users

Add a **temporary** endpoint to `Program.cs` to generate hashes:

```csharp
// Temporary — remove after use
app.MapGet("/hash/{password}", (string password) =>
    Results.Ok(StudentAPI.Helpers.PasswordHasher.Hash(password)));
```

> Make sure to use the correct namespace — `EquipmentAPI.Helpers` not `StudentAPI.Helpers`.

Run the API, call `/hash/adminpass` in Swagger, copy the returned hash. Do the same for `/hash/viewerpass`. Then insert both users into SSMS:

```sql
USE EquipmentDb;
GO

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('admin', 'paste-admin-hash-here', 'Admin');

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('viewer', 'paste-viewer-hash-here', 'Viewer');
GO
```

Once the users are inserted, **remove the `/hash/{password}` endpoint** from `Program.cs`.

---

## Part 6 — Create the Auth Handler

Create an `Auth/` folder and add `BasicAuthHandler.cs`.

The handler must:

- Inject `AppDbContext` in the constructor
- Read and decode the `Authorization` header
- Hash the incoming password using `PasswordHasher.Hash()`
- Query the `Users` table to find a matching username and password hash
- Return `AuthenticateResult.Fail` if no user is found
- Attach **two claims** if authentication succeeds — `ClaimTypes.Name` and `ClaimTypes.Role`

> Refer to the `BasicAuthHandler.cs` from the lecture. The only differences are the namespace (`EquipmentAPI`) and the context type (`AppDbContext` with `DbSet<User>`).

---

## Part 7 — Update Program.cs

Make the following changes to `Program.cs`:

### Install the package first

```bash
dotnet add package Microsoft.AspNetCore.Authentication
```

### Changes to make

1. Add the two new `using` directives at the top
2. Update `AddSwaggerGen` to include the Basic Auth security definition so the **Authorize** button appears in Swagger
3. Register `AddAuthentication("BasicAuth")` with the `BasicAuthHandler` scheme
4. Register `AddAuthorization()`
5. Add `app.UseAuthentication()` and `app.UseAuthorization()` — in that order, before the endpoints
6. Protect the endpoints with the rules below

### Authorization rules

| Endpoint | Who can access |
|----------|---------------|
| `GET /equipments` | Any authenticated user |
| `GET /equipments/{id}` | Any authenticated user |
| `POST /equipments` | Admin only |

Use `.RequireAuthorization()` for any authenticated user and `.RequireAuthorization(policy => policy.RequireRole("Admin"))` for Admin only.

---

## Part 8 — Test in Swagger

Open Swagger and verify the following scenarios:

**Scenario 1 — No credentials:**
Call `GET /equipments` without logging in → expect `401 Unauthorized`

**Scenario 2 — Viewer reads:**
Click Authorize → enter `viewer` / `viewerpass` → call `GET /equipments` → expect `200 OK`

**Scenario 3 — Viewer creates:**
With viewer credentials → call `POST /equipments` → expect `403 Forbidden`

**Scenario 4 — Admin creates:**
Click Authorize → enter `admin` / `adminpass` → call `POST /equipments` → expect `201 Created`

**Scenario 5 — Wrong password:**
Click Authorize → enter `admin` / `wrongpassword` → call `GET /equipments` → expect `401 Unauthorized`

---

## Grading Rubric

| | Task | Points |
|--|------|--------|
| 1 | `Users` table created in `EquipmentDb` with correct columns | 2 |
| 2 | `User` entity and `DbSet<User>` added to `AppDbContext` | 2 |
| 3 | `PasswordHasher.cs` implemented using SHA-256 | 2 |
| 4 | `BasicAuthHandler` queries the database and attaches Name and Role claims | 4 |
| 5 | Two users inserted with correctly hashed passwords | 2 |
| 6 | Swagger shows Authorize button — security definition configured | 1 |
| 7 | `GET` endpoints allow any authenticated user — Viewer gets `200` | 3 |
| 8 | `POST` endpoint restricted to Admin — Viewer gets `403`, Admin gets `201` | 4 |

---

## Submission Checklist

- [ ] `Users` table created in `EquipmentDb`
- [ ] `Models/User.cs` created with correct properties
- [ ] `DbSet<User>` added to `AppDbContext`
- [ ] `Helpers/PasswordHasher.cs` created using SHA-256
- [ ] `Auth/BasicAuthHandler.cs` queries Users table, attaches Name and Role claims
- [ ] Two users (`admin` and `viewer`) inserted with hashed passwords
- [ ] Temporary `/hash/{password}` endpoint removed from `Program.cs`
- [ ] Swagger Authorize button visible
- [ ] `GET /equipments` returns `200` for both `admin` and `viewer`
- [ ] `POST /equipments` returns `403` for `viewer` and `201` for `admin`
- [ ] Unauthenticated requests return `401`
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 7*
