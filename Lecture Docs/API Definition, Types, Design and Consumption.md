# TECH 4263 — Lecture: APIs — Definition, Types, Design & Consumption

> **Course:** TECH 4263 — Server Application Technologies  
> **Topic:** What Are APIs, Types of APIs, REST Design Rules, and Consuming APIs

---

## Part 1 — Formal Definition of an API

### What Does API Stand For?

**API** stands for **Application Programming Interface**.

Let's break that down word by word:

| Word | Meaning in context |
|------|-------------------|
| **Application** | Any software — a mobile app, a web server, a database, a script |
| **Programming** | Interaction happens through code, not through a human UI |
| **Interface** | A defined boundary with agreed-upon rules for how two things communicate |

### The Formal Definition

> **An API is a defined contract between two pieces of software that specifies what requests can be made, how they must be structured, and what responses will be returned — without either side needing to know how the other is implemented internally.**

The key word is **contract**. An API is not just a URL or a function — it is a promise about behaviour. As long as both sides honour the contract, they can be built in completely different languages, run on different machines, and change their internals freely.

### A Concrete Analogy

Think of a **restaurant**:

```
You (Client)          Menu (API Contract)         Kitchen (Server)
──────────────────────────────────────────────────────────────────
You don't know    →   You order from       →    The kitchen
how the food          the menu using            prepares and
is made               defined items             returns the dish
```

- The **menu** is the API — it defines what you can request and in what format
- You (the **client**) consume it by placing an order (making a request)
- The **kitchen** (the server) fulfils it and sends back a response
- You never walk into the kitchen — the interface hides the implementation

### Three Parts of Every API Interaction

```
┌──────────────────────────────────────────────────────────────┐
│  1. REQUEST   →   What the client sends to the server        │
│     - Method (what to do)                                    │
│     - Endpoint (where to do it)                              │
│     - Headers (metadata, auth tokens)                        │
│     - Body (data payload, if any)                            │
├──────────────────────────────────────────────────────────────┤
│  2. PROCESSING  →  What happens on the server                │
│     - Authentication / Authorization check                   │
│     - Business logic execution                               │
│     - Database read or write                                 │
├──────────────────────────────────────────────────────────────┤
│  3. RESPONSE  →   What the server sends back                 │
│     - Status code (did it work?)                             │
│     - Headers (content type, caching)                        │
│     - Body (the data or error message)                       │
└──────────────────────────────────────────────────────────────┘
```

### Why APIs Exist — The Core Problems They Solve

| Problem without APIs | How APIs solve it |
|---------------------|-------------------|
| Every app must build everything from scratch | Reuse functionality across apps (e.g., one payment API used by thousands of apps) |
| Tight coupling between systems | Loose coupling — change the backend without touching the client |
| Data locked in silos | Controlled, standardized data sharing |
| Scaling is hard | A single API can serve millions of clients simultaneously |
| Security risks from direct DB access | APIs act as a controlled gateway — clients never touch the database directly |

---

## Part 2 — Types of APIs

APIs are categorized in two ways: **by access scope** (who can use them) and **by architectural style** (how they communicate).

---

### 2A — By Access Scope

#### 1. Open / Public APIs
Available to any developer. Usually require registration for an API key but have no approval gate.

**Real examples:**
- OpenWeatherMap API — fetch weather data for any city
- Google Maps API — embed maps and calculate directions
- NASA APIs — access astronomy images, Mars rover data

#### 2. Partner APIs
Shared between specific organizations under a formal agreement. Not publicly listed.

**Real examples:**
- A retailer sharing inventory data with a logistics partner
- A hospital sharing patient records with an insurance provider under HIPAA rules
- Uber exposing a driver availability API to a travel booking platform

#### 3. Internal / Private APIs
Used only within one organization — between the company's own teams, services, or apps. Never publicly documented.

**Real examples:**
- Netflix's internal API connecting its recommendation engine to the streaming service
- A company's HR system exposing employee data to its internal payroll service
- A bank's fraud detection service communicating with its transaction processing service

#### 4. Composite APIs
Combine multiple API calls into a single request. Common in **microservices** architectures.

