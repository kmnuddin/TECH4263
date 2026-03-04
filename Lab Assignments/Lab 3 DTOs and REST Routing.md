# TECH 4263 — Lab Assignment 3
## DTOs and REST Routing

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

Both `StudentAPI` and `EquipmentAPI` in your fork have two problems:

1. Routes use verbs instead of nouns
2. No DTOs — the POST endpoint binds directly to raw parameters or the entity

Your job is to fix both of these in both projects.

---

## Task 1 — StudentAPI (10 points)

Open `StudentAPI/StudentAPI/Program.cs`.

### Fix the Routes (5 points)

Rename all three endpoints to follow REST naming conventions:

| Current | Fixed |
|---------|-------|
| `POST /createstudent` | `POST /students` |
| `GET /getstudents` | `GET /students` |
| `GET /getstudent/{id}` | `GET /students/{id:int:min(1)}` |

### Add DTOs (5 points)

In the `Models/` folder, create two DTO files:

**`CreateStudentDto.cs`** — used by the POST endpoint (what the client sends in):
```csharp
namespace StudentAPI.Models
{
    public class CreateStudentDto
    {
        public string Name  { get; set; } = string.Empty;
        public int    Age   { get; set; }
        public string Major { get; set; } = string.Empty;
    }
}
```

**`StudentResponseDto.cs`** — used by the GET endpoints (what the server sends back):
```csharp
namespace StudentAPI.Models
{
    public class StudentResponseDto
    {
        public int    Id    { get; set; }
        public string Name  { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
    }
}
```

Notice `Age` is excluded from the response — it is not the client's concern.

Then update your endpoints to use these DTOs:
- `POST /students` should accept a `CreateStudentDto` in the request body
- `GET /students` should return a list of `StudentResponseDto`
- `GET /students/{id:int:min(1)}` should return a single `StudentResponseDto`

Here is how each endpoint should look after the changes:

**POST — accept `CreateStudentDto` from request body, map to entity:**
```csharp
app.MapPost("/students", (CreateStudentDto dto) =>
{
    var student = new Student(dto.Name, dto.Age, dto.Major);
    students.Add(student);
    return Results.Created($"/students/{student.Id}", new StudentResponseDto
    {
        Id    = student.Id,
        Name  = student.Name,
        Major = student.Major
    });
})
.WithName("CreateStudent")
.WithOpenApi();
```

**GET all — map each entity to a `StudentResponseDto`:**
```csharp
app.MapGet("/students", () =>
{
    var result = students.Select(s => new StudentResponseDto
    {
        Id    = s.Id,
        Name  = s.Name,
        Major = s.Major
    });
    return Results.Ok(result);
})
.WithName("GetStudents")
.WithOpenApi();
```

**GET by id — return a single `StudentResponseDto`:**
```csharp
app.MapGet("/students/{id:int:min(1)}", (int id) =>
{
    var student = students.FirstOrDefault(s => s.Id == id);
    if (student == null) return Results.NotFound();

    return Results.Ok(new StudentResponseDto
    {
        Id    = student.Id,
        Name  = student.Name,
        Major = student.Major
    });
})
.WithName("GetStudentById")
.WithOpenApi();
```

---

## Task 2 — EquipmentAPI (10 points)

Open `EquipmentAPI/EquipmentAPI/Program.cs`.

### Fix the Routes (3 points)

| Current | Fixed |
|---------|-------|
| `POST /createequipment` | `POST /equipments` |
| `GET /getequipments` | `GET /equipments` |
| `GET /getequipment/{id}` | `GET /equipments/{id:int:min(1)}` |

### Add DTOs (5 points)

Look at the existing `Equipment` model in `Models/`. Create two DTO files yourself following the same pattern as Task 1:

- **`CreateEquipmentDto.cs`** — include only the fields a client should be allowed to send. Do not include `Id`.
- **`EquipmentResponseDto.cs`** — include only the fields a client should receive back. You decide which fields to exclude.

Update your endpoints to use these DTOs.

### Clean Up (2 points)

Remove the commented-out stub code at the top of `Program.cs`.

---

## Hints

- **The entity stays untouched.** You are adding new DTO classes alongside the existing model, not replacing it.
- **Mapping is manual.** In each endpoint, create the DTO from the entity (or vice versa) by assigning fields one by one.
- **Test in Swagger** after each change — run `dotnet run` and open `/swagger` to confirm the request and response shapes look correct.

---

## Submission Checklist

- [ ] `StudentAPI` — all 3 routes fixed
- [ ] `StudentAPI` — `CreateStudentDto.cs` and `StudentResponseDto.cs` created and used
- [ ] `EquipmentAPI` — all 3 routes fixed
- [ ] `EquipmentAPI` — `CreateEquipmentDto.cs` and `EquipmentResponseDto.cs` created and used
- [ ] `EquipmentAPI` — commented-out stubs removed
- [ ] Both projects run with zero errors
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 3*