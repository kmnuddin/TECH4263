# TECH 4263 — Lab Assignment 4
## Equipment Dashboard WinForms Application

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed work to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

In this assignment you will build a Windows Forms desktop application called `EquipmentDashboard` that consumes the `EquipmentAPI` you refactored in Lab 3. The app must be added directly into the **EquipmentAPI solution** so that both projects can be started and run at the same time from Visual Studio 2022.

You have already seen how a WinForms client works from the `StudentDashboard` tutorial. This assignment follows the same pattern — your job is to apply it independently for the Equipment domain.

---

## Important — Project Structure

The `EquipmentDashboard` WinForms project **must live inside the EquipmentAPI solution**, not as a standalone project.

This is required so Visual Studio can launch both the API and the dashboard at the same time using **Multiple Startup Projects**.

Your solution structure must look like this after setup:

```
EquipmentAPI/                        ← solution folder
  EquipmentAPI/                      ← existing API project
    Models/
    Program.cs
    EquipmentAPI.csproj
  EquipmentDashboard/                ← new WinForms project (you create this)
    Models/
    MainForm.cs
    MainForm.Designer.cs
    MainForm.resx
    Program.cs
    EquipmentDashboard.csproj
  EquipmentAPI.sln                   ← solution file that includes both projects
```

---

## Part 1 — Add the WinForms Project to the Solution

### Step 1 — Add a new project to the existing solution

1. Open the `EquipmentAPI` solution in Visual Studio 2022 by double-clicking `EquipmentAPI.sln`.
2. In Solution Explorer, right-click the **solution** (not the project) → **Add → New Project**.
3. Search for `Windows Forms` in the project template search box. You will see two similar options — select **Windows Forms App** (the one that says *.NET* underneath, **not** the one that says *.NET Framework*). Click Next.
4. Set the project name to `EquipmentDashboard`.
5. Set the location to the `EquipmentAPI/` solution folder (the same folder that contains `EquipmentAPI.sln`).
6. Click Next, set the Framework to match `EquipmentAPI` (e.g. .NET 9.0), click Create.

> **Why add to the same solution?** A solution is a container for multiple related projects. Adding `EquipmentDashboard` to the same solution lets Visual Studio build and launch both projects together with one click.

### Step 2 — Configure Multiple Startup Projects

1. Right-click the **solution** in Solution Explorer → **Properties**.
2. Under **Common Properties → Startup Project**, select **Multiple startup projects**.
3. Set both `EquipmentAPI` and `EquipmentDashboard` to **Action: Start**.
4. Make sure `EquipmentAPI` is listed first so it starts before the dashboard.
5. Click **OK**.

From now on, pressing **F5** will launch both projects simultaneously.

---

## Part 2 — Add the DTO Models

Create a `Models/` folder inside `EquipmentDashboard` and add two DTO classes. Look at your `EquipmentAPI` models from Lab 3 to decide which fields belong in each.

- **`EquipmentResponseDto.cs`** — the shape of what the API sends back to the client.
- **`CreateEquipmentDto.cs`** — the shape of what the client sends to the API on POST.

> **Hint:** The DTOs in `EquipmentDashboard` must match the DTOs in `EquipmentAPI` exactly. If the field names or types differ, deserialization will silently produce empty or null values.

---

## Part 3 — Designer File

Instead of building the layout manually in the Visual Studio Designer, replace the entire contents of `MainForm.Designer.cs` with the code below.

> **Note:** Do not edit `MainForm.Designer.cs` through the Visual Studio Designer after pasting this — the Designer will overwrite your changes. Keep all layout edits in this file directly.

