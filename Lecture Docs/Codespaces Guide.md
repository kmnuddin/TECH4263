# TECH 4263 — Running & Contributing via GitHub Codespaces

> **Course:** TECH 4263 — Server Application Technologies  
> **Repo:** [github.com/kmnuddin/TECH4263](https://github.com/kmnuddin/TECH4263)  
> **Stack:** ASP.NET Core Web API · C# · GitHub Codespaces (browser-based VS Code)

---

## What is GitHub Codespaces?

GitHub Codespaces gives you a **full development environment that runs in your browser** — no installs needed. It spins up a cloud-hosted Linux machine pre-loaded with VS Code, .NET SDK, Git, and everything else required to build and run the projects in this course. You can use it from any computer, including a Chromebook or tablet.

> **Free tier:** GitHub gives every user **120 core-hours/month** free (more than enough for coursework). You don't need a paid plan.

---

## Table of Contents

1. [Fork the Course Repo](#1-fork-the-course-repo)
2. [Launch a Codespace](#2-launch-a-codespace)
3. [Get Familiar with the Codespace Environment](#3-get-familiar-with-the-codespace-environment)
4. [Run an Existing Project](#4-run-an-existing-project)
5. [Add a New API Project to the Repo](#5-add-a-new-api-project-to-the-repo)
6. [Commit and Push Your Changes](#6-commit-and-push-your-changes)
7. [Resume or Delete a Codespace](#7-resume-or-delete-a-codespace)
8. [Keep Your Fork in Sync with the Instructor's Repo](#8-keep-your-fork-in-sync-with-the-instructors-repo)
9. [Troubleshooting](#9-troubleshooting)
10. [Quick-Start Cheat Sheet](#10-quick-start-cheat-sheet)

---

## 1. Fork the Course Repo

You need your own copy of the repo before launching a Codespace.

1. Go to [https://github.com/kmnuddin/TECH4263](https://github.com/kmnuddin/TECH4263)
2. Click the **Fork** button (top-right corner)
3. Under **Owner**, select your personal GitHub username
4. Click **Create fork**

You are now on your own copy:
```
https://github.com/<your-username>/TECH4263
```

All your work will live here — the original course repo is never affected.

---

## 2. Launch a Codespace

1. On **your fork** page (`github.com/<your-username>/TECH4263`), click the green **Code** button
2. Select the **Codespaces** tab
3. Click **"Create codespace on main"**

![Codespace launch flow: Code → Codespaces tab → Create codespace on main]

GitHub will now:
- Provision a cloud VM (usually takes 30–60 seconds the first time)
- Install .NET SDK, C# extensions, and Git automatically
- Open a **VS Code editor in your browser** connected to your repo

> **Tip:** Next time you return, click **Code → Codespaces** and your existing codespace will appear — click it to **resume** rather than creating a new one.

---

## 3. Get Familiar with the Codespace Environment

Once loaded, you'll see a VS Code interface with these key areas:

```
┌─────────────────────────────────────────────────────────────┐
│  EXPLORER (left sidebar)     │  EDITOR (center)             │
│  └─ TECH4263/                │  Open files appear here      │
│      ├─ EquipmentAPI/        │                              │
│      ├─ StudentAPI/          │                              │
│      └─ README.md            │                              │
├──────────────────────────────┴──────────────────────────────┤
│  TERMINAL (bottom panel)  — bash shell on the cloud VM      │
└─────────────────────────────────────────────────────────────┘
```

**Open the terminal** at any time with:
- Keyboard shortcut: `` Ctrl + ` `` (backtick)
- Menu: **Terminal → New Terminal**

Verify .NET is ready by running in the terminal:
```bash
dotnet --version
# Expected output: 8.0.x (or higher)
```

---

## 4. Run an Existing Project

### Step 1 — Navigate to the Project Folder

In the terminal, change into one of the demo projects:
```bash
cd StudentAPI
```
*(Or `cd EquipmentAPI` — each folder is a self-contained project.)*

### Step 2 — Restore Dependencies

```bash
dotnet restore
```
This downloads any NuGet packages the project needs. You'll only need to do this once per project.

### Step 3 — Build the Project

```bash
dotnet build
```
Check for any errors in the output. A successful build ends with:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 4 — Run the Project

```bash
dotnet run
```

You'll see output like:
```
info: Now listening on: http://localhost:5000
info: Now listening on: https://localhost:5001
```

### Step 5 — Open the Running App

Codespaces automatically detects open ports. A notification will pop up in the bottom-right corner:

> **"Your application running on port 5000 is available."** → Click **Open in Browser**

Or open it manually:
1. Click the **Ports** tab in the bottom panel (next to Terminal)
2. Find port **5000** or **5001** in the list
3. Click the 🌐 globe icon to open the URL in your browser

### Step 6 — View the Swagger UI

Append `/swagger` to the browser URL to explore and test all API endpoints interactively:
```
https://<your-codespace-url>/swagger
```

### Step 7 — Stop the App

Press **Ctrl + C** in the terminal to stop the running server.

---

## 5. Add a New API Project to the Repo

When an assignment asks you to build a new Web API, follow these steps to create it inside your forked repo.

### Step 1 — Go Back to the Repo Root

```bash
cd /workspaces/TECH4263
```
Always create new projects from the repo root so they sit alongside the existing demos.

### Step 2 — Scaffold a New ASP.NET Core Web API Project (Minimal API)

Use the `--no-openapi` flag to get a clean, minimal starting point without the default controller scaffolding:

```bash
dotnet new web -n LibraryAPI
```

> **`dotnet new web`** gives you a bare-minimum ASP.NET Core app — just `Program.cs` and the project file. This is the correct template for **Minimal APIs**, where all your routes live directly in `Program.cs` with no controller classes needed.

Your repo will now look like:
```
TECH4263/
├── EquipmentAPI/
├── StudentAPI/
├── LibraryAPI/          ← your new project
│   ├── Properties/
│   ├── appsettings.json
│   ├── LibraryAPI.csproj
│   └── Program.cs       ← all routes go here
└── README.md
```

### Step 3 — Navigate into the Project

```bash
cd LibraryAPI
```

Key files to know:

| File | What to do with it |
|------|-------------------|
| `Program.cs` | **Everything goes here** — model, data store, and all route handlers |
| `appsettings.json` | Add connection strings or config values |
| `*.csproj` | Add NuGet package references (or via `dotnet add package`) |

### Step 4 — Define the Model and Routes in `Program.cs`

With Minimal APIs, you define your data model and wire up all endpoints directly in `Program.cs` — no separate controller files or class attributes needed.

Open `Program.cs` and replace its contents with:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// ── In-memory data store ──────────────────────────────────────────
var books = new List<Book>();
int nextId = 1;

// ── Model ─────────────────────────────────────────────────────────
// Defined as a record at the bottom of this file (see below)

// ── Routes ────────────────────────────────────────────────────────

// GET all books
app.MapGet("/api/books", () => Results.Ok(books));

// GET a single book by ID
app.MapGet("/api/books/{id}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

// POST — create a new book
app.MapPost("/api/books", (Book book) =>
{
    book.Id = nextId++;
    books.Add(book);
    return Results.Created($"/api/books/{book.Id}", book);
});

// PUT — update an existing book
app.MapPut("/api/books/{id}", (int id, Book updated) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();
    book.Title  = updated.Title;
    book.Author = updated.Author;
    book.Year   = updated.Year;
    return Results.NoContent();
});

// DELETE — remove a book
app.MapDelete("/api/books/{id}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();
    books.Remove(book);
    return Results.NoContent();
});

app.Run();

// ── Model definition ──────────────────────────────────────────────
class Book
{
    public int    Id     { get; set; }
    public string Title  { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int    Year   { get; set; }
}
```

> **How Minimal APIs differ from Controllers:**
> - No `[ApiController]`, `[Route]`, or `[HttpGet]` attributes
> - Routes are registered with `app.MapGet()`, `app.MapPost()`, etc.
> - The model lives in the same file — no separate `Models/` folder required
> - `Results.Ok()`, `Results.NotFound()`, `Results.Created()` replace `IActionResult` return types

### Step 5 — Add Swagger NuGet Package

The `dotnet new web` template doesn't include Swagger by default. Add it:

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
```

Then restore:
```bash
dotnet restore
```

### Step 6 — Install Additional NuGet Packages (if needed)

For example, to add Entity Framework Core for database work:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### Step 7 — Run and Test Your New API

```bash
dotnet run
```
Open the forwarded port in the browser and go to `/swagger` to explore and test all five endpoints interactively.

---

## 6. Commit and Push Your Changes

After building and testing your project, save your work to GitHub.

### Using the VS Code Source Control Panel (GUI)

1. Click the **Source Control** icon in the left sidebar (the branching icon, or press `Ctrl + Shift + G`)
2. You'll see all new and changed files listed under **"Changes"**
3. Hover over **"Changes"** and click the **+** button to **stage all files**
4. Type a commit message in the text box at the top, e.g.:
   ```
   Add LibraryAPI with Minimal API in-memory CRUD for books
   ```
5. Click the **✓ Commit** button (checkmark)
6. Click **Sync Changes** (or the **↑ Push** button) to send it to your GitHub fork

### Using the Terminal (CLI)

```bash
# Go to the repo root first
cd /workspaces/TECH4263

# Stage everything (new files + changes)
git add .

# Commit with a descriptive message
git commit -m "Add LibraryAPI with Minimal API in-memory CRUD for books"

# Push to your fork
git push origin main
```

### Verify on GitHub

Visit `https://github.com/<your-username>/TECH4263` in a new tab — your new project folder should now appear in the file listing.

---

## 7. Resume or Delete a Codespace

### Resuming a Codespace

Your codespace **auto-suspends** after 30 minutes of inactivity (your code is saved). To resume:

1. Go to [github.com/codespaces](https://github.com/codespaces)
2. Find your `TECH4263` codespace in the list
3. Click **Open** to pick up right where you left off

Or from your fork: **Code → Codespaces** → click the existing codespace name.

### Managing Your Codespaces

Visit [github.com/codespaces](https://github.com/codespaces) to:
- See all your active codespaces
- Check how many free hours you've used this month
- **Stop** a codespace manually to preserve your free hours
- **Delete** a codespace when you're done with it (your pushed code on GitHub is safe)

> **Important:** Always **push your work** before deleting a codespace. Any unpushed commits exist only on the cloud VM and will be lost if deleted.

---

## 8. Keep Your Fork in Sync with the Instructor's Repo

When your instructor adds new demo projects to the original course repo, follow these steps to pull them into your fork.

### One-Time Setup

Run this once to link the original course repo as an "upstream" source:
```bash
git remote add upstream https://github.com/kmnuddin/TECH4263.git
```

Confirm it's set up:
```bash
git remote -v
# Should show:
# origin    https://github.com/<your-username>/TECH4263.git
# upstream  https://github.com/kmnuddin/TECH4263.git
```

### Syncing Updates

Run this whenever the instructor announces new content:
```bash
# Pull the latest from the course repo
git fetch upstream

# Merge it into your local main branch
git merge upstream/main

# Push the updated code to your own GitHub fork
git push origin main
```

You can also sync directly on GitHub **without** using the terminal:
1. Go to your fork on GitHub (`github.com/<your-username>/TECH4263`)
2. Click the **"Sync fork"** button (appears when your fork is behind the original)
3. Click **"Update branch"**
4. Back in Codespaces, run `git pull` in the terminal to pull the updates into your open codespace

---

## 9. Troubleshooting

| Problem | Solution |
|---------|----------|
| `dotnet: command not found` | The Codespace is still initializing — wait 30 seconds and try again |
| Port not forwarded / no popup | Click the **Ports** tab in the bottom panel and manually forward port `5000` |
| `Build failed` errors | Run `dotnet restore` first, then `dotnet build` again |
| Changes not showing in Source Control | Make sure you saved the file (`Ctrl + S`); unsaved files show a dot in their tab |
| Pushed code not appearing on GitHub | Confirm you ran `git push origin main` and check the branch name matches |
| Ran out of free Codespace hours | Delete unused codespaces at [github.com/codespaces](https://github.com/codespaces) to free up quota |
| `HTTPS port not accessible` | Use the `http://` URL instead of `https://` when testing locally in Codespaces |
| `You must install or update .NET to run this application` | See the **.NET Version Mismatch** section below |

---

### ⚠️ Common Issue: .NET Version Mismatch

You may see this error when running `dotnet run` on an existing project:

```
You must install or update .NET to run this application.
Framework: 'Microsoft.NETCore.App', version '8.0.0' (x64)
The following frameworks were found:
  9.0.x at [/usr/share/dotnet/shared/Microsoft.NETCore.App]
  10.0.x at [/usr/share/dotnet/shared/Microsoft.NETCore.App]
```

This means the project was written targeting .NET 8, but your Codespace only has .NET 9 or 10 installed. Pick one of the fixes below.

---

#### Fix 1 — Update the Target Framework in the `.csproj` (Recommended)

This is a one-line change. Open the project's `.csproj` file (e.g., `StudentAPI/StudentAPI.csproj`) and find:

```xml
<TargetFramework>net8.0</TargetFramework>
```

Change it to match the version your Codespace has (check with `dotnet --version`):

```xml
<TargetFramework>net9.0</TargetFramework>
```

Save the file, then:
```bash
dotnet restore
dotnet run
```

> **Why is this safe?** For the basic Web API patterns used in this course, .NET 8 → 9 requires no code changes. Your controllers, models, and routing all work identically.

---

#### Fix 2 — Do It from the Terminal with One Command

If you'd rather not open the file manually, use `sed` to make the replacement instantly:

```bash
sed -i 's/<TargetFramework>net8.0<\/TargetFramework>/<TargetFramework>net9.0<\/TargetFramework>/' StudentAPI.csproj
```

Then:
```bash
dotnet restore
dotnet run
```

---

#### Fix 3 — Install .NET 8 in the Codespace (Only if Required)

If your assignment specifically requires .NET 8, you can install it manually in the Codespace:

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.0
export PATH="$HOME/.dotnet:$PATH"
dotnet --version   # should now show 8.0.x
```

To make the PATH change persist across terminal sessions in this Codespace, add it to your shell profile:
```bash
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

> **Note:** This installed version only exists in your current Codespace. If you create a new Codespace, you'll need to run these steps again.

---

## 10. Quick-Start Cheat Sheet

```
FIRST TIME SETUP
────────────────────────────────────────────────────────
1. Fork     →  github.com/kmnuddin/TECH4263 → Fork
2. Codespace →  Your fork → Code → Codespaces → Create codespace on main

RUN AN EXISTING PROJECT
────────────────────────────────────────────────────────
cd StudentAPI
dotnet restore
dotnet run
→  Open forwarded port in browser → /swagger

ADD A NEW PROJECT
────────────────────────────────────────────────────────
cd /workspaces/TECH4263
dotnet new web -n YourProjectName        ← Minimal API (no controllers)
cd YourProjectName
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
# Define model + MapGet/MapPost/MapPut/MapDelete routes in Program.cs
dotnet run  →  test at /swagger

SAVE & PUSH
────────────────────────────────────────────────────────
git add .
git commit -m "Add YourProjectName with Minimal API [description]"
git push origin main

SYNC INSTRUCTOR UPDATES
────────────────────────────────────────────────────────
git fetch upstream
git merge upstream/main
git push origin main
```

---

*Last updated: February 2026 | TECH 4263 — Server Application Technologies*
