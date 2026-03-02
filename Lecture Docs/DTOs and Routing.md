# TECH 4263 — Lecture: DTOs and Routing Fundamentals

> **Course:** TECH 4263 — Server Application Technologies  
> **Topic:** Data Transfer Objects and API Routing

---

## Part 1 — The Problem Before DTOs

Before introducing DTOs, let's understand why they exist.

### The Naked Model Problem

Imagine you have a `User` entity that maps directly to your database table:

```csharp
public class User
{
    public int    Id             { get; set; }
    public string FirstName      { get; set; }
    public string LastName       { get; set; }
    public string Email          { get; set; }
    public string PasswordHash   { get; set; }   // sensitive
    public string Role           { get; set; }   // sensitive
    public DateTime CreatedAt    { get; set; }   // internal
    public bool  IsDeleted       { get; set; }   // internal
}
```

If you return this directly from an API endpoint:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    var user = db.Users.Find(id);
    return Results.Ok(user);   // ← exposes EVERYTHING
});
```

The response would include `PasswordHash`, `Role`, `IsDeleted` — fields the client has no business seeing. This is a **data leakage** problem.

There are three additional problems beyond security:

**Over-posting** — a client sending a POST or PUT request can set fields they shouldn't, like `Role` or `IsDeleted`, if you bind directly to the entity.

**Tight coupling** — your API response shape is now locked to your database schema. If you rename a column, your API breaks for every client using it.

**Bloated payloads** — you're sending fields the client never uses, wasting bandwidth on every request.

---

## Part 2 — What is a DTO?

### Definition

> **A DTO (Data Transfer Object) is a simple class whose only job is to carry data between layers — specifically between your API and the outside world. It contains only the fields that need to be transferred, nothing more.**

A DTO has:
- Properties only — no business logic
- No database annotations
- No navigation properties
- Exactly the shape the consumer needs

### The Same Example, Fixed with DTOs

Instead of returning the `User` entity, you define two separate DTOs:

```csharp
// What the API sends OUT to the client (response)
public class UserResponseDto
{
    public int    Id        { get; set; }
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public string Email     { get; set; }
}

// What the API accepts IN from the client (request)
public class CreateUserDto
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public string Email     { get; set; }
    public string Password  { get; set; }   // plain text, hashed in the service
}
```

Now your endpoint looks like:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    var user = db.Users.Find(id);
    var dto  = new UserResponseDto
    {
        Id        = user.Id,
        FirstName = user.FirstName,
        LastName  = user.LastName,
        Email     = user.Email
        // PasswordHash, Role, IsDeleted → never included
    };
    return Results.Ok(dto);
});
```

---

## Part 3 — Types of DTOs

DTOs are typically named by their direction and purpose. There are no strict rules, but these conventions are widely used:

### Request DTOs (inbound — client → server)

Used for POST and PUT request bodies. Contains only what the client is allowed to send.

```csharp
// Used when creating a new book
public class CreateBookDto
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }
}

// Used when updating an existing book
public class UpdateBookDto
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }
}
```

### Response DTOs (outbound — server → client)

Used for GET responses. Contains only what the client should see.

```csharp
// Full detail — used for GET /books/{id}
public class BookDetailDto
{
    public int    Id        { get; set; }
    public string Title     { get; set; }
    public string Author    { get; set; }
    public int    Year      { get; set; }
    public string Genre     { get; set; }
    public double Rating    { get; set; }
}

// Summary — used for GET /books (list view, less data per item)
public class BookSummaryDto
{
    public int    Id     { get; set; }
    public string Title  { get; set; }
    public string Author { get; set; }
}
```

Notice that `BookSummaryDto` is leaner — when returning a list of 100 books, you don't need the full detail of each one.

---

## Part 4 — DTO Mapping

Once you have DTOs, you need to convert between your entity and your DTO. This is called **mapping**.

### Manual Mapping

The simplest approach — write the mapping code yourself:

```csharp
// Entity → Response DTO
public static BookDetailDto ToDetailDto(Book book) => new BookDetailDto
{
    Id     = book.Id,
    Title  = book.Title,
    Author = book.Author,
    Year   = book.Year,
    Genre  = book.Genre,
    Rating = book.Rating
};

// Request DTO → Entity
public static Book ToEntity(CreateBookDto dto) => new Book
{
    Title  = dto.Title,
    Author = dto.Author,
    Year   = dto.Year
};
```

### Where to Put the Mapping

Two common patterns:

**Option A — static method on the DTO itself:**

```csharp
public class BookDetailDto
{
    public int    Id     { get; set; }
    public string Title  { get; set; }
    public string Author { get; set; }

    // Mapping lives on the DTO
    public static BookDetailDto FromEntity(Book book) => new BookDetailDto
    {
        Id     = book.Id,
        Title  = book.Title,
        Author = book.Author
    };
}

// Usage:
var dto = BookDetailDto.FromEntity(book);
```

**Option B — inline in the endpoint:**

