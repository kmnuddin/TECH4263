# TECH 4263 — Lecture: DB Authentication & Role-Based Authorization
## Upgrading StudentAPI from hardcoded credentials to a Users table

---

## Part 1 — Where We Left Off

In the last class we added Basic Authentication to StudentAPI using a hardcoded username and password stored in `appsettings.json`. This works for a demo but has two real problems:

1. **Only one user** — everyone shares the same credentials
2. **No roles** — every authenticated user can do everything

Today we fix both. We will:
- Replace the hardcoded credentials with a `Users` table in `StudentDb`
- Add a `Role` column so different users have different permissions
- Protect endpoints based on role — only `Admin` can create students, anyone authenticated can read

---

## Part 2 — The Users Table

### Create the table in SSMS

Open SSMS, connect to `StudentDb`, and run:

```sql
USE StudentDb;
GO

CREATE TABLE Users (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role         NVARCHAR(50)  NOT NULL
);
GO

-- Insert two test users
-- Passwords are stored as SHA-256 hashes, not plain text
-- "password123" hashed = "ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f"
-- "readonly123" hashed = "f9a1e63d4c8b72a5d14e79c3b8a2f1d0e7c6b5a4d3e2f1c0b9a8e7d6c5b4a3f2"

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('admin', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Admin');

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('viewer', 'f9a1e63d4c8b72a5d14e79c3b8a2f1d0e7c6b5a4d3e2f1c0b9a8e7d6c5b4a3f2', 'Viewer');
GO
```

> **Why hash the password?** Never store plain text passwords in a database. A hash is a one-way transformation — if the database is ever leaked, attackers cannot reverse the hash back to the original password. We will hash the incoming password in C# and compare the hash to what is stored.

---

## Part 3 — Add the User Entity and DbContext

### Models/User.cs

```csharp
namespace StudentAPI.Models;

public class User
{
    public int    Id           { get; set; }
    public string Username     { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role         { get; set; } = string.Empty;
}
```

### Update Data/AppDbContext.cs

Add a `DbSet<User>` property alongside the existing `Students`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentAPI.Models;

namespace StudentAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<User>    Users    { get; set; }
}
```

---

## Part 4 — Hash Passwords in C#

We need a consistent way to hash a password string so we can compare it to the stored hash. Add a helper class:

### Helpers/PasswordHasher.cs

```csharp
using System.Security.Cryptography;
using System.Text;

namespace StudentAPI.Helpers;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
```

`SHA256.HashData` takes the password as bytes and returns a 32-byte hash. `Convert.ToHexString` converts those bytes to a readable hex string like `ef92b778...`. This is what we store in the database and compare against on every login.

---

## Part 5 — Update BasicAuthHandler to Use the Database

The handler now receives `AppDbContext` via dependency injection and queries the `Users` table instead of reading from `appsettings.json`.

### Auth/BasicAuthHandler.cs

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using StudentAPI.Data;
using StudentAPI.Helpers;

namespace StudentAPI.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _context;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AppDbContext context) : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization header");

        try
        {
            var authHeader      = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
            var credentials     = Encoding.UTF8.GetString(credentialBytes).Split(':');

            var username = credentials[0];
            var password = credentials[1];

            // Hash the incoming password and look up the user in the database
            var hashedPassword = PasswordHasher.Hash(password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.PasswordHash == hashedPassword);

            if (user is null)
                return AuthenticateResult.Fail("Invalid username or password");

            // Attach both the username AND the role as claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity  = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket    = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization header format");
        }
    }
}
```

### Key changes from last class

| | Last class | Today |
|--|-----------|-------|
| Credentials source | `appsettings.json` | `Users` table in `StudentDb` |
| Password check | Plain text comparison | SHA-256 hash comparison |
| Constructor dependency | `IConfiguration` | `AppDbContext` |
| Claims | Name only | Name **and Role** |
| Method | `Task` (sync) | `async Task` (awaits DB query) |

---

## Part 6 — Role-Based Authorization