**Real example:** A mobile app's "dashboard" loads in one call, but behind the scenes the composite API fetches from the user service, the notifications service, and the activity feed service simultaneously, then merges the results.

---

### 2B — By Architectural Style

This is the more technically important classification for this course.

#### 1. REST (Representational State Transfer) ⭐ — Primary focus of this course

The dominant style for web APIs today. Communicates over HTTP using standard methods, and responses are typically JSON.

**Characteristics:**
- Stateless — each request carries all the information needed; the server stores no session data
- Resource-based — URLs represent *things* (nouns), not *actions* (verbs)
- Uses standard HTTP methods: GET, POST, PUT, PATCH, DELETE
- Responses are usually JSON (sometimes XML)

**Example:**
```
GET  https://api.library.com/books        → return all books
GET  https://api.library.com/books/42     → return book with ID 42
POST https://api.library.com/books        → create a new book
PUT  https://api.library.com/books/42     → replace book 42
DELETE https://api.library.com/books/42   → delete book 42
```

---

#### 2. SOAP (Simple Object Access Protocol)

Older, stricter protocol. Uses XML exclusively and has mandatory envelope structure. Still used heavily in banking, healthcare, and enterprise systems.

**Characteristics:**
- Rigid schema — every message must follow a defined WSDL (Web Services Description Language) contract
- Transport-agnostic — can work over HTTP, SMTP, TCP
- Built-in error handling and security standards (WS-Security)
- Verbose — much more overhead than REST

**Example SOAP request:**
```xml
<Envelope>
  <Body>
    <GetBook>
      <BookId>42</BookId>
    </GetBook>
  </Body>
</Envelope>
```

**When you'll encounter it:** Legacy banking systems, government services, insurance platforms.

---

#### 3. GraphQL

Developed by Facebook (2015). Lets the client specify *exactly* what data it wants in a single request — no over-fetching or under-fetching.

**Characteristics:**
- Single endpoint (usually `/graphql`)
- Client defines the shape of the response
- Strongly typed schema
- Excellent for mobile apps where bandwidth matters

**Example — client asks for only the title and author of a book:**
```graphql
query {
  book(id: 42) {
    title
    author
  }
}
```
**Response:**
```json
{ "data": { "book": { "title": "Clean Code", "author": "Robert Martin" } } }
```
A REST equivalent would return the entire book object including all fields you didn't need.

---

#### 4. gRPC (Google Remote Procedure Call)

High-performance, binary protocol built on HTTP/2. Used for server-to-server communication where speed is critical.

**Characteristics:**
- Uses Protocol Buffers (protobuf) — binary format, much faster than JSON
- Strongly typed contracts (`.proto` files)
- Supports streaming (server → client, client → server, bidirectional)
- Not human-readable (not inspectable in a browser)

**When you'll encounter it:** Microservices, real-time systems, IoT, internal infrastructure at Google, Netflix, Dropbox.

---

#### 5. WebSocket APIs

Unlike the above (which are all request-response), WebSockets maintain a **persistent, two-way connection** between client and server.

**Characteristics:**
- Full-duplex — server can push data to the client at any time without a request
- Low latency — no overhead of opening a new connection for each message
- Ideal for real-time data

**Real examples:** Live chat (Slack, Discord), stock ticker dashboards, live sports scores, collaborative document editing (Google Docs).

---

#### Side-by-Side Comparison

| | REST | SOAP | GraphQL | gRPC | WebSocket |
|---|---|---|---|---|---|
| **Format** | JSON/XML | XML only | JSON | Binary (protobuf) | Any |
| **Transport** | HTTP | HTTP/SMTP/TCP | HTTP | HTTP/2 | TCP |
| **Communication** | Request/Response | Request/Response | Request/Response | Request/Response + Streaming | Full-duplex |
| **Human readable** | ✅ | ✅ | ✅ | ❌ | Depends |
| **Best for** | Web & mobile apps | Enterprise / legacy | Flexible data queries | Internal microservices | Real-time apps |
| **Complexity** | Low | High | Medium | Medium | Medium |

---

## Part 3 — Rules for Designing a REST API