```csharp
app.MapGet("/books/{id}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();

    return Results.Ok(new BookDetailDto
    {
        Id     = book.Id,
        Title  = book.Title,
        Author = book.Author
    });
});
```

Both are valid for this course. Option A keeps your endpoints cleaner as the project grows.

---

## Part 5 — DTOs in Practice: A Complete Example

Here is a full in-memory books API using proper request and response DTOs:

```csharp
// ── Models ─────────────────────────────────────────────────────────

// Internal entity — never leaves the server
class Book
{
    public int    Id        { get; set; }
    public string Title     { get; set; } = string.Empty;
    public string Author    { get; set; } = string.Empty;
    public int    Year      { get; set; }
    public bool   IsDeleted { get; set; }  // hidden from client
}

// ── DTOs ───────────────────────────────────────────────────────────

// What the client sends to create a book
class CreateBookDto
{
    public string Title  { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int    Year   { get; set; }
}

// What the client receives back
class BookResponseDto
{
    public int    Id     { get; set; }
    public string Title  { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int    Year   { get; set; }
    // IsDeleted is intentionally excluded
}

// ── Endpoints ──────────────────────────────────────────────────────

var books  = new List<Book>();
int nextId = 1;

// GET all — returns summary DTOs, excludes soft-deleted
app.MapGet("/books", () =>
{
    var result = books
        .Where(b => !b.IsDeleted)
        .Select(b => new BookResponseDto
        {
            Id     = b.Id,
            Title  = b.Title,
            Author = b.Author,
            Year   = b.Year
        });
    return Results.Ok(result);
});

// POST — accepts CreateBookDto, client cannot set Id or IsDeleted
app.MapPost("/books", (CreateBookDto dto) =>
{
    var book = new Book
    {
        Id     = nextId++,
        Title  = dto.Title,
        Author = dto.Author,
        Year   = dto.Year
    };
    books.Add(book);

    var response = new BookResponseDto
    {
        Id     = book.Id,
        Title  = book.Title,
        Author = book.Author,
        Year   = book.Year
    };
    return Results.Created($"/books/{book.Id}", response);
});
```

---

## Part 6 — Routing Fundamentals

Routing is the mechanism that decides **which endpoint handles a given request**. It matches the incoming URL and HTTP method to a specific handler.

---

### 6A — How Routing Works

Every request has two identifying pieces:

```
POST  https://api.library.com/books/42/reviews
───┬─  ────────────────────────────────────────
   │         ┌──────────────────────────────────── path
HTTP Method  └─  /books/42/reviews
```

The router takes the method + path combination and finds the matching endpoint. If no match is found, it returns `404`. If the path matches but the method does not, it returns `405 Method Not Allowed`.

---

### 6B — Route Templates

A route template is the pattern used to define an endpoint URL. It can contain:

**Literal segments** — fixed text that must match exactly:
```
/books              → matches only /books
/api/books/featured → matches only /api/books/featured
```

**Route parameters** — variable segments wrapped in `{ }`:
```
/books/{id}         → matches /books/1, /books/42, /books/999
/users/{userId}/orders/{orderId}  → matches /users/3/orders/7
```

**Optional parameters** — add `?` after the name:
```
/books/{id?}        → matches /books AND /books/42
```

**Default values** — provide a fallback:
```
/books/{id=1}       → if no id given, id = 1
```

**Constraints** — restrict what values a parameter accepts:
```
/books/{id:int}          → id must be an integer
/books/{title:alpha}     → title must be letters only
/books/{id:int:min(1)}   → id must be an integer >= 1
```

---

### 6C — Route Constraints Reference

Constraints are added after the parameter name with a colon:

| Constraint | Example | Matches |
|-----------|---------|---------|
| `int` | `{id:int}` | Any integer: `42`, `-1` |
| `long` | `{id:long}` | Large integer |
| `double` | `{price:double}` | Decimal number: `9.99` |
| `bool` | `{active:bool}` | `true` or `false` |
| `alpha` | `{name:alpha}` | Letters only: `john` |
| `minlength(n)` | `{code:minlength(3)}` | String at least 3 chars |
| `maxlength(n)` | `{code:maxlength(10)}` | String at most 10 chars |
| `min(n)` | `{id:int:min(1)}` | Integer >= 1 |
| `max(n)` | `{page:int:max(100)}` | Integer <= 100 |
| `range(min,max)` | `{id:range(1,999)}` | Integer between 1 and 999 |
| `guid` | `{id:guid}` | A GUID value |

**Example combining multiple constraints:**
```csharp
app.MapGet("/books/{id:int:min(1)}", (int id) =>
{
    // id is guaranteed to be a positive integer
    // /books/0  → 404
    // /books/-5 → 404
    // /books/42 → matches
});
```

---

### 6D — Query Parameters vs Route Parameters

Route parameters and query parameters serve different purposes:

| | Route Parameter | Query Parameter |
|---|---|---|
| **Location** | Inside the URL path | After `?` in the URL |
| **Example** | `/books/42` | `/books?genre=fiction` |
| **Used for** | Identifying a specific resource | Filtering, sorting, pagination |
| **Required?** | Usually yes | Usually optional |
| **C# binding** | Method parameter matching name | Method parameter matching name |

