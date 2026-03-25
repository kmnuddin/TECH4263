# TECH 4263 — Lab Assignment 5
## SQL Server Integration with EquipmentAPI

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

In this lab you will replace the in-memory `List<Equipment>` in your `EquipmentAPI` with a real SQL Server database. By the end, all equipment records will persist in the database — they will survive an API restart.

Use the `StudentAPI` in your repo as your reference throughout. The pattern is identical — only the table name, column names, and DTO type are different.

---

## Part 1 — Create the Database and Table in SSMS

Open **SQL Server Management Studio (SSMS)**, connect to your local SQL Server instance, and run the following script:

```sql
CREATE DATABASE EquipmentDb;
GO

USE EquipmentDb;
GO

CREATE TABLE Equipments (
    Id       INT           IDENTITY(1,1) PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Status   NVARCHAR(50)  NOT NULL,
    Location NVARCHAR(100) NOT NULL
);
GO
```

Once the script runs, expand **Databases → EquipmentDb → Tables** in the Object Explorer to confirm the `Equipments` table was created.

> `IDENTITY(1,1)` means SQL Server auto-generates and increments the `Id` column — the same as the `Students` table in the StudentAPI.

---

## Part 2 — Install the NuGet Package

Open a terminal inside the `EquipmentAPI/EquipmentAPI/` project folder and run:

```bash
dotnet add package System.Data.SqlClient
```

> **Note:** The StudentAPI uses `System.Data.SqlClient` — make sure you install the same package, not `Microsoft.Data.SqlClient`.

---

## Part 3 — Add the Connection String

Open `appsettings.json` inside the `EquipmentAPI` project and add a `ConnectionStrings` section at the top level:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EquipmentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Note `Database=EquipmentDb` — this must match the database name you created in Part 1. The StudentAPI used `StudentDb`; this API uses `EquipmentDb`.

---

## Part 4 — Modify Program.cs

Open `EquipmentAPI/EquipmentAPI/Program.cs`. Work through each change below in order.

### 4a — Add the using directive

At the very top of `Program.cs`, add the namespace — the same one used in StudentAPI:

```csharp
using System.Data.SqlClient;
```

### 4b — Read the connection string

After `var app = builder.Build();`, read the connection string from configuration:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

Place this line before the first endpoint, exactly as it appears in the StudentAPI.

### 4c — Remove the in-memory list

Delete the line that declares `var equipments = new List<Equipment>();` — the database is now the source of truth. Also remove the commented-out stubs if any remain from Lab 3.

### 4d — Implement POST /equipments

Rewrite the existing `POST /equipments` endpoint to insert a new row into the database. Open the StudentAPI `Program.cs` and look at the `POST /students` endpoint — your implementation follows the exact same structure:

- Open a `SqlConnection` using `connection` as the variable name
- Create a `SqlCommand` with an `INSERT INTO Equipments (Name, Category, Status, Location)` statement
- Include `OUTPUT INSERTED.Id` between the column list and `VALUES` — this returns the auto-generated primary key
- Add a `SqlParameter` for each field using `command.Parameters.AddWithValue`
- Execute with `ExecuteScalarAsync()` and cast: `var newId = (int)(await command.ExecuteScalarAsync())!;`
- Return `Results.Created($"/equipments/{newId}", new EquipmentResponseDto { ... })`
- Keep `.WithName("CreateEquipment").WithOpenApi()`

### 4e — Implement GET /equipments

Rewrite the existing `GET /equipments` endpoint to read all rows from the database. Look at the `GET /students` endpoint in the StudentAPI:

- Declare an empty `List<EquipmentResponseDto>`
- Open a `SqlConnection` using `connection` as the variable name
- Create a `SqlCommand` — your `SELECT` should retrieve `Id`, `Name`, `Category`, `Status`, and `Location` from `Equipments`
- Execute with `ExecuteReaderAsync()` and loop with `while (await reader.ReadAsync())`
- Read each column using `reader.GetInt32(reader.GetOrdinal("Id"))` and `reader.GetString(reader.GetOrdinal("Name"))` — the same `GetOrdinal` pattern from StudentAPI
- Map each row into an `EquipmentResponseDto` and add it to the list
- Return `Results.Ok(list)`
- Keep `.WithName("GetEquipments").WithOpenApi()`

### 4f — Implement GET /equipments/{id}

Rewrite the existing `GET /equipments/{id}` endpoint to read a single row. Look at the `GET /students/{id:int:min(1)}` endpoint in the StudentAPI:

- Open a `SqlConnection`
- Write a `SELECT` with `WHERE Id = @Id`
- Add the parameter: `command.Parameters.AddWithValue("@Id", id)`
- Execute with `ExecuteReaderAsync()`
- If `!await reader.ReadAsync()` — the row does not exist, return `Results.NotFound()`
- Otherwise read and map the columns into an `EquipmentResponseDto` and return `Results.Ok()`
- Keep `.WithName("GetEquipmentById").WithOpenApi()`

> **Important:** Never write `WHERE Id = {id}` using string interpolation. Always use `@Id` with `AddWithValue` — look at how StudentAPI handles this and follow the same pattern.

---

## Part 5 — Test in Swagger

Press **F5** in Visual Studio to start the API, then open Swagger at `https://localhost:{port}/swagger`. Test the following flow in order:

**Step 1 — Create equipment:**
Call `POST /equipments` with a valid request body. Confirm you receive a `201 Created` response with a generated `Id`.

**Step 2 — List all equipment:**
Call `GET /equipments`. Confirm the item you just created appears in the response.

**Step 3 — Get by ID:**
Call `GET /equipments/{id}` using the `Id` from Step 1. Confirm all five fields (`Id`, `Name`, `Category`, `Status`, `Location`) are returned correctly.

**Step 4 — Confirm persistence:**
Stop the API with `Ctrl + C`, press F5 to restart it, then call `GET /equipments` again. The equipment must still be there — this confirms data is stored in SQL Server, not in memory.

---

## Grading Rubric

| | Task | Points |
|--|------|--------|
| 1 | `EquipmentDb` database and `Equipments` table created correctly in SSMS | 3 |
| 2 | `System.Data.SqlClient` package installed, `using` directive added | 1 |
| 3 | Connection string in `appsettings.json`, read with `GetConnectionString` in `Program.cs` | 2 |
| 4 | `POST /equipments` inserts row using `OUTPUT INSERTED.Id` and `ExecuteScalarAsync`, returns `201` with generated Id | 5 |
| 5 | `GET /equipments` reads all rows using `ExecuteReaderAsync`, `ReadAsync` loop, and `GetOrdinal` pattern | 5 |
| 6 | `GET /equipments/{id}` reads one row with `@Id` parameter, returns `404` if not found | 2 |
| 7 | Data persists after API restart | 2 |

---

## Submission Checklist

- [ ] `EquipmentDb` database and `Equipments` table exist in local SQL Server
- [ ] `System.Data.SqlClient` package installed
- [ ] `using System.Data.SqlClient` at top of `Program.cs`
- [ ] Connection string in `appsettings.json` with `Database=EquipmentDb`
- [ ] `connectionString` read with `builder.Configuration.GetConnectionString("DefaultConnection")`
- [ ] In-memory `List<Equipment>` removed from `Program.cs`
- [ ] `POST /equipments` uses `OUTPUT INSERTED.Id` and `ExecuteScalarAsync`
- [ ] `GET /equipments` uses `ExecuteReaderAsync`, `ReadAsync` loop, and `GetOrdinal` pattern
- [ ] `GET /equipments/{id}` uses `@Id` parameter — no string interpolation
- [ ] All three endpoints keep `.WithName().WithOpenApi()`
- [ ] Data confirmed persistent after API restart
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 5*
