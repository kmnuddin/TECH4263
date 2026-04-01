# TECH 4263 — Lab Assignment 6
## Entity Framework Core with EquipmentAPI

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

In this lab you will migrate `EquipmentAPI` from raw ADO.NET to Entity Framework Core. You will update the same three endpoints — `POST /equipments`, `GET /equipments`, and `GET /equipments/{id}` — to use EF Core instead of `SqlConnection` and `SqlCommand`.

Use the `StudentAPI` you completed in class as your reference.

---

## Part 1 — Check Your .NET Version

Before installing any packages, check what version of .NET your `EquipmentAPI` project targets:

```bash
cat EquipmentAPI/EquipmentAPI/EquipmentAPI.csproj
```

Look for the `<TargetFramework>` value — it will be `net8.0` or `net9.0`. You will need this in Part 2.

---

## Part 2 — Install EF Core Packages

Navigate into the `EquipmentAPI/EquipmentAPI/` project folder and install the three packages, pinning the version to match your target framework:

**If your project targets `net8.0`:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
```

**If your project targets `net9.0`:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
```

> The EF Core major version must match your target framework. Running `dotnet add package` without a version grabs the latest, which may be incompatible.

---

## Part 3 — Update the Equipment Entity

Open `Models/Equipment.cs`. The existing model likely has a constructor that manually assigns `Id` using a counter — **remove it**. SQL Server will generate the `Id` automatically via `IDENTITY(1,1)`.

Add the `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` attribute to the `Id` property so EF Core knows not to send an `Id` value on `INSERT`.

---

## Part 4 — Create the DbContext

Create a `Data/` folder inside the project. Add a new file `AppDbContext.cs` that:

- Inherits from `DbContext`
- Accepts `DbContextOptions<AppDbContext>` in the constructor
- Has a `DbSet<Equipment>` property named `Equipments`

---

## Part 5 — Register EF Core in Program.cs

In `Program.cs`, register the `AppDbContext` with the DI container **before** `builder.Build()`. Read the connection string from `appsettings.json`.

Update `appsettings.json` to point to your `EquipmentDb` database — confirm the database name matches what exists in SSMS.

> **Common mistake:** Registering `AddDbContext` after `builder.Build()` will throw `InvalidOperationException: The service collection cannot be modified because it is read-only.`

Remove the `var connectionString = ...` line and the `var equipments = new List<Equipment>()` line — both are no longer needed.

---
## Part 6 — Update the Three Endpoints

Inject `AppDbContext` as a parameter into each endpoint and rewrite the database logic using EF Core. Remove all `SqlConnection`, `SqlCommand`, `SqlDataReader`, and `AddWithValue` code.

- **`POST /equipments`** — use `context.Equipments.Add()` and `await context.SaveChangesAsync()`
- **`GET /equipments`** — use `await context.Equipments.ToListAsync()`
- **`GET /equipments/{id}`** — use `await context.Equipments.FindAsync(id)`, return `404` if null

Keep `.WithName().WithOpenApi()` on all three endpoints.

---

## Part 7 — Test in Swagger

Press **F5** and open Swagger. Test the following in order:

1. `POST /equipments` — create a new item, confirm `201` with a generated `Id`
2. `GET /equipments` — confirm the item appears
3. `GET /equipments/{id}` — confirm all fields are returned
4. Restart the API and call `GET /equipments` again — data must still be there

---

## Grading Rubric

| | Task | Points |
|--|------|--------|
| 1 | EF Core packages installed at correct version for target framework | 2 |
| 2 | `Equipment` entity cleaned up — no manual Id assignment, `[DatabaseGenerated]` added | 3 |
| 3 | `AppDbContext` created with `DbSet<Equipment>` | 3 |
| 4 | `AddDbContext` registered before `builder.Build()`, connection string correct | 2 |
| 5 | All three endpoints use EF Core — no ADO.NET code remaining | 8 |
| 6 | Data persists after API restart | 2 |

---

## Submission Checklist

- [ ] EF Core packages installed at the correct version
- [ ] `Equipment` entity has no constructor with manual `Id` assignment
- [ ] `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` on `Id`
- [ ] `Data/AppDbContext.cs` created with `DbSet<Equipment> Equipments`
- [ ] `AddDbContext` registered before `builder.Build()`
- [ ] `appsettings.json` has correct `Database=EquipmentDb` connection string
- [ ] `POST`, `GET all`, `GET by id` all use `AppDbContext` — no `SqlConnection` remaining
- [ ] Data persists after API restart
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 6*
