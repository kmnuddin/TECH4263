# TECH 4263 — Lecture: Entity Framework Core + Repository Pattern
## Code First with StudentAPI

---

## Part 1 — From ADO.NET to EF Core

In the last class you wrote every SQL statement by hand — `SELECT`, `INSERT`, `SqlConnection`, `SqlCommand`, `SqlDataReader`. That works, but it has a cost:

- Every endpoint has the same boilerplate: open connection, create command, add parameters, execute, read, close
- If the `Students` table gains a new column, you update the SQL in every endpoint that touches it
- The SQL strings are plain text — no compiler checks, no IntelliSense, errors only show at runtime

**Entity Framework Core** removes all of that. You write C# — EF Core generates the SQL. The table schema stays in sync with your C# classes automatically through **migrations**.

### ADO.NET vs EF Core — side by side

```csharp
// ── ADO.NET ───────────────────────────────────────────────────────────────
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
using var command = new SqlCommand("SELECT Id, Name, Major FROM Students", connection);
using var reader = await command.ExecuteReaderAsync();
var list = new List<StudentResponseDto>();
while (await reader.ReadAsync())
{
    list.Add(new StudentResponseDto
    {
        Id    = reader.GetInt32(reader.GetOrdinal("Id")),
        Name  = reader.GetString(reader.GetOrdinal("Name")),
        Major = reader.GetString(reader.GetOrdinal("Major"))
    });
}
return Results.Ok(list);

// ── EF Core ───────────────────────────────────────────────────────────────
var students = await context.Students.ToListAsync();
return Results.Ok(students.Select(s => new StudentResponseDto
{
    Id = s.Id, Name = s.Name, Major = s.Major
}));
```

Same result. EF Core generates and runs the SQL — you never write it.

---

## Part 2 — Install EF Core Packages

Run these from inside the `StudentAPI/StudentAPI/` project folder:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

| Package | Purpose |
|---------|---------|
| `SqlServer` | EF Core provider that talks to SQL Server |
| `Tools` | Enables `dotnet ef` CLI commands |
| `Design` | Required for migrations and scaffolding |

---

## Part 3 — The Student Entity

The **entity** is a plain C# class that maps to a database table. EF Core uses conventions to figure out the mapping:

- Class `Student` → table `Students`
- Property `Id` → primary key, auto-incremented
- `string` → `NVARCHAR`, `int` → `INT`

Your existing `Student` model in `Models/Student.cs` is already the entity — no changes needed:

```csharp
// Models/Student.cs
namespace StudentAPI.Models;

public class Student
{
    public int    Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
    public int    Age   { get; set; }
    public string Major { get; set; } = string.Empty;
}
```

No SQL attributes, no database annotations. EF Core reads the class and figures out the schema automatically.

---

## Part 4 — Create the DbContext

The **DbContext** is the bridge between your C# code and the database. It represents a session with the database and exposes `DbSet<T>` properties — one per table.

Create a new folder `Data/` and add `AppDbContext.cs`:

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using StudentAPI.Models;

namespace StudentAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
}
```

`DbSet<Student>` is the EF Core equivalent of the `List<Student>` you used before — except it talks to the real database table. When you call `context.Students`, EF Core translates it into SQL against the `Students` table.

---

## Part 5 — Connection String and Registration

### appsettings.json

The connection string stays the same as the ADO.NET version — only the database name changes. Since you already have `StudentDb` from last class, keep it:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Register in Program.cs

Replace the old `var connectionString = ...` line with EF Core's service registration:

```csharp
// Remove this (ADO.NET):
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add this (EF Core):
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

This registers `AppDbContext` with the dependency injection container. EF Core will inject it into your endpoints automatically.

---

## Part 6 — Migrations

A **migration** is a versioned snapshot of your model. EF Core compares the current model to the last migration and generates SQL to update the schema.

Since `StudentDb` and the `Students` table already exist from last class, you need to tell EF Core about the existing state without re-running the `CREATE TABLE`.

### Option A — The table already exists (your situation)

Create an initial migration that matches the existing table, then mark it as already applied:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update --connection "Server=localhost;Database=StudentDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

EF Core will detect the table already exists and skip creating it. It only records the migration as applied.

### Option B — Fresh database (starting from scratch)

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

EF Core creates the database and table from your `Student` entity.

