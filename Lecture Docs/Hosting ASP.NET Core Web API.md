# TECH 4263 — Lecture: Hosting REST APIs
## Deploying ASP.NET Core Web APIs to a Live Server

---

## Part 1 — Why Host?

Running an API on `localhost` means only your own machine can reach it. The moment you close Visual Studio the API stops. Hosting puts your API on a server that:

- Runs 24/7 without your machine being on
- Has a public URL anyone can reach
- Connects to a hosted database that persists between deployments
- Behaves the same way as a real production environment

Every API you build in this course must eventually be deployed. The semester project requires a live hosted URL — without it the API cannot be tested or marked.

---

## Part 2 — Hosting Options

Not all hosting platforms support ASP.NET Core. Many popular free platforms (Render, Railway, Fly.io) run Linux containers and require Docker configuration. The options below are purpose-built for .NET and require no credit card.

| Platform | Free tier | .NET support | Database | Limit |
|----------|-----------|-------------|----------|-------|
| **MonsterASP.NET** | Permanent free | .NET 8, 9, 10 | MSSQL included | Free subdomain only |
| **SmarterASP.NET** | 60-day trial | .NET 8, 9, 10 | MSSQL included | Expires after 60 days |
| **Somee.com** | Permanent free | .NET Core | MSSQL included | Small ad injected in HTML responses |

**For this course we use MonsterASP.NET.** It is permanently free, requires no credit card, supports ASP.NET Core Web APIs with Swagger, includes MSSQL hosting, and deploys directly from Visual Studio with one click.

---

## Part 3 — How Web Hosting Works

When you deploy to MonsterASP.NET your API runs inside **IIS** (Internet Information Services) — Microsoft's web server. IIS listens for incoming HTTP requests on port 80/443 and forwards them to your ASP.NET Core application.

```
Client (browser / Postman / WinForms app)
        ↓  HTTP request to https://yoursite.runasp.net/equipments
IIS — web server running on MonsterASP.NET's machine
        ↓  forwards to Kestrel
Your ASP.NET Core app (Kestrel)
        ↓  queries
SQL Server database on MonsterASP.NET
        ↓  returns data
Response back to client
```

Your code does not change — the same `Program.cs` that runs locally runs on the server. The only differences are the connection string (pointing at the hosted database) and the environment (Production instead of Development).

---

## Part 4 — The Deployment Process

Deploying to MonsterASP.NET has four main steps:

### Step 1 — Create a website and database in the control panel

Log in to `admin.monsterasp.net` and:

- Click **Create Website** — choose a name, this becomes your subdomain (e.g. `myapi.runasp.net`)
- Click **Databases → Create Database** — note the server address, username, password, and connection string shown

### Step 2 — Prepare the project

Two changes are needed before publishing:

**Enable Swagger in production.** Swagger is wrapped in an `IsDevelopment()` check by default, which means it does not run in Production. Comment it out:

```csharp
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
```

**Update the connection string** in `appsettings.json` to point at the hosted database:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:db12345.databaseasp.net,1433;Database=db12345;User Id=db12345;Password=yourpassword;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

> The `tcp:` prefix and port `,1433` are required for remote SQL Server connections. Spaces around semicolons will cause the connection to fail.

### Step 3 — Run EF Core migrations against the hosted database

With the hosted connection string in `appsettings.json`, run from inside the project folder:

```bash
dotnet ef migrations add InitialCreate   # only if no Migrations folder exists
dotnet ef database update
```

This creates the tables on the hosted database. Verify in SSMS by connecting to the hosted server using SQL Server Authentication with the credentials from the connection string.

### Step 4 — Publish from Visual Studio

- In the MonsterASP.NET control panel, activate **WebDeploy** on your website and download the `.publishSettings` file
- In Visual Studio, right-click the project → **Publish** → **Import Profile** → select the `.publishSettings` file
- Click **Show all settings** → **Settings tab** → paste the hosted connection string into the **Database** and **Entity Framework Migrations** fields
- Click **Publish**

Visual Studio builds, packages, and deploys the API automatically. A successful deploy ends with:

```
Publish Succeeded.
Web App was published successfully http://yoursite.runasp.net/
========== Publish: 1 succeeded, 0 failed, 0 skipped ==========
```

---

## Part 5 — The Connection String Format

Connection strings for remote SQL Server databases follow a specific format. Every field matters.

```
Server=tcp:db12345.databaseasp.net,1433;Database=db12345;User Id=db12345;Password=yourpassword;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True;
```

| Part | Meaning |
|------|---------|
| `Server=tcp:...` | Forces TCP protocol — required for remote connections |
| `,1433` | SQL Server default port |
| `Database=` | The name of the database to connect to — missing this means no tables are found |
| `User Id=` | SQL Server login username |
| `Password=` | SQL Server login password |
| `Encrypt=False` | Disables TLS encryption requirement on the connection |
| `TrustServerCertificate=True` | Skips SSL certificate validation |
| No spaces around `;` | Spaces in connection strings break parsing |

**Common mistakes:**

- Using `Trusted_Connection=True` — this is Windows Authentication and only works locally, never on a remote server
- Forgetting `Database=` — EF Core connects but cannot find any tables
- Spaces around semicolons — `; Database=` instead of `;Database=`
- Wrong `tcp:` prefix or missing port

---

## Part 6 — Development vs Production Environment

ASP.NET Core has an environment concept controlled by the `ASPNETCORE_ENVIRONMENT` variable. This changes how the app behaves.

| Setting | Development (local) | Production (hosted) |
|---------|-------------------|-------------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| Swagger shown | Yes (inside IsDevelopment block) | No — unless block is commented out |
| Detailed error pages | Yes | No |
| Connection string | Local SQL Server | Hosted SQL Server |

This is why Swagger disappears after deployment — the `IsDevelopment()` check evaluates to `false` on the server. Commenting it out makes Swagger available in all environments.

---

## Part 7 — Updating After Code Changes

Once the publish profile is saved, redeploying is one click:

1. Make your code changes in Visual Studio
2. Right-click the project → **Publish**
3. Click **Publish** — the saved profile handles everything

The profile remembers the server address, credentials, and connection string. There is no need to re-import or reconfigure anything.

**If you change the database model** (add a column, add a new entity), run a new migration before publishing:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

Then publish. The migration is applied to the hosted database and the updated code is deployed in one step.

---

## Part 8 — Common Errors and Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| Swagger page does not appear | `IsDevelopment()` block still active | Comment out the `if` block around `UseSwagger()` |
| 500 error on all endpoints after publishing | Wrong connection string in publish settings | Re-open publish settings → Settings tab → re-paste the connection string |
| `dotnet ef` not found | Tool not installed | Run `dotnet tool install --global dotnet-ef --version 8.0.0` |
| Tables not created after `database update` | `Database=` missing from connection string | Add `Database=YourDbName;` to the connection string |
| Cannot connect to hosted DB in SSMS | Still connected to local server | Connect to the hosted server address using SQL Server Authentication |
| `IDENTITY_INSERT is OFF` error | Entity constructor manually assigns `Id` | Remove the constructor — let SQL Server generate `Id` via `IDENTITY(1,1)` |
| Named Pipes error on `database update` | Missing `tcp:` prefix in connection string | Use `Server=tcp:yourserver.databaseasp.net,1433` |

---

## Summary

| Concept | Key point |
|---------|-----------|
| **IIS** | The web server that runs your ASP.NET Core app on MonsterASP.NET |
| **WebDeploy** | The protocol Visual Studio uses to push code to the server |
| **Publish profile** | A `.publishSettings` file containing server credentials — import once, reuse forever |
| **`IsDevelopment()`** | Must be commented out so Swagger appears in Production |
| **`tcp:` prefix** | Required in the connection string for remote SQL Server connections |
| **`Database=`** | Must be present in the connection string — missing it means no tables |
| **EF Core migrations** | Must be run against the hosted DB before or during publish to create tables |
| **Republishing** | Right-click → Publish → Publish — one click after the first deployment |

---

*TECH 4263 — Server Application Technologies*
