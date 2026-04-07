# TECH 4263 — Lecture: Authentication in REST APIs
## Basic Authentication with StudentAPI

---

## Part 1 — Authentication vs Authorization

Two terms that are often confused:

**Authentication** — proving who you are. The API asks: *"Who is making this request?"*

**Authorization** — deciding what you are allowed to do. The API asks: *"Is this person allowed to do this?"*

```
Request arrives
      ↓
Authentication: Who are you?      → Invalid credentials → 401 Unauthorized
      ↓
Authorization:  What can you do?  → Not allowed         → 403 Forbidden
      ↓
Handler:        Process request   → Success             → 200 OK
```

**401 Unauthorized** — you did not prove who you are.
**403 Forbidden** — you proved who you are, but you do not have permission.

---

## Part 2 — What is Basic Authentication?

Basic Authentication is the simplest authentication method in HTTP. The client sends a username and password with every request inside the `Authorization` header.

The credentials are combined into the format `username:password`, encoded in Base64, and sent like this:

```
Authorization: Basic dXNlcjpwYXNzd29yZA==
```

Breaking it down:
- `Authorization` — the standard HTTP header name
- `Basic` — the authentication scheme
- `dXNlcjpwYXNzd29yZA==` — Base64 encoding of `user:password`

**Important:** Base64 is not encryption. Anyone who intercepts the request can decode it instantly. This is why Basic Auth must always be used over **HTTPS**.

---

## Part 3 — How to Encode Credentials

```csharp
// In C#
var credentials = "admin:password123";
var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
// Result: YWRtaW46cGFzc3dvcmQxMjM=
```

The encoded value goes directly into the header:
```
Authorization: Basic YWRtaW46cGFzc3dvcmQxMjM=
```

---

## Part 4 — Adding Basic Auth to StudentAPI

We will protect all three existing endpoints. Any request without valid credentials will receive `401 Unauthorized`.

### Step 1 — Install the package

```bash
dotnet add package Microsoft.AspNetCore.Authentication
```

### Step 2 — Configure Swagger to show the Authorize button

By default Swagger has no knowledge that the API uses authentication. Replace the existing `builder.Services.AddSwaggerGen()` line with the following so Swagger shows an **Authorize** button and attaches credentials to requests:

```csharp
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
```

`AddSecurityDefinition` tells the OpenAPI spec that this API uses HTTP Basic Auth. `AddSecurityRequirement` tells it that all endpoints require it. Together these two calls make the **Authorize** button appear in Swagger UI.

### Step 3 — Add credentials to appsettings.json

```json
{
  "BasicAuth": {
    "Username": "admin",
    "Password": "password123"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Step 4 — Create the Auth Handler

Create a new folder `Auth/` and add `BasicAuthHandler.cs`:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace StudentAPI.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _config;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration config) : base(options, logger, encoder)
    {
        _config = config;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

        try
        {
            var authHeader      = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
            var credentials     = Encoding.UTF8.GetString(credentialBytes).Split(':');

            var username = credentials[0];
            var password = credentials[1];

            var validUsername = _config["BasicAuth:Username"];
            var validPassword = _config["BasicAuth:Password"];

            if (username != validUsername || password != validPassword)
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));

            var claims    = new[] { new Claim(ClaimTypes.Name, username) };
            var identity  = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket    = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
        }
    }
}
```

---

## Part 5 — Understanding BasicAuthHandler.cs

Every time a request arrives at a protected endpoint, ASP.NET Core calls `HandleAuthenticateAsync()`. Here is what each section does:

### The class declaration

```csharp
public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
```

`BasicAuthHandler` inherits from `AuthenticationHandler` — a built-in ASP.NET Core base class for writing custom authentication logic. By inheriting from it, we only need to implement one method: `HandleAuthenticateAsync()`. Everything else — hooking into the request pipeline, returning 401 responses — is handled by the base class automatically.

### The constructor

```csharp
public BasicAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration config) : base(options, logger, encoder)
{
    _config = config;
}
```

The first three parameters (`options`, `logger`, `encoder`) are required by the base class and passed straight to it via `: base(...)`. We do not use them directly.

`IConfiguration config` is injected by ASP.NET Core's dependency injection system and gives us access to `appsettings.json`. We store it in `_config` so we can read the valid username and password later.

### Step 1 — Check the header exists

```csharp
if (!Request.Headers.ContainsKey("Authorization"))
    return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
```

Before doing anything else, check whether the `Authorization` header was included in the request at all. If it is missing, there is nothing to validate — return a failure result immediately. The base class converts this into a `401 Unauthorized` response.

### Step 2 — Parse the header value

```csharp
var authHeader      = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
var credentials     = Encoding.UTF8.GetString(credentialBytes).Split(':');
```

`AuthenticationHeaderValue.Parse` splits the header into two parts: the scheme (`Basic`) and the parameter (the Base64 string).

`Convert.FromBase64String` decodes the Base64 string back into raw bytes.

`Encoding.UTF8.GetString` converts those bytes into a plain string — `"admin:password123"`.