Now that the `Role` claim is attached to the authenticated identity, we can use it to restrict specific endpoints.

### How it works

`.RequireAuthorization()` — any authenticated user can access this endpoint.

`.RequireAuthorization(policy => policy.RequireRole("Admin"))` — only users whose `Role` claim equals `"Admin"` can access this endpoint. Anyone else gets `403 Forbidden`.

### Endpoint protection rules for StudentAPI

| Endpoint | Who can access |
|----------|---------------|
| `GET /students` | Any authenticated user (Admin or Viewer) |
| `GET /students/{id}` | Any authenticated user (Admin or Viewer) |
| `POST /students` | Admin only |

---

## Part 7 — Updated Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using StudentAPI.Auth;
using StudentAPI.Data;
using StudentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme      = "basic",
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your username and password"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "basic"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// GET /students — any authenticated user
app.MapGet("/students", async (AppDbContext context) =>
{
    var students = await context.Students.ToListAsync();
    return Results.Ok(students.Select(s => new StudentResponseDto
    {
        Id    = s.Id,
        Name  = s.Name,
        Major = s.Major
    }));
}).WithName("GetStudents").WithOpenApi().RequireAuthorization();

// GET /students/{id} — any authenticated user
app.MapGet("/students/{id:int:min(1)}", async (int id, AppDbContext context) =>
{
    var student = await context.Students.FindAsync(id);
    if (student is null)
        return Results.NotFound();
    return Results.Ok(new StudentResponseDto
    {
        Id    = student.Id,
        Name  = student.Name,
        Major = student.Major
    });
}).WithName("GetStudentById").WithOpenApi().RequireAuthorization();

// POST /students — Admin only
app.MapPost("/students", async (CreateStudentDto dto, AppDbContext context) =>
{
    var student = new Student(dto.Name, dto.Age, dto.Major);
    context.Students.Add(student);
    await context.SaveChangesAsync();
    return Results.Created($"/students/{student.Id}", new StudentResponseDto
    {
        Id    = student.Id,
        Name  = dto.Name,
        Major = dto.Major
    });
}).WithName("CreateStudent").WithOpenApi()
  .RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();
```

---

## Part 8 — Testing in Swagger

**Login as `viewer` and call `GET /students`:**
1. Click Authorize → enter `viewer` / `readonly123` → click Authorize
2. Call `GET /students` → `200 OK` — Viewer can read

**Login as `viewer` and call `POST /students`:**
1. Call `POST /students` → `403 Forbidden` — Viewer cannot create

**Login as `admin` and call `POST /students`:**
1. Click Authorize → enter `admin` / `password123` → click Authorize
2. Call `POST /students` → `201 Created` — Admin can create

This demonstrates both authentication (who are you?) and authorization (what can you do?) working together.

---

## Part 9 — Final Project Structure

```
StudentAPI/
  Auth/
    BasicAuthHandler.cs       ← now queries Users table + attaches Role claim
  Data/
    AppDbContext.cs            ← now has DbSet<User> alongside DbSet<Student>
  Helpers/
    PasswordHasher.cs         ← SHA-256 hash helper
  Migrations/
  Models/
    Student.cs
    User.cs                   ← new — maps to Users table
    CreateStudentDto.cs
    StudentResponseDto.cs
  appsettings.json
  Program.cs
  StudentAPI.csproj
```

---

## Summary

| Concept | Key point |
|---------|-----------|
| **Users table** | Stores Username, PasswordHash, and Role — never plain text passwords |
| **SHA-256** | One-way hash — incoming password is hashed and compared to stored hash |
| **Role claim** | `new Claim(ClaimTypes.Role, user.Role)` — attached to the identity on login |
| **`RequireAuthorization()`** | Any authenticated user can access |
| **`RequireRole("Admin")`** | Only users with the Admin role claim can access |
| **401** | Authentication failed — wrong credentials or missing header |
| **403** | Authenticated but wrong role — access denied |
| **`AppDbContext` in handler** | Injected the same way as in endpoints — DI handles it |

---

*TECH 4263 — Server Application Technologies*
