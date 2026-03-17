# Building a WinForms Client for the StudentAPI
**TECH 4263 — Server Application Technologies**

---

## Overview

In this tutorial you will build a Windows Forms desktop application that connects to the StudentAPI you refactored in Lab 3. The app has three panels:

- **Left panel** — a list of student names loaded from `GET /students`
- **Right panel** — detail labels showing the selected student's information
- **Bottom panel** — a form to create a new student via `POST /students`

> **Note:** Start your StudentAPI with `dotnet run` before launching the WinForms app. Note the HTTPS port from `launchSettings.json` — you will need it in Step 4.

---

## Step 1 — Create the Project

Open Visual Studio 2022. On the start screen click **Create a new project**.

1. Search for `Windows Forms` and select **Windows Forms App (.NET)**. Click Next.
2. Set Project name to `StudentDashboard`.
3. Set Location to your TECH4263 repo folder so it sits alongside `StudentAPI` and `EquipmentAPI`.
4. Click Next. Set Framework to **.NET 9.0**. Click Create.
5. Right-click `Form1.cs` in Solution Explorer → Rename → type `MainForm.cs`. Click **Yes** when asked to rename all references.

**Project structure after setup:**
```
StudentDashboard/
  Models/                       ← create this folder next
  MainForm.cs                   ← logic file
  MainForm.Designer.cs          ← layout file
  MainForm.resx
  Program.cs
  StudentDashboard.csproj
```

---

## Step 2 — Add DTO Models

The WinForms app needs to understand the same data shapes the StudentAPI sends and receives. Add a `Models` folder with two files.

1. Right-click the project → Add → New Folder → name it `Models`.
2. Right-click Models → Add → Class → name it `StudentResponseDto.cs`.

**`Models/StudentResponseDto.cs`**
```csharp
namespace StudentDashboard.Models;

public class StudentResponseDto
{
    public int    Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
}
```

3. Add a second class in Models named `CreateStudentDto.cs`.

**`Models/CreateStudentDto.cs`**
```csharp
namespace StudentDashboard.Models;

public class CreateStudentDto
{
    public string Name  { get; set; } = string.Empty;
    public int    Age   { get; set; }
    public string Major { get; set; } = string.Empty;
}
```

> **Note:** These DTOs are intentionally identical to the ones in StudentAPI. Both sides of an HTTP connection must agree on the shape of the data.

---

## Step 3 — Add the Designer File

Instead of building the layout manually in the Visual Studio Designer, replace `MainForm.Designer.cs` entirely with the code below. Open the file in Solution Explorer and replace all its contents.