`.Split(':')` splits on the colon to separate the username from the password, giving us a two-element array.

### Step 3 — Validate against appsettings.json

```csharp
var username = credentials[0];
var password = credentials[1];

var validUsername = _config["BasicAuth:Username"];
var validPassword = _config["BasicAuth:Password"];

if (username != validUsername || password != validPassword)
    return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
```

`credentials[0]` is the username, `credentials[1]` is the password.

`_config["BasicAuth:Username"]` reads the value from `appsettings.json` under the `BasicAuth` section. If either the username or password does not match, return another failure — which becomes a `401 Unauthorized`.

### Step 4 — Build the authenticated identity

```csharp
var claims    = new[] { new Claim(ClaimTypes.Name, username) };
var identity  = new ClaimsIdentity(claims, Scheme.Name);
var principal = new ClaimsPrincipal(identity);
var ticket    = new AuthenticationTicket(principal, Scheme.Name);

return Task.FromResult(AuthenticateResult.Success(ticket));
```

If credentials are valid, we build an **authentication ticket** — the object that represents the authenticated user for the rest of the request.

A **Claim** is a piece of information about the user. Here we attach the username as the `Name` claim. In more advanced systems you would add role claims, email claims, etc.

A `ClaimsIdentity` wraps the claims into an identity object.

A `ClaimsPrincipal` wraps the identity — this is the `user` object that endpoints can access via `ClaimsPrincipal user` if they need to know who is calling.

`AuthenticationTicket` packages everything together and `AuthenticateResult.Success(ticket)` tells ASP.NET Core the request is authenticated. The pipeline continues and the endpoint handler runs.

### The try/catch

```csharp
catch
{
    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
}
```

If the Base64 string is malformed or the split produces unexpected results, the whole block would throw an exception. The catch handles this gracefully and returns a `401` instead of crashing the API.

---

## Part 6 — Update Program.cs

Three changes from the current version:
1. Register authentication and authorization before `builder.Build()`
2. Add `app.UseAuthentication()` and `app.UseAuthorization()` to the pipeline
3. Add `.RequireAuthorization()` to each endpoint

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using StudentAPI.Auth;
using StudentAPI.Data;
using StudentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Basic Authentication
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

// These two lines must come in this order
app.UseAuthentication();
app.UseAuthorization();

// POST /students
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
}).WithName("CreateStudent").WithOpenApi().RequireAuthorization();

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

// GET /students/{id}
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

app.Run();
```

The only lines added compared to the existing `Program.cs`:

```
+ using Microsoft.AspNetCore.Authentication;
+ using StudentAPI.Auth;

+ builder.Services.AddAuthentication("BasicAuth")
+     .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);
+ builder.Services.AddAuthorization();

+ app.UseAuthentication();
+ app.UseAuthorization();

+ .RequireAuthorization()   ← on each of the three endpoints
```

---

## Part 7 — Testing in Swagger

Swagger does not attach credentials automatically. Without them every protected endpoint returns `401`.

1. Click the **Authorize** button at the top of the Swagger page
2. Enter `admin` as Username and `password123` as Password
3. Click **Authorize** — Swagger encodes and attaches the header to all subsequent requests

---

## Part 8 — What the Requests Look Like

**Without credentials:**
```
GET /students HTTP/1.1

→ 401 Unauthorized
```

**With wrong credentials:**
```
GET /students HTTP/1.1
Authorization: Basic d3Jvbmc6Y3JlZGVudGlhbHM=

→ 401 Unauthorized
```

**With correct credentials:**
```
GET /students HTTP/1.1
Authorization: Basic YWRtaW46cGFzc3dvcmQxMjM=

→ 200 OK
[{ "id": 1, "name": "Alice", "major": "Computer Science" }]
```

---

## Part 9 — Final Project Structure

```
StudentAPI/
  Auth/
    BasicAuthHandler.cs       ← validates credentials on every request
  Data/
    AppDbContext.cs
  Migrations/
  Models/
    Student.cs
    CreateStudentDto.cs
    StudentResponseDto.cs
  appsettings.json             ← stores valid username and password
  Program.cs                   ← registers auth + protects endpoints
  StudentAPI.csproj
```

---

## Summary

| Concept | Key point |
|---------|-----------|
| **Basic Auth** | `Authorization: Basic <base64(username:password)>` sent on every request |
| **Base64** | Encoding only — not encryption. Always use HTTPS |
| **`AuthenticationHandler`** | Base class — we only implement `HandleAuthenticateAsync()` |
| **`AuthenticateResult.Fail`** | Returns 401 — credentials missing or wrong |
| **`AuthenticateResult.Success`** | Request is authenticated — endpoint handler runs |
| **Claims** | Facts about the authenticated user stored in the ticket |
| **401** | Missing or invalid credentials |
| **403** | Authenticated but not allowed |
| **`UseAuthentication()`** | Must come before `UseAuthorization()` in the pipeline |
| **`.RequireAuthorization()`** | Protects an individual endpoint |
| **`appsettings.json`** | Where credentials live — never hardcode them in source code |

---

*TECH 4263 — Server Application Technologies*