These are the industry-standard conventions you must follow when building a Web API. Violating them makes your API confusing, brittle, and hard to maintain.

---

### Rule 1 — Use Nouns for Endpoints, Not Verbs

The URL identifies a **resource** (a thing). The HTTP method tells you what *action* to take on it.

| ❌ Bad (verb in URL) | ✅ Good (noun in URL) |
|----|-----|
| `GET /getBooks` | `GET /books` |
| `POST /createBook` | `POST /books` |
| `DELETE /deleteBook/5` | `DELETE /books/5` |
| `GET /fetchUserOrders` | `GET /users/3/orders` |

---

### Rule 2 — Use Plural Nouns for Collections

Always use the plural form for resource names — even when fetching a single item.

```
✅  GET /books          →  list of books
✅  GET /books/42       →  single book with ID 42
✅  POST /books         →  create a new book
✅  DELETE /books/42    →  delete book 42

❌  GET /book           ← inconsistent
❌  GET /book/42        ← inconsistent
```

---

### Rule 3 — Use HTTP Methods Correctly

Each HTTP method has one specific meaning. Do not misuse them.

| Method | Purpose | Has Request Body? | Success Status |
|--------|---------|-------------------|---------------|
| `GET` | Retrieve resource(s) | No | `200 OK` |
| `POST` | Create a new resource | Yes | `201 Created` |
| `PUT` | Replace a resource entirely | Yes | `200 OK` or `204 No Content` |
| `PATCH` | Update part of a resource | Yes | `200 OK` or `204 No Content` |
| `DELETE` | Remove a resource | No | `204 No Content` |

**Important:** `GET` and `DELETE` must never modify server state based on query parameters.

---

### Rule 4 — Return the Right HTTP Status Codes

Status codes are not optional decoration — they are the language clients use to handle responses correctly. Always return the semantically correct code.

**2xx — Success**
| Code | Meaning | Use when |
|------|---------|----------|
| `200 OK` | Request succeeded | GET, PUT, PATCH returned data |
| `201 Created` | Resource was created | Successful POST |
| `204 No Content` | Success, no body | DELETE, or PUT/PATCH with no return |

**4xx — Client Error (the client did something wrong)**
| Code | Meaning | Use when |
|------|---------|----------|
| `400 Bad Request` | Malformed or invalid input | Validation failed, missing required fields |
| `401 Unauthorized` | Authentication required | No or invalid auth token |
| `403 Forbidden` | Authenticated but not allowed | User exists but lacks permission |
| `404 Not Found` | Resource doesn't exist | `/books/999` when ID 999 doesn't exist |
| `409 Conflict` | State conflict | Trying to create a duplicate resource |
| `422 Unprocessable Entity` | Semantically invalid input | Valid JSON but business rules violated |

**5xx — Server Error (something broke on your end)**
| Code | Meaning | Use when |
|------|---------|----------|
| `500 Internal Server Error` | Unexpected crash | Unhandled exceptions |
| `503 Service Unavailable` | Server temporarily down | Maintenance, overload |

---

### Rule 5 — Use Hierarchical URLs for Related Resources

When a resource belongs to another resource, express that relationship in the URL path.

```
GET  /users/3/orders          → all orders belonging to user 3
GET  /users/3/orders/7        → order 7 belonging to user 3
POST /users/3/orders          → create a new order for user 3
GET  /books/42/reviews        → all reviews for book 42
POST /books/42/reviews        → add a review to book 42
```

**Keep nesting to 2 levels maximum.** Deep nesting becomes unreadable:
```
❌  /users/3/orders/7/items/2/discounts   ← too deep
✅  /order-items/2/discounts              ← flatten it
```

---

### Rule 6 — Version Your API

Never change an existing API endpoint in a breaking way without versioning it. Clients depend on the contract you published.

```
https://api.library.com/v1/books    ← original contract, never broken
https://api.library.com/v2/books    ← new version with different response shape
```

Versioning can go in the URL (`/v1/`), in a header (`API-Version: 2`), or as a query parameter (`?version=2`). URL versioning is the most visible and widely used.

---

### Rule 7 — Use Query Parameters for Filtering, Sorting, and Pagination