### Common migration commands

```bash
dotnet ef migrations add MigrationName   # create a new migration after model changes
dotnet ef database update                # apply all pending migrations
dotnet ef migrations list                # list all migrations and their status
dotnet ef migrations remove              # undo the last migration (before update only)
```

> After any change to the `Student` entity — adding a property, changing a type — always run `dotnet ef migrations add` followed by `dotnet ef database update` to keep the database in sync.

---

## Part 7 — CRUD Endpoints with EF Core

Now update the three endpoints in `Program.cs`. The `AppDbContext` is injected as a parameter — EF Core handles the connection automatically.

```csharp
using Microsoft.EntityFrameworkCore;
using StudentAPI.Data;
using StudentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /students
app.MapPost("/students", async (CreateStudentDto dto, AppDbContext context) =>
{
    var student = new Student
    {
        Name  = dto.Name,
        Age   = dto.Age,
        Major = dto.Major
    };

    context.Students.Add(student);     // stage the INSERT
    await context.SaveChangesAsync();  // execute INSERT, get generated Id

    return Results.Created($"/students/{student.Id}", new StudentResponseDto
    {
        Id    = student.Id,
        Name  = student.Name,
        Major = student.Major
    });
}).WithName("CreateStudent").WithOpenApi();

// GET /students
app.MapGet("/students", async (AppDbContext context) =>
{
    var students = await context.Students.ToListAsync();

    return Results.Ok(students.Select(s => new StudentResponseDto
    {
        Id    = s.Id,
        Name  = s.Name,
        Major = s.Major
    }));
}).WithName("GetStudents").WithOpenApi();

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
}).WithName("GetStudentById").WithOpenApi();

app.Run();
```

### What changed from ADO.NET

| Operation | ADO.NET | EF Core |
|-----------|---------|---------|
| Get all | `ExecuteReaderAsync` + `ReadAsync` loop | `context.Students.ToListAsync()` |
| Get one | `ExecuteReaderAsync` + `AddWithValue("@Id")` | `context.Students.FindAsync(id)` |
| Insert | `ExecuteScalarAsync` + `OUTPUT INSERTED.Id` | `context.Students.Add()` + `SaveChangesAsync()` |
| Connection | Manual `SqlConnection` + `OpenAsync` | Injected `AppDbContext` — no manual connection |
| Parameters | `AddWithValue` for every value | Not needed — EF Core parameterizes automatically |

---

## Part 8 — The Problem EF Core Doesn't Solve

EF Core is cleaner than raw ADO.NET, but all the database logic is still inside `Program.cs`. If you have 20 endpoints, `Program.cs` becomes 500 lines of mixed concerns:

- HTTP routing
- Input validation
- Database access
- DTO mapping

And if you want to change the database technology — say, swap SQL Server for SQLite — you have to touch every endpoint.

This is the problem the **Repository Pattern** solves.

---

## Part 9 — What is the Repository Pattern?

A **repository** is a class that owns all the database access logic for one entity. The endpoints talk to the repository — they have no knowledge of EF Core, `DbContext`, or SQL.

```
Before (everything in Program.cs):
Endpoint → DbContext → Database

After (Repository Pattern):
Endpoint → IStudentRepository → StudentRepository → DbContext → Database
```

The key is the **interface** (`IStudentRepository`). The endpoint depends on the interface, not the concrete implementation. This means:

- You can swap `StudentRepository` for a different implementation without touching the endpoints
- You can write tests by replacing the real repository with a fake one

---

## Part 10 — Define the Repository Interface

Create a new folder `Repositories/` and add the interface:

```csharp
// Repositories/IStudentRepository.cs
using StudentAPI.Models;

namespace StudentAPI.Repositories;

public interface IStudentRepository
{
    Task<IEnumerable<StudentResponseDto>> GetAllAsync();
    Task<StudentResponseDto?> GetByIdAsync(int id);
    Task<StudentResponseDto> CreateAsync(CreateStudentDto dto);
}
```

The interface defines **what** the repository can do — not **how** it does it. The endpoints only need to know about this interface.

---

## Part 11 — Implement the Repository

Add the concrete implementation in the same folder:

```csharp
// Repositories/StudentRepository.cs
using Microsoft.EntityFrameworkCore;
using StudentAPI.Data;
using StudentAPI.Models;

namespace StudentAPI.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentResponseDto>> GetAllAsync()
    {
        var students = await _context.Students.ToListAsync();

        return students.Select(s => new StudentResponseDto
        {
            Id    = s.Id,
            Name  = s.Name,
            Major = s.Major
        });
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student is null)
            return null;

        return new StudentResponseDto
        {
            Id    = student.Id,
            Name  = student.Name,
            Major = student.Major
        };
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
    {
        var student = new Student
        {
            Name  = dto.Name,
            Age   = dto.Age,
            Major = dto.Major
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return new StudentResponseDto
        {
            Id    = student.Id,
            Name  = student.Name,
            Major = student.Major
        };
    }
}
```

The `AppDbContext` is injected through the constructor — `StudentRepository` does not create it or manage its lifetime. That is handled by the dependency injection container.

---

## Part 12 — Register the Repository and Update Program.cs

### Register in Program.cs

Add one line to register the repository with the DI container:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the repository — interface mapped to implementation
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
```

`AddScoped` means one instance of `StudentRepository` is created per HTTP request and disposed when the request ends.

### Updated endpoints

The endpoints are now clean — no EF Core, no DTOs mapping, no database logic. Just HTTP concerns:

```csharp
using StudentAPI.Data;
using StudentAPI.Models;
using StudentAPI.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /students
app.MapPost("/students", async (CreateStudentDto dto, IStudentRepository repo) =>
{
    var result = await repo.CreateAsync(dto);
    return Results.Created($"/students/{result.Id}", result);
}).WithName("CreateStudent").WithOpenApi();

// GET /students
app.MapGet("/students", async (IStudentRepository repo) =>
{
    var students = await repo.GetAllAsync();
    return Results.Ok(students);
}).WithName("GetStudents").WithOpenApi();

// GET /students/{id}
app.MapGet("/students/{id:int:min(1)}", async (int id, IStudentRepository repo) =>
{
    var student = await repo.GetByIdAsync(id);
    return student is null ? Results.NotFound() : Results.Ok(student);
}).WithName("GetStudentById").WithOpenApi();

app.Run();
```

Notice the endpoints now receive `IStudentRepository repo` — not `AppDbContext`. They have no idea whether the data comes from SQL Server, SQLite, or a flat file. That detail lives inside `StudentRepository`.

---

## Part 13 — Final Project Structure

```
StudentAPI/
  Data/
    AppDbContext.cs              ← DbContext — bridge to the database
  Migrations/
    TIMESTAMP_InitialCreate.cs  ← generated by dotnet ef migrations add
    AppDbContextModelSnapshot.cs
  Models/
    Student.cs                  ← entity — maps to Students table
    CreateStudentDto.cs         ← request DTO
    StudentResponseDto.cs       ← response DTO
  Repositories/
    IStudentRepository.cs       ← interface — what the repo can do
    StudentRepository.cs        ← implementation — how it does it
  appsettings.json              ← connection string
  Program.cs                    ← endpoints + service registrations
  StudentAPI.csproj
```

---

## Part 14 — Summary

| Concept | What it does |
|---------|-------------|
| **Entity** | C# class that maps to a database table — no SQL needed |
| **DbContext** | Session with the database — injected by DI, one per request |
| **DbSet\<T\>** | Represents a table — `ToListAsync()`, `FindAsync()`, `Add()` |
| **Migration** | Versioned snapshot — keeps DB schema in sync with your model |
| **SaveChangesAsync()** | Sends all staged changes to the database in one transaction |
| **Repository interface** | Defines what operations are available — endpoints depend on this |
| **Repository implementation** | Contains all EF Core code — endpoints never touch DbContext directly |
| **AddScoped** | One repository instance per HTTP request — correct lifetime for DbContext |

### The three-layer progression

```
Class 1 — ADO.NET
  Endpoint → SqlConnection → SQL Server
  (manual SQL, manual connections, manual mapping)

Class 2 — EF Core
  Endpoint → AppDbContext → SQL Server
  (C# instead of SQL, automatic mapping)

Today — EF Core + Repository Pattern
  Endpoint → IStudentRepository → StudentRepository → AppDbContext → SQL Server
  (endpoints know nothing about the database — clean separation)
```

---

*TECH 4263 — Server Application Technologies*