**`MainForm.Designer.cs`**
```csharp
namespace EquipmentDashboard;

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
        // ── Containers ─────────────────────────────────────────────────────
        splitMain = new SplitContainer();   // horizontal: top | bottom
        splitTop  = new SplitContainer();   // vertical inside top: left | right

        // ── Left panel (equipment list) ────────────────────────────────────
        lblEquipmentHeader = new Label();
        lstEquipment       = new ListBox();

        // ── Right panel (equipment detail) ────────────────────────────────
        lblDetailHeader  = new Label();
        lblNoSelection   = new Label();
        pnlDetailFields  = new Panel();
        lblIdKey         = new Label();
        lblIdValue       = new Label();
        lblNameKey       = new Label();
        lblNameValue     = new Label();
        lblCategoryKey   = new Label();
        lblCategoryValue = new Label();
        lblStatusKey     = new Label();
        lblStatusValue   = new Label();
        lblLocationKey   = new Label();
        lblLocationValue = new Label();

        // ── Bottom panel (create equipment) ───────────────────────────────
        lblCreateHeader = new Label();
        lblName         = new Label();
        txtName         = new TextBox();
        lblCategory     = new Label();
        txtCategory     = new TextBox();
        lblStatus       = new Label();
        txtStatus       = new TextBox();
        lblLocation     = new Label();
        txtLocation     = new TextBox();
        btnCreate       = new Button();
        lblStatusMsg    = new Label();

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
        splitMain.SplitterDistance = 320;
        splitMain.SplitterWidth    = 6;
        splitMain.Panel2MinSize    = 160;
        splitMain.IsSplitterFixed  = true;
        splitMain.Name             = "splitMain";
        splitMain.TabIndex         = 0;
        splitMain.Panel1.Controls.Add(splitTop);
        splitMain.Panel2.Padding = new Padding(14, 8, 14, 8);
        splitMain.Panel2.Controls.Add(lblStatusMsg);
        splitMain.Panel2.Controls.Add(btnCreate);
        splitMain.Panel2.Controls.Add(txtLocation);
        splitMain.Panel2.Controls.Add(lblLocation);
        splitMain.Panel2.Controls.Add(txtStatus);
        splitMain.Panel2.Controls.Add(lblStatus);
        splitMain.Panel2.Controls.Add(txtCategory);
        splitMain.Panel2.Controls.Add(lblCategory);
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
        splitTop.Panel1.Controls.Add(lstEquipment);
        splitTop.Panel1.Controls.Add(lblEquipmentHeader);
        splitTop.Panel2.Padding = new Padding(14);
        splitTop.Panel2.Controls.Add(pnlDetailFields);
        splitTop.Panel2.Controls.Add(lblNoSelection);
        splitTop.Panel2.Controls.Add(lblDetailHeader);

        // Left — equipment list
        lblEquipmentHeader.AutoSize  = false;
        lblEquipmentHeader.Dock      = DockStyle.Top;
        lblEquipmentHeader.Height    = 30;
        lblEquipmentHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblEquipmentHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblEquipmentHeader.Name      = "lblEquipmentHeader";
        lblEquipmentHeader.Text      = "Equipment";
        lblEquipmentHeader.TextAlign = ContentAlignment.MiddleLeft;

        lstEquipment.Dock                  = DockStyle.Fill;
        lstEquipment.BorderStyle           = BorderStyle.FixedSingle;
        lstEquipment.Font                  = new Font("Segoe UI", 9.5F);
        lstEquipment.ItemHeight            = 22;
        lstEquipment.Name                  = "lstEquipment";
        lstEquipment.TabIndex              = 0;
        lstEquipment.SelectedIndexChanged += lstEquipment_SelectedIndexChanged;

        // Right — equipment detail
        lblDetailHeader.AutoSize  = false;
        lblDetailHeader.Dock      = DockStyle.Top;
        lblDetailHeader.Height    = 30;
        lblDetailHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDetailHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblDetailHeader.Name      = "lblDetailHeader";
        lblDetailHeader.Text      = "Equipment Details";
        lblDetailHeader.TextAlign = ContentAlignment.MiddleLeft;

        lblNoSelection.Dock      = DockStyle.Fill;
        lblNoSelection.Font      = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblNoSelection.ForeColor = Color.Gray;
        lblNoSelection.Name      = "lblNoSelection";
        lblNoSelection.Text      = "Select an item from the list to view details.";
        lblNoSelection.TextAlign = ContentAlignment.MiddleCenter;
        lblNoSelection.Visible   = true;

        pnlDetailFields.Dock    = DockStyle.Fill;
        pnlDetailFields.Name    = "pnlDetailFields";
        pnlDetailFields.Visible = false;
        pnlDetailFields.Controls.AddRange(new Control[]
        {
            lblIdKey,       lblIdValue,
            lblNameKey,     lblNameValue,
            lblCategoryKey, lblCategoryValue,
            lblStatusKey,   lblStatusValue,
            lblLocationKey, lblLocationValue
        });

        int rowH = 36;

        lblIdKey.AutoSize  = false;  lblIdKey.Location  = new Point(0, 4);
        lblIdKey.Size      = new Size(80, rowH);
        lblIdKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblIdKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblIdKey.Name      = "lblIdKey";  lblIdKey.Text = "ID";
        lblIdKey.TextAlign = ContentAlignment.MiddleLeft;

        lblIdValue.AutoSize  = false;  lblIdValue.Location  = new Point(90, 4);
        lblIdValue.Size      = new Size(300, rowH);
        lblIdValue.Font      = new Font("Segoe UI", 9.5F);
        lblIdValue.Name      = "lblIdValue";  lblIdValue.Text = "";
        lblIdValue.TextAlign = ContentAlignment.MiddleLeft;

        lblNameKey.AutoSize  = false;  lblNameKey.Location  = new Point(0, 4 + rowH);
        lblNameKey.Size      = new Size(80, rowH);
        lblNameKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblNameKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblNameKey.Name      = "lblNameKey";  lblNameKey.Text = "Name";
        lblNameKey.TextAlign = ContentAlignment.MiddleLeft;

        lblNameValue.AutoSize  = false;  lblNameValue.Location  = new Point(90, 4 + rowH);
        lblNameValue.Size      = new Size(300, rowH);
        lblNameValue.Font      = new Font("Segoe UI", 9.5F);
        lblNameValue.Name      = "lblNameValue";  lblNameValue.Text = "";
        lblNameValue.TextAlign = ContentAlignment.MiddleLeft;

        lblCategoryKey.AutoSize  = false;  lblCategoryKey.Location  = new Point(0, 4 + rowH * 2);
        lblCategoryKey.Size      = new Size(80, rowH);
        lblCategoryKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCategoryKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblCategoryKey.Name      = "lblCategoryKey";  lblCategoryKey.Text = "Category";
        lblCategoryKey.TextAlign = ContentAlignment.MiddleLeft;

        lblCategoryValue.AutoSize  = false;  lblCategoryValue.Location  = new Point(90, 4 + rowH * 2);
        lblCategoryValue.Size      = new Size(300, rowH);
        lblCategoryValue.Font      = new Font("Segoe UI", 9.5F);
        lblCategoryValue.Name      = "lblCategoryValue";  lblCategoryValue.Text = "";
        lblCategoryValue.TextAlign = ContentAlignment.MiddleLeft;

        lblStatusKey.AutoSize  = false;  lblStatusKey.Location  = new Point(0, 4 + rowH * 3);
        lblStatusKey.Size      = new Size(80, rowH);
        lblStatusKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblStatusKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblStatusKey.Name      = "lblStatusKey";  lblStatusKey.Text = "Status";
        lblStatusKey.TextAlign = ContentAlignment.MiddleLeft;

        lblStatusValue.AutoSize  = false;  lblStatusValue.Location  = new Point(90, 4 + rowH * 3);
        lblStatusValue.Size      = new Size(300, rowH);
        lblStatusValue.Font      = new Font("Segoe UI", 9.5F);
        lblStatusValue.Name      = "lblStatusValue";  lblStatusValue.Text = "";
        lblStatusValue.TextAlign = ContentAlignment.MiddleLeft;

        lblLocationKey.AutoSize  = false;  lblLocationKey.Location  = new Point(0, 4 + rowH * 4);
        lblLocationKey.Size      = new Size(80, rowH);
        lblLocationKey.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblLocationKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblLocationKey.Name      = "lblLocationKey";  lblLocationKey.Text = "Location";
        lblLocationKey.TextAlign = ContentAlignment.MiddleLeft;

        lblLocationValue.AutoSize  = false;  lblLocationValue.Location  = new Point(90, 4 + rowH * 4);
        lblLocationValue.Size      = new Size(300, rowH);
        lblLocationValue.Font      = new Font("Segoe UI", 9.5F);
        lblLocationValue.Name      = "lblLocationValue";  lblLocationValue.Text = "";
        lblLocationValue.TextAlign = ContentAlignment.MiddleLeft;

        // Bottom — add new equipment
        lblCreateHeader.AutoSize  = false;
        lblCreateHeader.Dock      = DockStyle.Top;
        lblCreateHeader.Height    = 28;
        lblCreateHeader.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCreateHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblCreateHeader.Name      = "lblCreateHeader";
        lblCreateHeader.Text      = "Add New Equipment";
        lblCreateHeader.TextAlign = ContentAlignment.MiddleLeft;

        // Row 1 — Name and Category
        int row1y = 36;

        lblName.AutoSize  = false;  lblName.Location  = new Point(0, row1y + 4);
        lblName.Size      = new Size(50, 24);  lblName.Font = new Font("Segoe UI", 9.5F);
        lblName.Name      = "lblName";  lblName.Text = "Name";
        lblName.TextAlign = ContentAlignment.MiddleRight;

        txtName.Location = new Point(56, row1y);  txtName.Size = new Size(160, 24);
        txtName.Font     = new Font("Segoe UI", 9.5F);
        txtName.Name     = "txtName";  txtName.TabIndex = 1;

        lblCategory.AutoSize  = false;  lblCategory.Location  = new Point(230, row1y + 4);
        lblCategory.Size      = new Size(64, 24);  lblCategory.Font = new Font("Segoe UI", 9.5F);
        lblCategory.Name      = "lblCategory";  lblCategory.Text = "Category";
        lblCategory.TextAlign = ContentAlignment.MiddleRight;

        txtCategory.Location = new Point(300, row1y);  txtCategory.Size = new Size(160, 24);
        txtCategory.Font     = new Font("Segoe UI", 9.5F);
        txtCategory.Name     = "txtCategory";  txtCategory.TabIndex = 2;

        // Row 2 — Status and Location
        int row2y = row1y + 36;

        lblStatus.AutoSize  = false;  lblStatus.Location  = new Point(0, row2y + 4);
        lblStatus.Size      = new Size(50, 24);  lblStatus.Font = new Font("Segoe UI", 9.5F);
        lblStatus.Name      = "lblStatus";  lblStatus.Text = "Status";
        lblStatus.TextAlign = ContentAlignment.MiddleRight;

        txtStatus.Location = new Point(56, row2y);  txtStatus.Size = new Size(160, 24);
        txtStatus.Font     = new Font("Segoe UI", 9.5F);
        txtStatus.Name     = "txtStatus";  txtStatus.TabIndex = 3;

        lblLocation.AutoSize  = false;  lblLocation.Location  = new Point(230, row2y + 4);
        lblLocation.Size      = new Size(64, 24);  lblLocation.Font = new Font("Segoe UI", 9.5F);
        lblLocation.Name      = "lblLocation";  lblLocation.Text = "Location";
        lblLocation.TextAlign = ContentAlignment.MiddleRight;

        txtLocation.Location = new Point(300, row2y);  txtLocation.Size = new Size(160, 24);
        txtLocation.Font     = new Font("Segoe UI", 9.5F);
        txtLocation.Name     = "txtLocation";  txtLocation.TabIndex = 4;

        btnCreate.Location = new Point(480, row1y);
        btnCreate.Size     = new Size(90, 58);
        btnCreate.Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnCreate.Name     = "btnCreate";  btnCreate.TabIndex = 5;  btnCreate.Text = "Create";
        btnCreate.UseVisualStyleBackColor = true;
        btnCreate.Click   += btnCreate_Click;

        lblStatusMsg.AutoSize  = false;  lblStatusMsg.Location  = new Point(0, row2y + 34);
        lblStatusMsg.Size      = new Size(560, 22);
        lblStatusMsg.Font      = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblStatusMsg.ForeColor = Color.Gray;
        lblStatusMsg.Name      = "lblStatusMsg";  lblStatusMsg.Text = "";

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(900, 520);
        Controls.Add(splitMain);
        Font          = new Font("Segoe UI", 9F);
        MinimumSize   = new Size(760, 500);
        Name          = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text          = "Equipment Dashboard";

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

    // ── Control declarations ───────────────────────────────────────────────
    private SplitContainer splitMain;
    private SplitContainer splitTop;
    private Label   lblEquipmentHeader;
    private ListBox lstEquipment;
    private Label   lblDetailHeader;
    private Label   lblNoSelection;
    private Panel   pnlDetailFields;
    private Label   lblIdKey;         private Label lblIdValue;
    private Label   lblNameKey;       private Label lblNameValue;
    private Label   lblCategoryKey;   private Label lblCategoryValue;
    private Label   lblStatusKey;     private Label lblStatusValue;
    private Label   lblLocationKey;   private Label lblLocationValue;
    private Label   lblCreateHeader;
    private Label   lblName;          private TextBox txtName;
    private Label   lblCategory;      private TextBox txtCategory;
    private Label   lblStatus;        private TextBox txtStatus;
    private Label   lblLocation;      private TextBox txtLocation;
    private Button  btnCreate;
    private Label   lblStatusMsg;
}
```