Never create separate endpoints for different views of the same collection. Use query parameters instead.

```
GET /books?author=tolkien                  → filter by author
GET /books?genre=fantasy&year=1954         → filter by multiple fields
GET /books?sort=year&order=desc            → sort results
GET /books?page=2&limit=10                 → paginate results
GET /books?search=ring                     → full-text search
```

---

### Rule 8 — Design Consistent, Predictable Response Bodies

Pick a response format and stick to it across every endpoint. A common pattern:

**Success response:**
```json
{
  "data": {
    "id": 42,
    "title": "The Hobbit",
    "author": "J.R.R. Tolkien",
    "year": 1937
  }
}
```

**Error response:**
```json
{
  "error": {
    "code": 404,
    "message": "Book with ID 42 was not found.",
    "field": null
  }
}
```

**Paged collection response:**
```json
{
  "data": [ ...array of books... ],
  "pagination": {
    "page": 2,
    "limit": 10,
    "total": 84,
    "totalPages": 9
  }
}
```

---

### Rule 9 — Use JSON and Follow Naming Conventions

- Use **JSON** for request and response bodies (the standard for REST)
- Use **camelCase** for JSON property names (`firstName`, not `first_name` or `FirstName`)
- Use **kebab-case** for URL paths (`/book-reviews`, not `/bookReviews` or `/book_reviews`)
- Use **lowercase** for all URL paths

---

### Rule 10 — Secure Your API

| Concern | Implementation |
|---------|---------------|
| **Authentication** | Use JWT (JSON Web Tokens) or API keys in the `Authorization` header |
| **Transport security** | Always use HTTPS — never HTTP in production |
| **Input validation** | Validate and sanitize all incoming data before processing |
| **Rate limiting** | Limit requests per client to prevent abuse (e.g., 100 requests/minute) |
| **CORS** | Configure Cross-Origin Resource Sharing to control which domains can call your API |

---

## Part 4 — Rules for Consuming (Ingesting) an API

Consuming an API means writing the **client-side** code that calls someone else's (or your own) API. There are rules for doing this well.

---

### Rule 1 — Always Read the Documentation First

Before writing a single line of code, read the API's documentation. Look for:

- **Base URL** — the root address all endpoints are relative to
- **Authentication method** — API key? OAuth token? Basic auth?
- **Available endpoints** — what resources exist
- **Request format** — what headers and body structure are expected
- **Response format** — what fields come back and what types they are
- **Rate limits** — how many requests you can make per minute/day
- **Error codes** — what error responses look like and how to handle them

---

### Rule 2 — Always Handle HTTP Status Codes Explicitly

Never assume a request succeeded. Always check the status code before reading the response body.

```csharp
// ❌ Bad — assumes success
var response = await httpClient.GetAsync("/api/books/42");
var book = await response.Content.ReadFromJsonAsync<Book>();

// ✅ Good — checks the status first
var response = await httpClient.GetAsync("/api/books/42");

if (response.IsSuccessStatusCode)
{
    var book = await response.Content.ReadFromJsonAsync<Book>();
    // use book
}
else if (response.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("Book not found.");
}
else
{
    Console.WriteLine($"Error: {response.StatusCode}");
}
```

---

### Rule 3 — Always Handle Errors and Exceptions

Network calls can fail for many reasons: timeout, DNS failure, server crash, bad JSON. Wrap every API call in error handling.

```csharp
try
{
    var response = await httpClient.GetAsync("/api/books");
    response.EnsureSuccessStatusCode();
    var books = await response.Content.ReadFromJsonAsync<List<Book>>();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request timed out.");
}
catch (JsonException)
{
    Console.WriteLine("Response was not valid JSON.");
}
```

---

### Rule 4 — Use the Correct Headers

Most APIs require specific headers. Always set them explicitly.

```
Content-Type: application/json       ← tells the server your body is JSON
Accept: application/json             ← tells the server you want JSON back
Authorization: Bearer <token>        ← sends your auth token
```

In C# with `HttpClient`:
```csharp
httpClient.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "your-token-here");
```

---

### Rule 5 — Never Hard-Code Base URLs or API Keys

