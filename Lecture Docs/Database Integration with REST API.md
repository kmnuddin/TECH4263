# TECH 4263 — Lecture: Database Communication Through APIs
## ADO.NET + SQL Server

---

## Part 1 — Why Raw SQL First?

Before using any ORM or helper library, it is important to understand what actually happens when an application talks to a database. ADO.NET is the foundation that everything else is built on — Entity Framework Core, Dapper, and every other .NET database library all use ADO.NET under the hood.

Learning ADO.NET first means you understand:
- How a connection to SQL Server is opened and closed
- How a SQL command is built and executed
- How rows are read back into C# objects
- What a SQL injection vulnerability actually looks like — and how parameters prevent it

```
Your C# Code
     ↓
  ADO.NET
  (SqlConnection / SqlCommand / SqlDataReader)
     ↓
SQL Server Driver
     ↓
SQL Server Database
```

---

## Part 2 — The Four Core ADO.NET Objects

Every database operation in ADO.NET uses some combination of these four classes:

| Class | Purpose |
|-------|---------|
| `SqlConnection` | Opens and manages the connection to SQL Server |
| `SqlCommand` | Holds the SQL query or stored procedure to execute |
| `SqlDataReader` | Reads rows returned by a SELECT query, one at a time |
| `SqlParameter` | Passes values into a query safely — prevents SQL injection |

All four live in the `Microsoft.Data.SqlClient` namespace.

---

## Part 3 — Setup

### Install the NuGet package

```bash
dotnet add package Microsoft.Data.SqlClient
```

### Create the database and table in SQL Server

Open SQL Server Management Studio (SSMS) and run:

```sql
CREATE DATABASE StudentDb;
GO

USE StudentDb;
GO

CREATE TABLE Students (
    Id    INT           IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(100) NOT NULL,
    Age   INT           NOT NULL,
    Major NVARCHAR(100) NOT NULL
);
GO
```

`IDENTITY(1,1)` means SQL Server auto-increments the `Id` column — you never set it manually.

### Add the connection string to appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Read the connection string in Program.cs

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

---

## Part 4 — The Connection Lifecycle

Every database operation follows the same pattern:

```
1. Open connection
2. Create command with SQL
3. Add parameters
4. Execute command
5. Read results (if SELECT)
6. Close connection
```

Always open the connection as late as possible and close it as soon as you are done. The `using` statement handles closing automatically — even if an exception is thrown.

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

// ... do work here ...