**`MainForm.Designer.cs`**
```csharp
namespace StudentDashboard;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        // Containers
        splitMain = new SplitContainer();   // horizontal: top | bottom
        splitTop  = new SplitContainer();   // vertical inside top: left | right

        // Left panel
        lblStudentsHeader = new Label();
        lstStudents       = new ListBox();

        // Right panel
        lblDetailHeader = new Label();
        lblNoSelection  = new Label();
        pnlDetailFields = new Panel();
        lblIdKey        = new Label();
        lblIdValue      = new Label();
        lblNameKey      = new Label();
        lblNameValue    = new Label();
        lblMajorKey     = new Label();
        lblMajorValue   = new Label();

        // Bottom panel
        lblCreateHeader = new Label();
        lblName         = new Label();
        txtName         = new TextBox();
        lblAge          = new Label();
        txtAge          = new TextBox();
        lblMajor        = new Label();
        txtMajor        = new TextBox();
        btnCreate       = new Button();
        lblStatus       = new Label();

        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitTop).BeginInit();
        splitTop.Panel1.SuspendLayout();
        splitTop.Panel2.SuspendLayout();
        splitTop.SuspendLayout();
        pnlDetailFields.SuspendLayout();
        SuspendLayout();

        // splitMain: outer horizontal split — top area | bottom create panel
        splitMain.Dock             = DockStyle.Fill;
        splitMain.Orientation      = Orientation.Horizontal;
        splitMain.FixedPanel       = FixedPanel.Panel2;
        splitMain.SplitterDistance = 340;
        splitMain.SplitterWidth    = 6;
        splitMain.Panel2MinSize    = 130;
        splitMain.IsSplitterFixed  = true;
        splitMain.Name             = "splitMain";
        splitMain.TabIndex         = 0;
        splitMain.Panel1.Controls.Add(splitTop);
        splitMain.Panel2.Padding = new Padding(14, 8, 14, 8);
        splitMain.Panel2.Controls.Add(lblStatus);
        splitMain.Panel2.Controls.Add(btnCreate);
        splitMain.Panel2.Controls.Add(txtMajor);
        splitMain.Panel2.Controls.Add(lblMajor);
        splitMain.Panel2.Controls.Add(txtAge);
        splitMain.Panel2.Controls.Add(lblAge);
        splitMain.Panel2.Controls.Add(txtName);
        splitMain.Panel2.Controls.Add(lblName);
        splitMain.Panel2.Controls.Add(lblCreateHeader);

        // splitTop: inner vertical split — left list | right detail
        splitTop.Dock             = DockStyle.Fill;
        splitTop.Orientation      = Orientation.Vertical;
        splitTop.FixedPanel       = FixedPanel.Panel1;
        splitTop.SplitterDistance = 220;
        splitTop.SplitterWidth    = 6;
        splitTop.Panel1MinSize    = 160;
        splitTop.Name             = "splitTop";
        splitTop.TabIndex         = 0;
        splitTop.Panel1.Padding = new Padding(10);
        splitTop.Panel1.Controls.Add(lstStudents);
        splitTop.Panel1.Controls.Add(lblStudentsHeader);
        splitTop.Panel2.Padding = new Padding(14);
        splitTop.Panel2.Controls.Add(pnlDetailFields);
        splitTop.Panel2.Controls.Add(lblNoSelection);
        splitTop.Panel2.Controls.Add(lblDetailHeader);

        // Left — student list
        lblStudentsHeader.AutoSize  = false;
        lblStudentsHeader.Dock      = DockStyle.Top;
        lblStudentsHeader.Height    = 30;
        lblStudentsHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStudentsHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblStudentsHeader.Name      = "lblStudentsHeader";
        lblStudentsHeader.Text      = "Students";
        lblStudentsHeader.TextAlign = ContentAlignment.MiddleLeft;

        lstStudents.Dock                  = DockStyle.Fill;
        lstStudents.BorderStyle           = BorderStyle.FixedSingle;
        lstStudents.Font                  = new Font("Segoe UI", 9.5F);
        lstStudents.ItemHeight            = 22;
        lstStudents.Name                  = "lstStudents";
        lstStudents.TabIndex              = 0;
        lstStudents.SelectedIndexChanged += lstStudents_SelectedIndexChanged;

        // Right — student detail
        lblDetailHeader.AutoSize  = false;
        lblDetailHeader.Dock      = DockStyle.Top;
        lblDetailHeader.Height    = 30;
        lblDetailHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDetailHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblDetailHeader.Name      = "lblDetailHeader";
        lblDetailHeader.Text      = "Student Details";
        lblDetailHeader.TextAlign = ContentAlignment.MiddleLeft;

        lblNoSelection.Dock      = DockStyle.Fill;
        lblNoSelection.Font      = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblNoSelection.ForeColor = Color.Gray;
        lblNoSelection.Name      = "lblNoSelection";
        lblNoSelection.Text      = "Select a student from the list to view details.";
        lblNoSelection.TextAlign = ContentAlignment.MiddleCenter;
        lblNoSelection.Visible   = true;

        pnlDetailFields.Dock    = DockStyle.Fill;
        pnlDetailFields.Name    = "pnlDetailFields";
        pnlDetailFields.Visible = false;
        pnlDetailFields.Controls.AddRange(new Control[]
        {
            lblIdKey, lblIdValue, lblNameKey, lblNameValue, lblMajorKey, lblMajorValue
        });

        int rowH = 38;

        lblIdKey.AutoSize  = false;
        lblIdKey.Location  = new Point(0, 4);
        lblIdKey.Size      = new Size(70, rowH);
        lblIdKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblIdKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblIdKey.Name      = "lblIdKey";
        lblIdKey.Text      = "ID";
        lblIdKey.TextAlign = ContentAlignment.MiddleLeft;

        lblIdValue.AutoSize  = false;
        lblIdValue.Location  = new Point(80, 4);
        lblIdValue.Size      = new Size(300, rowH);
        lblIdValue.Font      = new Font("Segoe UI", 9.5F);
        lblIdValue.Name      = "lblIdValue";
        lblIdValue.Text      = "";
        lblIdValue.TextAlign = ContentAlignment.MiddleLeft;

        lblNameKey.AutoSize  = false;
        lblNameKey.Location  = new Point(0, 4 + rowH);
        lblNameKey.Size      = new Size(70, rowH);
        lblNameKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblNameKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblNameKey.Name      = "lblNameKey";
        lblNameKey.Text      = "Name";
        lblNameKey.TextAlign = ContentAlignment.MiddleLeft;

        lblNameValue.AutoSize  = false;
        lblNameValue.Location  = new Point(80, 4 + rowH);
        lblNameValue.Size      = new Size(300, rowH);
        lblNameValue.Font      = new Font("Segoe UI", 9.5F);
        lblNameValue.Name      = "lblNameValue";
        lblNameValue.Text      = "";
        lblNameValue.TextAlign = ContentAlignment.MiddleLeft;

        lblMajorKey.AutoSize  = false;
        lblMajorKey.Location  = new Point(0, 4 + rowH * 2);
        lblMajorKey.Size      = new Size(70, rowH);
        lblMajorKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblMajorKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblMajorKey.Name      = "lblMajorKey";
        lblMajorKey.Text      = "Major";
        lblMajorKey.TextAlign = ContentAlignment.MiddleLeft;

        lblMajorValue.AutoSize  = false;
        lblMajorValue.Location  = new Point(80, 4 + rowH * 2);
        lblMajorValue.Size      = new Size(300, rowH);
        lblMajorValue.Font      = new Font("Segoe UI", 9.5F);
        lblMajorValue.Name      = "lblMajorValue";
        lblMajorValue.Text      = "";
        lblMajorValue.TextAlign = ContentAlignment.MiddleLeft;

        // Bottom — add new student
        lblCreateHeader.AutoSize  = false;
        lblCreateHeader.Dock      = DockStyle.Top;
        lblCreateHeader.Height    = 28;
        lblCreateHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCreateHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblCreateHeader.Name      = "lblCreateHeader";
        lblCreateHeader.Text      = "Add New Student";
        lblCreateHeader.TextAlign = ContentAlignment.MiddleLeft;

        int iy = 38;

        lblName.AutoSize  = false;
        lblName.Location  = new Point(0, iy + 4);
        lblName.Size      = new Size(44, 24);
        lblName.Font      = new Font("Segoe UI", 9.5F);
        lblName.Name      = "lblName";
        lblName.Text      = "Name";
        lblName.TextAlign = ContentAlignment.MiddleRight;

        txtName.Location = new Point(50, iy);
        txtName.Size     = new Size(160, 24);
        txtName.Font     = new Font("Segoe UI", 9.5F);
        txtName.Name     = "txtName";
        txtName.TabIndex = 1;

        lblAge.AutoSize  = false;
        lblAge.Location  = new Point(224, iy + 4);
        lblAge.Size      = new Size(30, 24);
        lblAge.Font      = new Font("Segoe UI", 9.5F);
        lblAge.Name      = "lblAge";
        lblAge.Text      = "Age";
        lblAge.TextAlign = ContentAlignment.MiddleRight;

        txtAge.Location = new Point(260, iy);
        txtAge.Size     = new Size(60, 24);
        txtAge.Font     = new Font("Segoe UI", 9.5F);
        txtAge.Name     = "txtAge";
        txtAge.TabIndex = 2;

        lblMajor.AutoSize  = false;
        lblMajor.Location  = new Point(334, iy + 4);
        lblMajor.Size      = new Size(44, 24);
        lblMajor.Font      = new Font("Segoe UI", 9.5F);
        lblMajor.Name      = "lblMajor";
        lblMajor.Text      = "Major";
        lblMajor.TextAlign = ContentAlignment.MiddleRight;

        txtMajor.Location = new Point(384, iy);
        txtMajor.Size     = new Size(160, 24);
        txtMajor.Font     = new Font("Segoe UI", 9.5F);
        txtMajor.Name     = "txtMajor";
        txtMajor.TabIndex = 3;

        btnCreate.Location = new Point(560, iy - 1);
        btnCreate.Size     = new Size(90, 28);
        btnCreate.Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnCreate.Name     = "btnCreate";
        btnCreate.TabIndex = 4;
        btnCreate.Text     = "Create";
        btnCreate.UseVisualStyleBackColor = true;
        btnCreate.Click   += btnCreate_Click;

        lblStatus.AutoSize  = false;
        lblStatus.Location  = new Point(0, iy + 36);
        lblStatus.Size      = new Size(560, 22);
        lblStatus.Font      = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblStatus.ForeColor = Color.Gray;
        lblStatus.Name      = "lblStatus";
        lblStatus.Text      = "";

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(860, 500);
        Controls.Add(splitMain);
        Font          = new Font("Segoe UI", 9F);
        MinimumSize   = new Size(700, 460);
        Name          = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text          = "Student Dashboard";

        pnlDetailFields.ResumeLayout(false);
        splitTop.Panel1.ResumeLayout(false);
        splitTop.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitTop).EndInit();
        splitTop.ResumeLayout(false);
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        splitMain.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private SplitContainer splitMain;
    private SplitContainer splitTop;
    private Label   lblStudentsHeader;
    private ListBox lstStudents;
    private Label   lblDetailHeader;
    private Label   lblNoSelection;
    private Panel   pnlDetailFields;
    private Label   lblIdKey;
    private Label   lblIdValue;
    private Label   lblNameKey;
    private Label   lblNameValue;
    private Label   lblMajorKey;
    private Label   lblMajorValue;
    private Label   lblCreateHeader;
    private Label   lblName;
    private TextBox txtName;
    private Label   lblAge;
    private TextBox txtAge;
    private Label   lblMajor;
    private TextBox txtMajor;
    private Button  btnCreate;
    private Label   lblStatus;
}
```

