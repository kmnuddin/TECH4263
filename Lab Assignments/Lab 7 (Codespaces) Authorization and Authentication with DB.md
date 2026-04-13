# TECH 4263 — Lab Assignment 7 (Codespaces Version)
## DB Authentication & Role-Based Authorization with StudentAPI
### Using SQLite

**Course:** TECH 4263 — Server Application Technologies
**Points:** 20
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

This is the Codespaces version of Lab 7. Since SQL Server cannot run in Codespaces, you will use **SQLite** as the database. The authentication and authorization logic is identical to the SQL Server version — only the database setup and connection string differ.

---

## Part 1 — Verify SQLite is Already Configured

If you completed the EF Core Codespaces guide earlier, your `StudentAPI` already has SQLite set up. Confirm by checking:

```bash
cat StudentAPI/StudentAPI/appsettings.json
```

You should see:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=student.db"
  }
}
```

And check the `.csproj` to confirm the SQLite package is installed:

```bash
cat StudentAPI/StudentAPI/StudentAPI.csproj
```

You should see `Microsoft.EntityFrameworkCore.Sqlite` in the package references. If not, install it:

```bash
cd StudentAPI/StudentAPI
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
```

---

## Part 2 — Add the User Entity

Create `Models/User.cs` with four properties: `Id`, `Username`, `PasswordHash`, and `Role`.

---

## Part 3 — Update AppDbContext

Add a `DbSet<User>` property to your existing `AppDbContext` alongside `DbSet<Student>`.

---

## Part 4 — Create a Migration for the Users Table

Since you are in Codespaces, you need to run the migration manually to add the `Users` table to your `student.db` file.

Make sure the `dotnet ef` tool is installed and on your PATH:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
export PATH="$PATH:$HOME/.dotnet/tools"
```

Create and apply the migration:

```bash
cd StudentAPI/StudentAPI
dotnet ef migrations add AddUsersTable
dotnet ef database update
```

This adds the `Users` table to `student.db` automatically. You do not need SSMS.

Confirm the migration ran by checking the database:

```bash
sqlite3 student.db ".tables"
```

You should see both `Students` and `Users` in the output.

---

## Part 5 — Add the Password Hasher

Create a `Helpers/` folder and add `PasswordHasher.cs` using SHA-256 — the same helper from the lecture. One static `Hash(string password)` method that returns the hex-encoded hash.

---

## Part 6 — Generate Hashes and Insert Users

Add a **temporary** endpoint to `Program.cs` to generate hashes:

```csharp
// Temporary — remove after use
app.MapGet("/hash/{password}", (string password) =>
    Results.Ok(StudentAPI.Helpers.PasswordHasher.Hash(password)));
```

Run the API:

```bash
dotnet run
```

Open the **Ports** tab, set the port to **Public**, and open `/swagger` in the browser. Call `/hash/adminpass` and `/hash/viewerpass` — copy both hashes.

Then insert the users directly into the SQLite database:

```bash
sqlite3 student.db
```

Inside the SQLite shell, run:

```sql
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('admin', 'paste-admin-hash-here', 'Admin');

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('viewer', 'paste-viewer-hash-here', 'Viewer');

SELECT * FROM Users;
.quit
```

Confirm both rows appear in the output, then **remove the `/hash/{password}` endpoint** from `Program.cs`.

---

## Part 7 — Create the Auth Handler

Create `Auth/BasicAuthHandler.cs`. The handler must:

- Inject `AppDbContext` in the constructor
- Read and decode the `Authorization` header
- Hash the incoming password using `PasswordHasher.Hash()`
- Query the `Users` table with `FirstOrDefaultAsync` to find a matching username and password hash
- Return `AuthenticateResult.Fail` if no user is found
- Attach `ClaimTypes.Name` and `ClaimTypes.Role` as claims on success

---

## Part 8 — Install Authentication Package and Update Program.cs

```bash
dotnet add package Microsoft.AspNetCore.Authentication
```

Make the following changes to `Program.cs`:

1. Add the two new `using` directives
2. Update `AddSwaggerGen` to include the Basic Auth security definition so the **Authorize** button appears
3. Register `AddAuthentication("BasicAuth")` with `BasicAuthHandler`
4. Register `AddAuthorization()`
5. Add `app.UseAuthentication()` and `app.UseAuthorization()` — in that order, before the endpoints
6. Protect endpoints with the rules below

### Authorization rules

| Endpoint | Who can access |
|----------|---------------|
| `GET /students` | Any authenticated user |
| `GET /students/{id}` | Any authenticated user |
| `POST /students` | Admin only |

---

## Part 9 — Run and Test in Swagger

Start the API:

```bash
dotnet run
```

Open the **Ports** tab → set port to **Public** → open `/swagger`.

Test the following scenarios:

**Scenario 1 — No credentials:**
Call `GET /students` without logging in → expect `401 Unauthorized`

**Scenario 2 — Viewer reads:**
Click Authorize → enter `viewer` / `viewerpass` → call `GET /students` → expect `200 OK`

**Scenario 3 — Viewer creates:**
With viewer credentials → call `POST /students` → expect `403 Forbidden`

**Scenario 4 — Admin creates:**
Click Authorize → enter `admin` / `adminpass` → call `POST /students` → expect `201 Created`

**Scenario 5 — Wrong password:**
Click Authorize → enter `admin` / `wrongpassword` → call `GET /students` → expect `401 Unauthorized`

---

## Troubleshooting

**`sqlite3` command not found:**
```bash
sudo apt-get install -y sqlite3
```

**`dotnet ef` not found after installing:**
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

**`FirstOrDefaultAsync` not found in BasicAuthHandler:**
Make sure you have this using directive at the top of `BasicAuthHandler.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
```

**Getting 401 with correct credentials:**
The hash in the database may not match what your `PasswordHasher` produces. Re-run the temporary `/hash/{password}` endpoint, copy the exact output, and update the Users table:
```bash
sqlite3 student.db
UPDATE Users SET PasswordHash = 'new-hash-here' WHERE Username = 'viewer';
SELECT * FROM Users;
.quit
```

---

## Grading Rubric

| | Task | Points |
|--|------|--------|
| 1 | `User` entity and `DbSet<User>` added to `AppDbContext` | 2 |
| 2 | Migration created and applied — `Users` table exists in `student.db` | 2 |
| 3 | `PasswordHasher.cs` implemented using SHA-256 | 2 |
| 4 | `BasicAuthHandler` queries the database and attaches Name and Role claims | 4 |
| 5 | Two users inserted with correctly hashed passwords | 2 |
| 6 | Swagger shows Authorize button — security definition configured | 1 |
| 7 | `GET` endpoints allow any authenticated user — Viewer gets `200` | 3 |
| 8 | `POST` endpoint restricted to Admin — Viewer gets `403`, Admin gets `201` | 4 |

---

## Submission Checklist

- [ ] `Models/User.cs` created with correct properties
- [ ] `DbSet<User>` added to `AppDbContext`
- [ ] Migration created and applied — `Users` table visible in `student.db`
- [ ] `Helpers/PasswordHasher.cs` created using SHA-256
- [ ] `Auth/BasicAuthHandler.cs` queries Users table, attaches Name and Role claims
- [ ] Two users (`admin` and `viewer`) inserted with hashed passwords
- [ ] Temporary `/hash/{password}` endpoint removed from `Program.cs`
- [ ] Swagger Authorize button visible
- [ ] `GET /students` returns `200` for both `admin` and `viewer`
- [ ] `POST /students` returns `403` for `viewer` and `201` for `admin`
- [ ] Unauthenticated requests return `401`
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 7 (Codespaces)*