// connection closes automatically when the using block exits
```

---

## Part 5 — SELECT All Rows

```csharp
// GET /students
app.MapGet("/students", async () =>
{
    var students = new List<StudentResponseDto>();

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand("SELECT Id, Name, Major FROM Students", connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        students.Add(new StudentResponseDto
        {
            Id    = reader.GetInt32(reader.GetOrdinal("Id")),
            Name  = reader.GetString(reader.GetOrdinal("Name")),
            Major = reader.GetString(reader.GetOrdinal("Major"))
        });
    }

    return Results.Ok(students);
});
```

### How the reader works

`reader.ReadAsync()` moves to the next row and returns `true` if a row exists, `false` when there are no more rows. Inside the loop, `reader.GetString("Name")` reads the value of the `Name` column for the current row.

`GetOrdinal("Name")` converts the column name to its index number — this is safer than hardcoding `reader.GetString(1)` because column order can change.

---

## Part 6 — SELECT One Row by ID

```csharp
// GET /students/{id}
app.MapGet("/students/{id:int:min(1)}", async (int id) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand(
        "SELECT Id, Name, Major FROM Students WHERE Id = @Id", connection);

    // Always use parameters — never concatenate user input into SQL
    command.Parameters.AddWithValue("@Id", id);

    using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound();

    return Results.Ok(new StudentResponseDto
    {
        Id    = reader.GetInt32(reader.GetOrdinal("Id")),
        Name  = reader.GetString(reader.GetOrdinal("Name")),
        Major = reader.GetString(reader.GetOrdinal("Major"))
    });
});
```

### Why @Id instead of string concatenation?

**Never do this:**
```csharp
// DANGEROUS — SQL injection vulnerability
var sql = $"SELECT * FROM Students WHERE Id = {id}";
```

**Always do this:**
```csharp
// SAFE — parameterized query
var sql = "SELECT * FROM Students WHERE Id = @Id";
command.Parameters.AddWithValue("@Id", id);
```

With string concatenation, a malicious user could send `1; DROP TABLE Students` as the ID. With parameters, the value is always treated as data — never as SQL code.

---

## Part 7 — INSERT a New Row

```csharp
// POST /students
app.MapPost("/students", async (CreateStudentDto dto) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    // OUTPUT INSERTED.Id returns the auto-generated Id from SQL Server
    using var command = new SqlCommand(
        @"INSERT INTO Students (Name, Age, Major)
          OUTPUT INSERTED.Id
          VALUES (@Name, @Age, @Major)", connection);

    command.Parameters.AddWithValue("@Name",  dto.Name);
    command.Parameters.AddWithValue("@Age",   dto.Age);
    command.Parameters.AddWithValue("@Major", dto.Major);

    // ExecuteScalarAsync returns the single value from OUTPUT INSERTED.Id
    var newId = (int)(await command.ExecuteScalarAsync())!;

    return Results.Created($"/students/{newId}", new StudentResponseDto
    {
        Id    = newId,
        Name  = dto.Name,
        Major = dto.Major
    });
});
```

### OUTPUT INSERTED.Id

SQL Server's `OUTPUT INSERTED.Id` clause returns the auto-generated primary key immediately after the INSERT. This is the standard way to get the new ID back without running a second SELECT query.

---

## Part 8 — UPDATE an Existing Row

```csharp
// PUT /students/{id}
app.MapPut("/students/{id:int:min(1)}", async (int id, CreateStudentDto dto) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand(
        @"UPDATE Students
          SET Name = @Name, Age = @Age, Major = @Major
          WHERE Id = @Id", connection);

    command.Parameters.AddWithValue("@Name",  dto.Name);
    command.Parameters.AddWithValue("@Age",   dto.Age);
    command.Parameters.AddWithValue("@Major", dto.Major);
    command.Parameters.AddWithValue("@Id",    id);

    // ExecuteNonQueryAsync returns the number of rows affected
    int rowsAffected = await command.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
        return Results.NotFound();

    return Results.NoContent();
});
```

`ExecuteNonQueryAsync()` is used for INSERT, UPDATE, and DELETE — any command that does not return rows. It returns the number of rows affected. If `rowsAffected == 0`, no row matched the WHERE clause — the ID did not exist.

---

## Part 9 — DELETE a Row

```csharp
// DELETE /students/{id}
app.MapDelete("/students/{id:int:min(1)}", async (int id) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand(
        "DELETE FROM Students WHERE Id = @Id", connection);

    command.Parameters.AddWithValue("@Id", id);

    int rowsAffected = await command.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
        return Results.NotFound();

    return Results.NoContent();
});
```

---

## Part 10 — Which Execute Method to Use?

| Method | Use when | Returns |
|--------|----------|---------|
| `ExecuteReaderAsync()` | SELECT — multiple rows | `SqlDataReader` to iterate |
| `ExecuteScalarAsync()` | SELECT — single value, or INSERT with OUTPUT | Single `object` value |
| `ExecuteNonQueryAsync()` | INSERT, UPDATE, DELETE | Number of rows affected |

---

## Part 11 — Complete Program.cs

```csharp
using Microsoft.Data.SqlClient;
using StudentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// ── GET /students ──────────────────────────────────────────────────────────
app.MapGet("/students", async () =>
{
    var students = new List<StudentResponseDto>();

    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand("SELECT Id, Name, Major FROM Students", conn);
    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        students.Add(new StudentResponseDto
        {
            Id    = reader.GetInt32(reader.GetOrdinal("Id")),
            Name  = reader.GetString(reader.GetOrdinal("Name")),
            Major = reader.GetString(reader.GetOrdinal("Major"))
        });
    }

    return Results.Ok(students);
}).WithName("GetStudents").WithOpenApi();