> **Note:** Do not edit `MainForm.Designer.cs` through the Visual Studio Designer after pasting this — the Designer will overwrite it. Keep all layout changes in this file directly.

---

## Step 4 — HttpClient and State

Open `MainForm.cs` (the logic file). Replace its entire contents with the following.

**`MainForm.cs`**
```csharp
using System.Net.Http.Json;
using System.Text.Json;
using StudentDashboard.Models;

namespace StudentDashboard;

public partial class MainForm : Form
{
    // Single shared HttpClient — never create one per request
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7001") // update to your API port
    };

    // In-memory list — keeps the ListBox and detail panel in sync
    private List<StudentResponseDto> _students = new();

    public MainForm()
    {
        InitializeComponent();
        _ = LoadStudentsAsync();   // load on startup
    }
}
```

> **Note:** The `BaseAddress` must match the HTTPS port in your StudentAPI `launchSettings.json`. Open `StudentAPI/Properties/launchSettings.json` and look for `applicationUrl` to find your port.

---

## Step 5 — Load Students (GET /students)

Add this method to `MainForm.cs`. It calls `GET /students`, deserializes the JSON response, and fills the `ListBox`.

```csharp
private async Task LoadStudentsAsync()
{
    try
    {
        SetStatus("Loading...", Color.Gray);

        var response = await _httpClient.GetAsync("/students");

        if (!response.IsSuccessStatusCode)
        {
            SetStatus($"Error: {response.StatusCode}", Color.Red);
            return;
        }

        string json = await response.Content.ReadAsStringAsync();

        _students = JsonSerializer.Deserialize<List<StudentResponseDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<StudentResponseDto>();

        lstStudents.Items.Clear();
        foreach (var s in _students)
            lstStudents.Items.Add(s.Name);

        SetStatus(_students.Count > 0 ? "" : "No students found.", Color.Gray);
    }
    catch (HttpRequestException)
    {
        SetStatus("Cannot connect to StudentAPI. Is it running?", Color.Red);
    }
}
```

