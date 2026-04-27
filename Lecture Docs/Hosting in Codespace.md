# TECH 4263 — Hosting Guide (Codespaces Version)
## Deploying StudentAPI to MonsterASP.NET from Codespaces

---

## Important Differences from Visual Studio

Codespaces users have two extra steps compared to the Visual Studio workflow:

1. **SQLite → SQL Server.** Your local Codespaces setup uses SQLite because SQL Server cannot run on Linux. MonsterASP.NET provides a real SQL Server database, so you need to switch the EF Core provider from SQLite to SQL Server before deploying.
2. **No WebDeploy GUI.** Visual Studio's one-click Publish button is not available. You will publish using the `dotnet publish` command and upload the output manually through the MonsterASP.NET file manager.

Everything else — the connection string, migrations, and Swagger fix — is identical to the Visual Studio guide.

---

## Part 1 — MonsterASP.NET Control Panel

### Step 1 — Create a website

1. Go to `admin.monsterasp.net` and log in
2. Click **Create Website**
3. Enter a name — this becomes your subdomain (e.g. `tech4263-studentapi.runasp.net`)
4. Click **Create**

### Step 2 — Create an MSSQL database

1. In the left menu click **Databases → Create Database**
2. Enter a database name (e.g. `StudentDb`), username, and password
3. Click **Create**
4. Click on the database entry and copy the full connection string — you will need it shortly. It looks like this:

```
Server=tcp:db12345.databaseasp.net,1433;Database=db12345;User Id=db12345;Password=yourpassword;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True;
```

---

## Part 2 — Switch from SQLite to SQL Server

Your project currently uses `Microsoft.EntityFrameworkCore.Sqlite`. MonsterASP.NET runs SQL Server, so you need to swap the provider.

### Step 1 — Install the SQL Server package

Open a terminal in your Codespace, navigate to the project folder, and run:

```bash
cd StudentAPI/StudentAPI
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
```

Use `--version 9.0.0` if your project targets `net9.0`.

### Step 2 — Update Program.cs

Find the line that registers SQLite:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Replace `UseSqlite` with `UseSqlServer`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Step 3 — Update appsettings.json

Replace the SQLite connection string with the MonsterASP.NET one you copied in Step 2 of Part 1. Make sure there are no spaces around the semicolons:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:db12345.databaseasp.net,1433;Database=db12345;User Id=db12345;Password=yourpassword;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

---

## Part 3 — Enable Swagger in Production

Swagger is hidden in Production by default. Comment out the `IsDevelopment()` check in `Program.cs`:

```csharp
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
```

---

## Part 4 — Run Migrations Against the Hosted Database

Make sure the `dotnet ef` tool is installed and on your PATH:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
export PATH="$PATH:$HOME/.dotnet/tools"
```

Delete the old SQLite migrations — they were generated for SQLite and are not compatible with SQL Server:

```bash
rm -rf Migrations/
```

Create a fresh migration for SQL Server and apply it:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

You should see the tables being created in the output:

```
Applying migration '20260101000000_InitialCreate'.
Done.
```

> If you see a connection error, check the connection string in `appsettings.json` has no spaces and uses the `tcp:` prefix.

---

## Part 5 — Publish the Project

### Step 1 — Build the publish output

Run this from inside `StudentAPI/StudentAPI/`:

```bash
dotnet publish -c Release -o ./publish
```

This compiles the project in Release mode and places all output files into a `publish/` folder.

### Step 2 — Zip the publish output

```bash
cd publish
zip -r ../studentapi_publish.zip .
cd ..
```

This creates `studentapi_publish.zip` in the `StudentAPI/StudentAPI/` folder.

### Step 3 — Download the zip from Codespaces

In the Codespaces file explorer on the left, find `studentapi_publish.zip`. Right-click it and select **Download**. Save it to your local machine.

### Step 4 — Upload via MonsterASP.NET File Manager

1. Log in to `admin.monsterasp.net`
2. Click on your website
3. Open the **File Manager**
4. Navigate to the `wwwroot` folder — this is where your site files live
5. Delete any existing files in `wwwroot`
6. Click **Upload** and select `studentapi_publish.zip`
7. Once uploaded, select the zip file and click **Extract** — extract into the current folder (`wwwroot`)

### Step 5 — Verify the deployment

Open your browser and go to:

```
http://yoursite.runasp.net/swagger/index.html
```

The Swagger page should load with all your endpoints. Click **Authorize**, enter your credentials, and test `POST /students` to confirm the database is connected.

---

## Part 6 — Updating After Code Changes

Every time you change your code and want to redeploy:

```bash
# From StudentAPI/StudentAPI/
dotnet publish -c Release -o ./publish
cd publish
zip -r ../studentapi_publish.zip .
cd ..
```

Then download the new zip and repeat the File Manager upload and extract steps in Part 5.

> If you changed the database model, run `dotnet ef migrations add YourMigrationName` and `dotnet ef database update` before publishing.

---

## Part 7 — Switching Back to SQLite for Local Development

After deploying, your `appsettings.json` points at the hosted database. If you want to continue developing locally against SQLite, switch it back:

**Option A — Use appsettings.Development.json (recommended)**

Create a file called `appsettings.Development.json` in the project root:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=student.db"
  }
}
```

ASP.NET Core automatically loads this file in Development mode and overrides the values in `appsettings.json`. In Codespaces the environment is Development by default, so local runs use SQLite and the hosted deployment uses SQL Server — no manual switching needed.

Also restore `UseSqlite` in `Program.cs` only for local development by checking the environment:

```csharp
if (app.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
```

**Option B — Manually swap (simpler)**

Just change the connection string in `appsettings.json` back to `Data Source=student.db` and change `UseSqlServer` back to `UseSqlite` when developing locally. Swap again before the next deploy.

---

## Part 8 — Troubleshooting

| Problem | Fix |
|---------|-----|
| Swagger does not appear at the live URL | Comment out the `IsDevelopment()` block in `Program.cs` around `UseSwagger()` and republish |
| 500 error on all endpoints | The SQL Server connection string in `appsettings.json` is wrong — check for spaces and the `tcp:` prefix |
| `dotnet ef` not found | Run `dotnet tool install --global dotnet-ef --version 8.0.0` then `export PATH="$PATH:$HOME/.dotnet/tools"` |
| Migration error after switching from SQLite | Delete the `Migrations/` folder and run `dotnet ef migrations add InitialCreate` again — SQLite migrations are not compatible with SQL Server |
| File manager extract does nothing | Make sure you are extracting into `wwwroot`, not into a subfolder. Delete all existing files first |
| `UseSqlServer` not found | Install `Microsoft.EntityFrameworkCore.SqlServer` — the SQLite package does not include it |
| Tables not created after `database update` | Check `Database=` is present in the connection string. Without it EF Core connects but has nowhere to create tables |

---

## Summary

| Step | What you do |
|------|-------------|
| Control panel | Create website and MSSQL database, copy connection string |
| Switch provider | Install SqlServer package, change `UseSqlite` to `UseSqlServer` in `Program.cs` |
| Connection string | Replace SQLite `Data Source=student.db` with the hosted SQL Server string |
| Swagger fix | Comment out `IsDevelopment()` block so Swagger runs in Production |
| Migrations | Delete old SQLite migrations, run `dotnet ef migrations add` and `database update` |
| Publish | `dotnet publish -c Release`, zip the output, upload and extract via File Manager |
| Update | Repeat publish → zip → upload → extract for every subsequent deployment |

---

*TECH 4263 — Server Application Technologies*
