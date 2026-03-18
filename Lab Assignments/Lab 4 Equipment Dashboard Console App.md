# TECH 4263 — Lab Assignment 4
## Equipment Dashboard Console Application

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

In this assignment you will build a console application called `EquipmentDashboard` that consumes the `EquipmentAPI` you refactored in Lab 3. The app runs entirely in Codespaces — no Windows required.

You have already built a similar console app for `StudentDashboard` in class. This assignment follows the same pattern. Your job is to apply what you learned and adapt it for the Equipment domain on your own.

---

## What the App Must Do

When launched, the app displays a menu and waits for user input:

```
==============================
  Equipment Dashboard (Console)
==============================

1. List all equipment
2. View equipment by ID
3. Create new equipment
0. Exit

Select an option: _
```

Each menu option calls the corresponding `EquipmentAPI` endpoint:

| Option | Action | API Call |
|--------|--------|----------|
| 1 | Display all equipment in a formatted table | `GET /equipments` |
| 2 | Prompt for an ID and display that equipment's details | `GET /equipments/{id}` |
| 3 | Prompt for equipment fields and create a new record | `POST /equipments` |
| 0 | Exit the application | — |

---

## Requirements

### Project Setup (2 points)

- Create a new console project named `EquipmentDashboard` inside your TECH4263 repo using `dotnet new console`.
- The project must sit alongside `StudentAPI`, `EquipmentAPI`, and `StudentDashboard` in the repo root.
- The project must build with `dotnet build` without errors.

### DTO Models (4 points)

- Create a `Models/` folder inside the project.
- Add `EquipmentResponseDto.cs` — the shape of what the API sends back. Look at your `EquipmentAPI` response DTO from Lab 3 to decide which fields to include.
- Add `CreateEquipmentDto.cs` — the shape of what you send to the API. Look at your `EquipmentAPI` create DTO from Lab 3.

### Menu and API Calls (10 points)

- **Option 1 — List all equipment (3 pts):** Call `GET /equipments`. Display results in a formatted table with at minimum the ID and one other field visible per row. Handle the case where no equipment exists yet.
- **Option 2 — View by ID (3 pts):** Prompt the user to enter an ID. Call `GET /equipments/{id}`. Display all fields of the returned equipment. Return a clear message if the ID is not found (404).
- **Option 3 — Create equipment (4 pts):** Prompt the user for each required field one at a time. Validate that no field is left empty. Call `POST /equipments` with a `CreateEquipmentDto`. Print a success or failure message based on the response status code.

### Error Handling (4 points)

- All three menu options must wrap their API calls in a `try/catch` for `HttpRequestException` and print a helpful message if the API is not reachable.
- Invalid menu input (anything other than 0, 1, 2, 3) must print an error and re-show the menu — it must not crash the app.
- Invalid ID input (non-numeric or zero/negative) must be rejected before the API is called.

---

## Codespaces Setup Hints

- Start `EquipmentAPI` in Terminal 1 with `dotnet run` before running `EquipmentDashboard`.
- Open the **Ports** tab, right-click the EquipmentAPI port, and set visibility to **Public**.
- Copy the local address from the Ports tab and use it as the `BaseAddress` in your `HttpClient`.
- Use `http://` not `https://` — the HTTPS port will cause certificate errors.
- Comment out `app.UseHttpsRedirection()` in `EquipmentAPI/Program.cs` while running in Codespaces.

---

## Code Hints

**Project file** — your `.csproj` should target plain `net9.0`, not `net9.0-windows`:
```xml
<TargetFramework>net9.0</TargetFramework>
```

**HttpClient** — create one shared instance at the top of `Program.cs`, not one per request:
```csharp
using var client = new HttpClient
{
    BaseAddress = new Uri("http://localhost:XXXX")  // your EquipmentAPI port
};
```

**Deserializing a list** — same pattern as StudentDashboard:
```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var items = JsonSerializer.Deserialize<List<EquipmentResponseDto>>(json, options);
```

**Checking for 404** — check the specific status code before checking `IsSuccessStatusCode`:
```csharp
if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    // item not found — print message, do not try to read the body
    return;
}
```

**Posting a DTO** — `PostAsJsonAsync` handles serialization and sets the content type automatically:
```csharp
var response = await client.PostAsJsonAsync("/equipments", dto);
```

**Console colors** — use `ConsoleColor` to make output easier to read:
```csharp
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("  Success message here.");
Console.ResetColor();
```

---

## Submission Checklist

- [ ] `EquipmentDashboard` project exists in repo alongside other projects
- [ ] `dotnet build` passes with zero errors
- [ ] `Models/EquipmentResponseDto.cs` created with correct fields
- [ ] `Models/CreateEquipmentDto.cs` created with correct fields
- [ ] Option 1 lists all equipment from `GET /equipments`
- [ ] Option 2 fetches and displays one equipment item from `GET /equipments/{id}`
- [ ] Option 2 handles 404 with a clear message
- [ ] Option 3 prompts for all fields, validates input, and calls `POST /equipments`
- [ ] All options handle `HttpRequestException` gracefully
- [ ] Invalid menu input does not crash the app
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 4*