Configuration values like base URLs and keys belong in config files or environment variables — never directly in your code.

```csharp
// ❌ Bad — hard-coded
var client = new HttpClient();
client.BaseAddress = new Uri("https://api.library.com/v1/");
var apiKey = "abc123secret";

// ✅ Good — read from configuration
var baseUrl = configuration["ApiSettings:BaseUrl"];
var apiKey  = configuration["ApiSettings:ApiKey"];
var client  = new HttpClient { BaseAddress = new Uri(baseUrl) };
```

In `appsettings.json`:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.library.com/v1/",
    "ApiKey": "your-key-here"
  }
}
```

---

### Rule 6 — Reuse a Single `HttpClient` Instance

Creating a new `HttpClient` for every request is a common mistake — it exhausts socket connections. Use a single shared instance or register it via dependency injection.

```csharp
// ❌ Bad — new instance per request
public async Task<Book> GetBook(int id)
{
    using var client = new HttpClient();   // socket exhaustion risk
    return await client.GetFromJsonAsync<Book>($"/api/books/{id}");
}

// ✅ Good — injected singleton
public class BookService
{
    private readonly HttpClient _client;

    public BookService(HttpClient client)   // injected via DI
    {
        _client = client;
    }

    public async Task<Book> GetBook(int id) =>
        await _client.GetFromJsonAsync<Book>($"/api/books/{id}");
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://api.library.com/v1/");
});
```

---

### Rule 7 — Respect Rate Limits

If an API limits how many requests you can make, implement throttling on your end:
- Space out requests using delays (`await Task.Delay(...)`)
- Cache responses so you don't re-fetch the same data
- Check for `429 Too Many Requests` responses and back off accordingly

```csharp
if ((int)response.StatusCode == 429)
{
    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
    await Task.Delay(retryAfter);
    // retry the request
}
```

---

### Rule 8 — Validate Response Data Before Using It

Don't assume the response contains the fields you expect. Null-check before accessing nested properties.

```csharp
var book = await response.Content.ReadFromJsonAsync<Book>();

// ❌ Bad — will throw NullReferenceException if book is null
Console.WriteLine(book.Title.ToUpper());

// ✅ Good — safe navigation and null checks
if (book is not null)
{
    Console.WriteLine(book.Title?.ToUpper() ?? "No title available");
}
```

---

## Part 5 — Putting It All Together

### The Full Lifecycle of an API Call

```
CLIENT                                         SERVER
──────────────────────────────────────────────────────────────
1. Build request
   - Set method: GET
   - Set URL: /api/books/42
   - Set headers: Accept: application/json
                  Authorization: Bearer <token>

2. Send over HTTPS ─────────────────────────────────────────▶

                                        3. Authenticate request
                                           - Validate token

                                        4. Authorize request
                                           - Does this user have
                                             access to book 42?

                                        5. Execute logic
                                           - Query the database
                                           - Find book where Id = 42

                                        6. Build response
                                           - 200 OK
                                           - Body: { "id": 42, ... }

7. Receive response ◀──────────────────────────────────────────

8. Check status code
   - 200? → deserialize body
   - 404? → show "not found"
   - 401? → redirect to login

9. Use the data
```

---

## Summary — Key Takeaways

| Concept | One-line summary |
|---------|-----------------|
| **API** | A contract between two software systems defining how they communicate |
| **REST** | Stateless, resource-based API style over HTTP — the dominant web standard |
| **SOAP** | XML-based, strict protocol used in enterprise and legacy systems |
| **GraphQL** | Client specifies exact data shape — great for flexible front-ends |
| **gRPC** | Binary, high-speed protocol for internal service-to-service calls |
| **WebSocket** | Persistent two-way connection for real-time apps |
| **Nouns not verbs** | URLs are resources; HTTP methods are the actions |
| **Status codes** | Always return and always check the right code |
| **Versioning** | Never break existing clients — version your API from day one |
| **Error handling** | Every network call can fail — always handle it |
| **Never hard-code secrets** | API keys and URLs belong in config, not in source code |

---

*TECH 4263 — Server Application Technologies | Lecture Notes*