| Method | Purpose |
|--------|---------|
| `GetAsync("/students")` | Sends GET /students to the API |
| `IsSuccessStatusCode` | Checks the status before reading the body — always do this first |
| `ReadAsStringAsync()` | Reads the raw JSON string from the response body |
| `JsonSerializer.Deserialize` | Converts JSON into a typed `List<StudentResponseDto>` |
| `PropertyNameCaseInsensitive` | Handles camelCase JSON into PascalCase C# properties |
| `catch HttpRequestException` | Handles connection failures without crashing the app |

---

## Step 6 — Show Detail on Selection

Add this event handler to `MainForm.cs`. It fires when the user clicks a name in the list, looks up the matching student from `_students`, and populates the detail labels.

```csharp
private void lstStudents_SelectedIndexChanged(object sender, EventArgs e)
{
    int index = lstStudents.SelectedIndex;

    if (index < 0 || index >= _students.Count)
    {
        lblNoSelection.Visible  = true;
        pnlDetailFields.Visible = false;
        return;
    }

    var student = _students[index];

    lblIdValue.Text    = student.Id.ToString();
    lblNameValue.Text  = student.Name;
    lblMajorValue.Text = student.Major;

    lblNoSelection.Visible  = false;
    pnlDetailFields.Visible = true;
}
```

