# TECH 4263 — Lecture: Async and Await in C#

---

## Part 1 — The Problem: Blocking

Imagine a restaurant where there is only one waiter. A customer orders food. The waiter walks to the kitchen, stands there staring at the stove, and waits until the food is ready before going back to take another order. Every other customer has to wait — even if they just want water.

That is exactly what happens in a **synchronous** application when it makes a slow operation like a database query or an HTTP request. The thread that handles the request just sits there waiting — doing nothing — while the operation completes. No other requests can be processed on that thread.

```
Synchronous (blocking):

Thread 1:  [Handle Request A]──[WAITING FOR DB]──────────────[Send Response A]
Thread 2:  (idle, waiting its turn)
Thread 3:  (idle, waiting its turn)

Time lost: Thread 1 is blocked for the entire duration of the DB call
```

In a web API that handles hundreds of requests per second, blocking threads is a serious problem. Threads are expensive resources — there is a finite number of them.

---

## Part 2 — The Solution: Async

An **asynchronous** operation lets the thread go do other work while waiting for a slow operation to complete. When the operation finishes, the thread picks up where it left off.

Back to the restaurant analogy — a good waiter takes the order, passes it to the kitchen, then goes and serves other customers while the food is being prepared. When the food is ready, they pick it up and deliver it.

```
Asynchronous (non-blocking):

Thread 1:  [Handle Request A]──[start DB call].. free ...........[Send Response A]
                                                ↓ meanwhile ↓
Thread 1:  ..............................[Handle Request B]──[Send Response B]

Result: Same thread handles multiple requests without blocking
```

This is why every database call and every HTTP call you have seen in this course uses `async` and `await` — those operations involve waiting for I/O, and we never want to block a thread while waiting.

---

## Part 3 — Task: The Currency of Async

Before understanding `async` and `await`, you need to understand `Task`.

A `Task` represents an **operation that will complete in the future**. It is a promise that says "I am working on this — here is a handle you can use to check on it or get the result when I am done."

```csharp
// A Task with no return value — like void but async
Task DoSomethingAsync()

// A Task that will eventually produce an int
Task<int> GetCountAsync()

// A Task that will eventually produce a Student object
Task<Student> GetStudentAsync(int id)
```

You can think of `Task<T>` like an order ticket at a restaurant. You hand the ticket to the kitchen (start the async operation). The kitchen works on it in the background. When the food is ready, you exchange the ticket for the actual food (the result).

---

## Part 4 — async and await Keywords

### async

Adding `async` to a method signature tells the compiler:
- This method contains asynchronous operations
- It will return a `Task` or `Task<T>` instead of the result directly

```csharp
// Synchronous
string ReadFile(string path)
{
    return File.ReadAllText(path);
}

// Asynchronous
async Task<string> ReadFileAsync(string path)
{
    return await File.ReadAllTextAsync(path);
}
```

### await

`await` is placed before an async operation. It means:
- Start this operation
- Release the current thread so it can do other work
- When the operation finishes, resume here with the result

```csharp
async Task<string> GetDataAsync()
{
    // Thread is released here while the file is being read
    string content = await File.ReadAllTextAsync("data.txt");

    // Thread resumes here with the content once the file is read
    return content;
}
```

Without `await`, the method would start the operation but not wait for it — the result would be a `Task`, not the actual value.

---

## Part 5 — Sync vs Async: Side by Side

```csharp
// ── SYNCHRONOUS ───────────────────────────────────────────────────────────
string GetWebPage(string url)
{
    using var client = new HttpClient();
    string content = client.GetStringAsync(url).Result;  // .Result blocks the thread
    return content;
}

// ── ASYNCHRONOUS ──────────────────────────────────────────────────────────
async Task<string> GetWebPageAsync(string url)
{
    using var client = new HttpClient();
    string content = await client.GetStringAsync(url);   // thread released while waiting
    return content;
}
```

The `.Result` property on the left forces synchronous waiting — it blocks the thread. The `await` on the right releases the thread and resumes when done.

---

## Part 6 — async Propagates Up the Call Chain

`async` and `await` need to be used consistently. Once a method is `async`, everything that calls it should also be `async`. This is called the **async all the way up** rule.

```csharp
// Level 3 — closest to the I/O operation
async Task<Student?> GetStudentFromDbAsync(int id)
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    // ... read from database
    return student;
}

// Level 2 — calls the database method
async Task<StudentResponseDto?> FindStudentAsync(int id)
{
    var student = await GetStudentFromDbAsync(id);  // awaits Level 3
    if (student is null) return null;
    return new StudentResponseDto { Id = student.Id, Name = student.Name };
}

// Level 1 — the API endpoint
app.MapGet("/students/{id}", async (int id) =>      // async endpoint
{
    var dto = await FindStudentAsync(id);            // awaits Level 2
    return dto is null ? Results.NotFound() : Results.Ok(dto);
});
```

If any level in the chain is not `async`, it blocks the thread at that point — defeating the purpose.

---

## Part 7 — Common Async Methods You Will Use

In this course you will mostly use async methods provided by the framework. You do not need to write your own `Task` machinery — just use `await` when calling them.