```csharp
// Route parameter — identifies which book
app.MapGet("/books/{id:int}", (int id) =>
{
    // called with: GET /books/42
    // id = 42
});

// Query parameter — filters the list
app.MapGet("/books", (string? genre, int page = 1, int limit = 10) =>
{
    // called with: GET /books?genre=fiction&page=2
    // genre = "fiction", page = 2, limit = 10 (default)
});
```

ASP.NET Core automatically binds query parameters to method parameters by matching names.

---

### 6E — Route Groups

When you have many endpoints sharing the same URL prefix, route groups keep your code clean and avoid repetition:

**Without groups (repetitive):**
```csharp
app.MapGet("/api/books",         () => { });
app.MapGet("/api/books/{id}",    (int id) => { });
app.MapPost("/api/books",        () => { });
app.MapPut("/api/books/{id}",    (int id) => { });
app.MapDelete("/api/books/{id}", (int id) => { });
```

**With a route group:**
```csharp
var books = app.MapGroup("/api/books");

books.MapGet("/",      () => { });
books.MapGet("/{id}",  (int id) => { });
books.MapPost("/",     () => { });
books.MapPut("/{id}",  (int id) => { });
books.MapDelete("/{id}", (int id) => { });
```

Both produce identical routes — the group just avoids typing `/api/books` on every line. If you ever change the prefix, you change it in one place.

You can nest groups for versioned APIs:

```csharp
var v1 = app.MapGroup("/api/v1");
var v1Books = v1.MapGroup("/books");

v1Books.MapGet("/", () => { });      // GET /api/v1/books
v1Books.MapGet("/{id}", (int id) => { }); // GET /api/v1/books/{id}
```

---

### 6F — Combining DTOs and Routing

Here is how DTOs and routing work together in a complete, realistic example:

```csharp
var books  = new List<Book>();
int nextId = 1;

var api = app.MapGroup("/api/books");

// GET /api/books
// Optional query params: genre, page
api.MapGet("/", (string? genre, int page = 1) =>
{
    var query = books.Where(b => !b.IsDeleted);

    if (genre is not null)
        query = query.Where(b => b.Genre == genre);

    var result = query
        .Skip((page - 1) * 10)
        .Take(10)
        .Select(b => new BookSummaryDto { Id = b.Id, Title = b.Title });

    return Results.Ok(result);
});

// GET /api/books/{id:int:min(1)}
api.MapGet("/{id:int:min(1)}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id && !b.IsDeleted);
    if (book is null) return Results.NotFound();

    return Results.Ok(new BookDetailDto
    {
        Id     = book.Id,
        Title  = book.Title,
        Author = book.Author,
        Year   = book.Year
    });
});

// POST /api/books
api.MapPost("/", (CreateBookDto dto) =>
{
    var book = new Book
    {
        Id     = nextId++,
        Title  = dto.Title,
        Author = dto.Author,
        Year   = dto.Year
    };
    books.Add(book);

    return Results.Created($"/api/books/{book.Id}",
        new BookDetailDto { Id = book.Id, Title = book.Title, Author = book.Author, Year = book.Year });
});

// DELETE /api/books/{id:int:min(1)}
api.MapDelete("/{id:int:min(1)}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();

    book.IsDeleted = true;   // soft delete — never exposed via DTO
    return Results.NoContent();
});
```

---

## Part 7 — Summary

### DTOs in one sentence
A DTO is a purpose-built class that carries exactly the data needed for a specific API operation — no more, no less.

### Routing in one sentence
Routing maps an incoming HTTP method + URL to the correct endpoint handler using templates, parameters, and constraints.

### Key Rules to Remember

**DTOs:**
- Never return your database entity directly from an endpoint
- Use separate DTOs for requests and responses
- Clients should not be able to set fields like `Id`, `CreatedAt`, `IsDeleted`, or `Role` through a request DTO
- A list endpoint and a detail endpoint can and should return different DTOs

**Routing:**
- Use `{paramName}` for required route parameters
- Use `{paramName?}` or `{paramName=default}` for optional ones
- Use constraints (`:int`, `:min(1)`, etc.) to reject invalid values before they reach your code
- Use query parameters for filtering, sorting, and pagination — not route parameters
- Use `MapGroup()` to avoid repeating URL prefixes across related endpoints

---

### Side-by-Side: Entity vs DTO vs Route

```
Database Entity          DTO (Response)           Route
─────────────────        ──────────────────       ──────────────────────
User                     UserResponseDto          GET /api/users/{id:int}
  Id                       Id
  FirstName                FirstName              Route param: id
  LastName                 LastName               Constraint: must be int
  Email                    Email
  PasswordHash          ← excluded
  Role                  ← excluded
  IsDeleted             ← excluded
  CreatedAt             ← excluded
```

---

*TECH 4263 — Server Application Technologies | Lecture Notes*