---

## Part 4 — Wire Up the Logic

Open `MainForm.cs` and implement the following:

### HttpClient

Declare a single shared `HttpClient` as a static field. Set its `BaseAddress` to match the EquipmentAPI port from `launchSettings.json`.

```csharp
private static readonly HttpClient _httpClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:XXXX")  // check EquipmentAPI launchSettings.json
};
```

Keep a `List<EquipmentResponseDto>` field to store the loaded equipment in memory.

### Load on startup

Call a `LoadEquipmentAsync()` method from the constructor (after `InitializeComponent()`) so the list populates as soon as the form opens.

### GET /equipments

- Call `GET /equipments` and deserialize the response into your list.
- Clear and repopulate the `ListBox` with equipment names.
- Handle `HttpRequestException` and show an error in the status label if the API is not reachable.
- Always check `IsSuccessStatusCode` before reading the response body.

### Selection changed

- When the user clicks an item in the `ListBox`, use the selected index to look up the matching item in your in-memory list.
- Populate the detail labels on the right.
- Toggle visibility: hide the placeholder label and show the detail panel.

### POST /equipments

- Validate that no input field is empty before making the API call.
- Build a `CreateEquipmentDto` from the input fields.
- Call `POST /equipments` using `PostAsJsonAsync`.
- On success: clear the inputs, show a success message in the status label, and call `LoadEquipmentAsync()` to refresh the list.
- On failure: show the status code in the status label.
- Re-enable the Create button in a `finally` block regardless of outcome.