```csharp
// SQL Server — ADO.NET
await connection.OpenAsync();
await command.ExecuteReaderAsync();
await command.ExecuteScalarAsync();
await command.ExecuteNonQueryAsync();
await reader.ReadAsync();

// HTTP Client
await client.GetAsync("/students");
await client.PostAsJsonAsync("/students", dto);
await response.Content.ReadAsStringAsync();

// File I/O
await File.ReadAllTextAsync("data.txt");
await File.WriteAllTextAsync("data.txt", content);
```

Notice the pattern — async methods always end in `Async` and return a `Task` or `Task<T>`. When you see that suffix, you know you should `await` them.

---

## Part 8 — What Happens Without await

Forgetting `await` is a common mistake. The code compiles but behaves incorrectly.

```csharp
// ❌ Missing await — returns Task<string>, not the actual string
async Task<string> BrokenAsync()
{
    Task<string> task = File.ReadAllTextAsync("data.txt");  // operation started
    return task.ToString();   // returns "System.Threading.Tasks.Task`1[System.String]"
                              // not the file content!
}

// ✅ Correct — awaits the Task and returns the actual string
async Task<string> CorrectAsync()
{
    string content = await File.ReadAllTextAsync("data.txt");
    return content;
}
```

Visual Studio will show a warning if you call an async method without awaiting it — always pay attention to those warnings.

---

## Part 9 — Returning from async Methods

The return type rules for async methods:

| You want to return | Method signature | Return statement |
|--------------------|-----------------|-----------------|
| Nothing | `async Task MethodAsync()` | `return;` or nothing |
| A value | `async Task<int> MethodAsync()` | `return 42;` |
| In a Minimal API endpoint | `async Task<IResult> EndpointAsync()` | `return Results.Ok(...)` |

```csharp
// Returns nothing
async Task LogAsync(string message)
{
    await File.AppendAllTextAsync("log.txt", message);
    // no return needed
}

// Returns an int
async Task<int> CountStudentsAsync()
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("SELECT COUNT(*) FROM Students", conn);
    return (int)(await cmd.ExecuteScalarAsync())!;
}

// Returns IResult (Minimal API endpoint)
async Task<IResult> GetStudentAsync(int id)
{
    // ... query database
    return Results.Ok(student);
}
```

---

## Part 10 — async in Minimal API Endpoints

In the StudentAPI you have been writing `async` lambdas directly in the endpoint definitions. This is the same pattern — the lambda is an `async` method.

```csharp
// Synchronous endpoint — fine for simple responses
app.MapGet("/ping", () => Results.Ok("pong"));

// Asynchronous endpoint — required when doing I/O
app.MapGet("/students", async () =>
{
    // await used here because opening a connection and reading
    // from a database are I/O operations
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand("SELECT Id, Name, Major FROM Students", conn);
    using var reader = await cmd.ExecuteReaderAsync();

    var students = new List<StudentResponseDto>();
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

The rule is simple: if an endpoint touches a database, a file, or another API — make it `async` and `await` every I/O call inside it.

---

## Part 11 — Exception Handling in Async Methods

Exceptions in async methods work exactly like synchronous exceptions. Use `try/catch` normally:

```csharp
async Task<IResult> GetStudentAsync(int id)
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT Id, Name, Major FROM Students WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return Results.NotFound();

        return Results.Ok(new StudentResponseDto
        {
            Id    = reader.GetInt32(reader.GetOrdinal("Id")),
            Name  = reader.GetString(reader.GetOrdinal("Name")),
            Major = reader.GetString(reader.GetOrdinal("Major"))
        });
    }
    catch (SqlException ex)
    {
        // Database-specific errors (connection failure, query error)
        Console.WriteLine($"Database error: {ex.Message}");
        return Results.Problem("A database error occurred.");
    }
}
```

One important difference from synchronous code: if you forget to `await` a Task and an exception is thrown inside it, the exception is lost silently. This is another reason why always awaiting async calls matters.

---

## Part 12 — Quick Reference

```csharp
// Mark a method as async
async Task MethodAsync() { }
async Task<T> MethodAsync() { }

// Await an async operation
var result = await SomeOperationAsync();

// Await with no return value
await SomeVoidOperationAsync();

// Async lambda (used in Minimal API endpoints)
app.MapGet("/route", async () =>
{
    var result = await SomeOperationAsync();
    return Results.Ok(result);
});

// Do NOT use .Result or .Wait() — they block the thread
var bad = SomeOperationAsync().Result;  // ❌ blocks
SomeOperationAsync().Wait();            // ❌ blocks
```

---

## Summary

| Concept | Key point |
|---------|-----------|
| **Synchronous** | Thread blocks and waits — nothing else can run on it |
| **Asynchronous** | Thread is released while waiting — can handle other work |
| **Task** | Represents a future result — the promise of a value |
| **async** | Marks a method as asynchronous — must return `Task` or `Task<T>` |
| **await** | Releases the thread and resumes when the operation completes |
| **Async all the way up** | Once one method is async, callers should be async too |
| **Async suffix** | Methods ending in `Async` return a `Task` — always await them |
| **.Result / .Wait()** | Never use these — they block the thread and defeat async |

---

*TECH 4263 — Server Application Technologies*