// ── GET /students/{id} ─────────────────────────────────────────────────────
app.MapGet("/students/{id:int:min(1)}", async (int id) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        "SELECT Id, Name, Major FROM Students WHERE Id = @Id", conn);
    cmd.Parameters.AddWithValue("@Id", id);

    using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync()) return Results.NotFound();

    return Results.Ok(new StudentResponseDto
    {
        Id    = reader.GetInt32(reader.GetOrdinal("Id")),
        Name  = reader.GetString(reader.GetOrdinal("Name")),
        Major = reader.GetString(reader.GetOrdinal("Major"))
    });
}).WithName("GetStudentById").WithOpenApi();

// ── POST /students ─────────────────────────────────────────────────────────
app.MapPost("/students", async (CreateStudentDto dto) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        @"INSERT INTO Students (Name, Age, Major)
          OUTPUT INSERTED.Id
          VALUES (@Name, @Age, @Major)", conn);

    cmd.Parameters.AddWithValue("@Name",  dto.Name);
    cmd.Parameters.AddWithValue("@Age",   dto.Age);
    cmd.Parameters.AddWithValue("@Major", dto.Major);

    var newId = (int)(await cmd.ExecuteScalarAsync())!;

    return Results.Created($"/students/{newId}", new StudentResponseDto
    {
        Id = newId, Name = dto.Name, Major = dto.Major
    });
}).WithName("CreateStudent").WithOpenApi();

// ── PUT /students/{id} ─────────────────────────────────────────────────────
app.MapPut("/students/{id:int:min(1)}", async (int id, CreateStudentDto dto) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        @"UPDATE Students
          SET Name = @Name, Age = @Age, Major = @Major
          WHERE Id = @Id", conn);

    cmd.Parameters.AddWithValue("@Name",  dto.Name);
    cmd.Parameters.AddWithValue("@Age",   dto.Age);
    cmd.Parameters.AddWithValue("@Major", dto.Major);
    cmd.Parameters.AddWithValue("@Id",    id);

    int rows = await cmd.ExecuteNonQueryAsync();
    return rows == 0 ? Results.NotFound() : Results.NoContent();
}).WithName("UpdateStudent").WithOpenApi();

// ── DELETE /students/{id} ──────────────────────────────────────────────────
app.MapDelete("/students/{id:int:min(1)}", async (int id) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        "DELETE FROM Students WHERE Id = @Id", conn);
    cmd.Parameters.AddWithValue("@Id", id);

    int rows = await cmd.ExecuteNonQueryAsync();
    return rows == 0 ? Results.NotFound() : Results.NoContent();
}).WithName("DeleteStudent").WithOpenApi();

app.Run();
```

---

## Part 12 — Setup Checklist

```
1.  Open SSMS → run CREATE DATABASE and CREATE TABLE scripts
2.  dotnet add package Microsoft.Data.SqlClient
3.  Add connection string to appsettings.json
4.  Add using Microsoft.Data.SqlClient to Program.cs
5.  Read connectionString from builder.Configuration
6.  Replace in-memory list endpoints with ADO.NET calls
7.  dotnet run → test all 5 endpoints in Swagger
```

---

## Part 13 — Summary

| Concept | Key point |
|---------|-----------|
| `SqlConnection` | Always wrap in `using` — closes automatically |
| `SqlCommand` | Holds the SQL — never concatenate user input |
| `SqlParameter` | The only safe way to pass values into SQL |
| `SqlDataReader` | Reads one row at a time with `ReadAsync()` |
| `ExecuteReaderAsync` | Use for SELECT returning multiple rows |
| `ExecuteScalarAsync` | Use for SELECT returning one value, or INSERT with OUTPUT |
| `ExecuteNonQueryAsync` | Use for UPDATE and DELETE — check rows affected for 404 |
| `OUTPUT INSERTED.Id` | Gets the auto-generated primary key back after INSERT |

---

*TECH 4263 — Server Application Technologies*
