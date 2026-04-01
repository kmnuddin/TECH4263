# EF Core with StudentAPI in Codespaces
## Guide for Chromebook / Codespaces Users

---

## Important — SQL Server Cannot Run in Codespaces

SQL Server requires Windows or a Docker container with significant resources. Codespaces runs on Linux and cannot host SQL Server directly.

The solution is to use **SQLite** instead. SQLite is a lightweight database that runs as a single file inside your project — no server installation needed. EF Core works exactly the same way with SQLite as it does with SQL Server. The only difference is the provider package and one line in `Program.cs`.

Your code for `DbContext`, entities, DTOs, endpoints, and repository stays identical.

---

## Part 1 — Check Your .NET Version

Open a terminal in your Codespace and run:

```bash
cat StudentAPI/StudentAPI/StudentAPI.csproj
```

Note the `<TargetFramework>` value — it will be `net8.0` or `net9.0`. You need this for the next step.

---

## Part 2 — Install EF Core Packages

Navigate into the StudentAPI project folder:

```bash
cd StudentAPI/StudentAPI
```

Install the SQLite provider and Tools packages, pinned to match your target framework.

**If your project targets `net8.0`:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
```

**If your project targets `net9.0`:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
```

> You do not need `Microsoft.EntityFrameworkCore.SqlServer` — that is the Windows/SQL Server provider. `Sqlite` is the Codespaces-compatible replacement.

---

## Part 3 — Add the Connection String

Open `StudentAPI/StudentAPI/appsettings.json` and add a `ConnectionStrings` section:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=student.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

`Data Source=student.db` tells SQLite to create a database file named `student.db` in the project folder. EF Core creates this file automatically when the app first runs — no SSMS, no setup needed.

---

## Part 4 — Update the Student Entity

Open `Models/Student.cs`. Remove any constructor that manually assigns `Id` using a counter — SQLite will generate the `Id` automatically.

Add the `[DatabaseGenerated]` attribute to the `Id` property:

```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAPI.Models;

public class Student
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int    Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
    public int    Age   { get; set; }
    public string Major { get; set; } = string.Empty;
}
```

---

## Part 5 — Create the DbContext

Create a `Data/` folder inside the project:

```bash
mkdir Data
```

Create `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentAPI.Models;

namespace StudentAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
}
```

This is identical to the SQL Server version — `DbContext` does not care which database provider is used.

---

## Part 6 — Register EF Core in Program.cs

Open `Program.cs`. Add the following using directives at the top:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentAPI.Data;
```

Register `AppDbContext` with SQLite **before** `builder.Build()`:

```csharp
// Replace this (ADO.NET):
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add this (EF Core with SQLite):
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

> Note: the method is `UseSqlite()` not `UseSqlServer()` — this is the only line that differs from the SQL Server version.

Remove the `var students = new List<Student>();` line — it is no longer needed.

---

## Part 7 — Apply the Migration

Install the `dotnet ef` global tool:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

Add the tools folder to your PATH so the command is found:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

Make it permanent across terminal restarts:

```bash
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

Create and apply the migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the `student.db` file and the `Students` table inside it. You will see a `Migrations/` folder appear in the project.

---

## Part 8 — Update the Three Endpoints

Replace the ADO.NET code in each endpoint with EF Core. Inject `AppDbContext` as a parameter — the pattern is identical to what was shown in class.

**POST /students** — inject `AppDbContext context`, use `context.Students.Add()` and `await context.SaveChangesAsync()`

**GET /students** — use `await context.Students.ToListAsync()`

**GET /students/{id}** — use `await context.Students.FindAsync(id)`, return `Results.NotFound()` if null

Keep `.WithName().WithOpenApi()` on all endpoints.

---

## Part 9 — Run and Test

Start the API from the StudentAPI project folder:

```bash
dotnet run
```

Open the **Ports** tab in Codespaces, set the port to **Public**, copy the local address, and open `/swagger` in a browser tab.

Test in order:

1. `POST /students` — create a student, confirm `201` with a generated `Id`
2. `GET /students` — confirm the student appears
3. `GET /students/{id}` — confirm all fields return correctly
4. Stop with `Ctrl + C`, run `dotnet run` again, call `GET /students` — data must still be there since it is saved in `student.db`

---

## Difference Summary — SQL Server vs SQLite in Codespaces

| | SQL Server (VS 2022) | SQLite (Codespaces) |
|--|---------------------|---------------------|
| Package | `EntityFrameworkCore.SqlServer` | `EntityFrameworkCore.Sqlite` |
| Connection string | `Server=localhost;Database=StudentDb;...` | `Data Source=student.db` |
| Registration | `options.UseSqlServer(...)` | `options.UseSqlite(...)` |
| Database location | SQL Server instance | `student.db` file in project folder |
| Setup required | SSMS + SQL Server install | Nothing — file created automatically |
| `DbContext`, entities, endpoints | Identical | Identical |

The migration step is done manually in Codespaces via `dotnet ef` because Visual Studio Community's automatic migration feature is not available in the terminal environment.

---

*TECH 4263 — Server Application Technologies*