---

## Grading Rubric

| | Task | Points |
|--|------|--------|
| 1 | `EquipmentDashboard` added to the `EquipmentAPI` solution (not standalone) | 3 |
| 2 | Multiple Startup Projects configured — both API and dashboard launch with F5 | 2 |
| 3 | `EquipmentResponseDto` and `CreateEquipmentDto` created with correct fields | 3 |
| 4 | `GET /equipments` called on startup, list populates correctly | 3 |
| 5 | Selecting an item shows its details in the right panel | 3 |
| 6 | `POST /equipments` sends correct DTO, list refreshes on success | 3 |
| 7 | Input validation, status feedback, and `HttpRequestException` handled | 3 |

---

## Submission Checklist

- [ ] `EquipmentDashboard` exists inside the `EquipmentAPI` solution folder
- [ ] Both projects appear in `EquipmentAPI.sln`
- [ ] Multiple Startup Projects configured with `EquipmentAPI` starting first
- [ ] Pressing F5 launches both the API and the dashboard
- [ ] `Models/EquipmentResponseDto.cs` created with correct fields
- [ ] `Models/CreateEquipmentDto.cs` created with correct fields
- [ ] List populates on startup from `GET /equipments`
- [ ] Clicking a list item shows details in the right panel
- [ ] Create button calls `POST /equipments` and refreshes the list
- [ ] Status label shows loading, success, and error states
- [ ] Both projects build with zero errors
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 4*