> **Note:** The selected index in the `ListBox` matches the index in `_students` because both are populated together in `LoadStudentsAsync`. Clearing and refilling both at the same time keeps them in sync.

---

## Step 7 — Create Student (POST /students)

Add this click handler to `MainForm.cs`. It validates the inputs, builds a `CreateStudentDto`, POSTs it to the API, and refreshes the list on success.

```csharp
private async void btnCreate_Click(object sender, EventArgs e)
{
    // 1. Validate
    string name  = txtName.Text.Trim();
    string major = txtMajor.Text.Trim();

    if (string.IsNullOrEmpty(name))
    {
        SetStatus("Name is required.", Color.OrangeRed);
        txtName.Focus();
        return;
    }
    if (!int.TryParse(txtAge.Text.Trim(), out int age) || age <= 0)
    {
        SetStatus("Age must be a positive number.", Color.OrangeRed);
        txtAge.Focus();
        return;
    }
    if (string.IsNullOrEmpty(major))
    {
        SetStatus("Major is required.", Color.OrangeRed);
        txtMajor.Focus();
        return;
    }

    // 2. Build DTO
    var dto = new CreateStudentDto { Name = name, Age = age, Major = major };

    try
    {
        btnCreate.Enabled = false;
        SetStatus("Creating...", Color.Gray);

        // 3. POST to API
        var response = await _httpClient.PostAsJsonAsync("/students", dto);

        if (response.IsSuccessStatusCode)
        {
            // 4. Clear inputs
            txtName.Clear();
            txtAge.Clear();
            txtMajor.Clear();

            SetStatus($"Student '{name}' created successfully.", Color.SeaGreen);

            // 5. Refresh the list
            await LoadStudentsAsync();
        }
        else
        {
            SetStatus($"Failed: {response.StatusCode}", Color.Red);
        }
    }
    catch (HttpRequestException)
    {
        SetStatus("Cannot connect to StudentAPI.", Color.Red);
    }
    finally
    {
        btnCreate.Enabled = true;
    }
}
```

> **Note:** `PostAsJsonAsync` serializes the DTO to JSON and sets `Content-Type: application/json` automatically. After a successful POST, `LoadStudentsAsync()` is called again so the new student appears in the list immediately.

---

## Step 8 — Status Helper and Run

Add this final helper to `MainForm.cs`:

```csharp
private void SetStatus(string message, Color color)
{
    lblStatus.Text      = message;
    lblStatus.ForeColor = color;
}
```

### Run and test

1. Start StudentAPI first in a terminal: `dotnet run` inside `StudentAPI/StudentAPI/`
2. Press **F5** in Visual Studio to launch StudentDashboard.
3. The list should show *Loading...* briefly, then populate with student names.
4. Fill in Name, Age, and Major in the bottom panel and click **Create**. The list should update automatically.
5. Click a name in the list — the right panel should show that student's ID, Name, and Major.

> **Troubleshooting:** If you see *Cannot connect to StudentAPI* — check that `dotnet run` is still active and that the port in `BaseAddress` matches your `launchSettings.json`.

### Complete MainForm.cs structure

```csharp
public partial class MainForm : Form
{
    // Fields
    private static readonly HttpClient _httpClient = ...;
    private List<StudentResponseDto> _students = new();

    // Constructor
    public MainForm() { InitializeComponent(); _ = LoadStudentsAsync(); }

    // API methods
    private async Task LoadStudentsAsync() { ... }

    // Event handlers
    private void lstStudents_SelectedIndexChanged(...) { ... }
    private async void btnCreate_Click(...) { ... }

    // Helpers
    private void SetStatus(string message, Color color) { ... }
}
```

---

*TECH 4263 — Server Application Technologies*
