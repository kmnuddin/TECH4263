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
        splitMain = new SplitContainer();
        splitTop = new SplitContainer();
        lstEquipment = new ListBox();
        lblEquipmentHeader = new Label();
        pnlDetailFields = new Panel();
        lblIdKey = new Label();
        lblIdValue = new Label();
        lblNameKey = new Label();
        lblNameValue = new Label();
        lblCategoryKey = new Label();
        lblCategoryValue = new Label();
        lblStatusKey = new Label();
        lblStatusValue = new Label();
        lblLocationKey = new Label();
        lblLocationValue = new Label();
        lblNoSelection = new Label();
        lblDetailHeader = new Label();
        lblStatusMsg = new Label();
        btnCreate = new Button();
        txtLocation = new TextBox();
        lblLocation = new Label();
        txtStatus = new TextBox();
        lblStatus = new Label();
        txtCategory = new TextBox();
        lblCategory = new Label();
        txtName = new TextBox();
        lblName = new Label();
        lblCreateHeader = new Label();
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
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.FixedPanel = FixedPanel.Panel2;
        splitMain.IsSplitterFixed = true;
        splitMain.Location = new Point(0, 0);
        splitMain.Name = "splitMain";
        splitMain.Orientation = Orientation.Horizontal;
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(splitTop);
        // 
        // splitMain.Panel2
        // 
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
        splitMain.Panel2.Padding = new Padding(14, 8, 14, 8);
        splitMain.Panel2MinSize = 160;
        splitMain.Size = new Size(900, 520);
        splitMain.SplitterDistance = 356;
        splitMain.TabIndex = 0;
        // 
        // splitTop
        // 
        splitTop.Dock = DockStyle.Fill;
        splitTop.FixedPanel = FixedPanel.Panel1;
        splitTop.Location = new Point(0, 0);
        splitTop.Name = "splitTop";
        // 
        // splitTop.Panel1
        // 
        splitTop.Panel1.Controls.Add(lstEquipment);
        splitTop.Panel1.Controls.Add(lblEquipmentHeader);
        splitTop.Panel1.Padding = new Padding(10);
        splitTop.Panel1MinSize = 160;
        // 
        // splitTop.Panel2
        // 
        splitTop.Panel2.Controls.Add(pnlDetailFields);
        splitTop.Panel2.Controls.Add(lblNoSelection);
        splitTop.Panel2.Controls.Add(lblDetailHeader);
        splitTop.Panel2.Padding = new Padding(14);
        splitTop.Size = new Size(900, 356);
        splitTop.SplitterDistance = 121;
        splitTop.TabIndex = 0;
        // 
        // lstEquipment
        // 
        lstEquipment.BorderStyle = BorderStyle.FixedSingle;
        lstEquipment.Dock = DockStyle.Fill;
        lstEquipment.Font = new Font("Segoe UI", 9.5F);
        lstEquipment.ItemHeight = 17;
        lstEquipment.Location = new Point(10, 40);
        lstEquipment.Name = "lstEquipment";
        lstEquipment.Size = new Size(101, 306);
        lstEquipment.TabIndex = 0;
        // 
        // lblEquipmentHeader
        // 
        lblEquipmentHeader.Dock = DockStyle.Top;
        lblEquipmentHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblEquipmentHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblEquipmentHeader.Location = new Point(10, 10);
        lblEquipmentHeader.Name = "lblEquipmentHeader";
        lblEquipmentHeader.Size = new Size(101, 30);
        lblEquipmentHeader.TabIndex = 1;
        lblEquipmentHeader.Text = "Equipment";
        lblEquipmentHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pnlDetailFields
        // 
        pnlDetailFields.Controls.Add(lblIdKey);
        pnlDetailFields.Controls.Add(lblIdValue);
        pnlDetailFields.Controls.Add(lblNameKey);
        pnlDetailFields.Controls.Add(lblNameValue);
        pnlDetailFields.Controls.Add(lblCategoryKey);
        pnlDetailFields.Controls.Add(lblCategoryValue);
        pnlDetailFields.Controls.Add(lblStatusKey);
        pnlDetailFields.Controls.Add(lblStatusValue);
        pnlDetailFields.Controls.Add(lblLocationKey);
        pnlDetailFields.Controls.Add(lblLocationValue);
        pnlDetailFields.Dock = DockStyle.Fill;
        pnlDetailFields.Location = new Point(14, 44);
        pnlDetailFields.Name = "pnlDetailFields";
        pnlDetailFields.Size = new Size(747, 298);
        pnlDetailFields.TabIndex = 0;
        pnlDetailFields.Visible = false;
        // 
        // lblIdKey
        // 
        lblIdKey.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblIdKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblIdKey.Location = new Point(0, 4);
        lblIdKey.Name = "lblIdKey";
        lblIdKey.Size = new Size(80, 36);
        lblIdKey.TabIndex = 0;
        lblIdKey.Text = "ID";
        lblIdKey.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblIdValue
        // 
        lblIdValue.Font = new Font("Segoe UI", 9.5F);
        lblIdValue.Location = new Point(90, 4);
        lblIdValue.Name = "lblIdValue";
        lblIdValue.Size = new Size(300, 36);
        lblIdValue.TabIndex = 1;
        lblIdValue.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblNameKey
        // 
        lblNameKey.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblNameKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblNameKey.Location = new Point(0, 4);
        lblNameKey.Name = "lblNameKey";
        lblNameKey.Size = new Size(80, 36);
        lblNameKey.TabIndex = 2;
        lblNameKey.Text = "Name";
        lblNameKey.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblNameValue
        // 
        lblNameValue.Font = new Font("Segoe UI", 9.5F);
        lblNameValue.Location = new Point(90, 4);
        lblNameValue.Name = "lblNameValue";
        lblNameValue.Size = new Size(300, 36);
        lblNameValue.TabIndex = 3;
        lblNameValue.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblCategoryKey
        // 
        lblCategoryKey.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCategoryKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblCategoryKey.Location = new Point(0, 4);
        lblCategoryKey.Name = "lblCategoryKey";
        lblCategoryKey.Size = new Size(80, 36);
        lblCategoryKey.TabIndex = 4;
        lblCategoryKey.Text = "Category";
        lblCategoryKey.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblCategoryValue
        // 
        lblCategoryValue.Font = new Font("Segoe UI", 9.5F);
        lblCategoryValue.Location = new Point(90, 4);
        lblCategoryValue.Name = "lblCategoryValue";
        lblCategoryValue.Size = new Size(300, 36);
        lblCategoryValue.TabIndex = 5;
        lblCategoryValue.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblStatusKey
        // 
        lblStatusKey.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblStatusKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblStatusKey.Location = new Point(0, 4);
        lblStatusKey.Name = "lblStatusKey";
        lblStatusKey.Size = new Size(80, 36);
        lblStatusKey.TabIndex = 6;
        lblStatusKey.Text = "Status";
        lblStatusKey.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblStatusValue
        // 
        lblStatusValue.Font = new Font("Segoe UI", 9.5F);
        lblStatusValue.Location = new Point(90, 4);
        lblStatusValue.Name = "lblStatusValue";
        lblStatusValue.Size = new Size(300, 36);
        lblStatusValue.TabIndex = 7;
        lblStatusValue.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblLocationKey
        // 
        lblLocationKey.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblLocationKey.ForeColor = Color.FromArgb(100, 100, 120);
        lblLocationKey.Location = new Point(0, 4);
        lblLocationKey.Name = "lblLocationKey";
        lblLocationKey.Size = new Size(80, 36);
        lblLocationKey.TabIndex = 8;
        lblLocationKey.Text = "Location";
        lblLocationKey.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblLocationValue
        // 
        lblLocationValue.Font = new Font("Segoe UI", 9.5F);
        lblLocationValue.Location = new Point(90, 4);
        lblLocationValue.Name = "lblLocationValue";
        lblLocationValue.Size = new Size(300, 36);
        lblLocationValue.TabIndex = 9;
        lblLocationValue.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblNoSelection
        // 
        lblNoSelection.Dock = DockStyle.Fill;
        lblNoSelection.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblNoSelection.ForeColor = Color.Gray;
        lblNoSelection.Location = new Point(14, 44);
        lblNoSelection.Name = "lblNoSelection";
        lblNoSelection.Size = new Size(747, 298);
        lblNoSelection.TabIndex = 1;
        lblNoSelection.Text = "Select an item from the list to view details.";
        lblNoSelection.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblDetailHeader
        // 
        lblDetailHeader.Dock = DockStyle.Top;
        lblDetailHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDetailHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblDetailHeader.Location = new Point(14, 14);
        lblDetailHeader.Name = "lblDetailHeader";
        lblDetailHeader.Size = new Size(747, 30);
        lblDetailHeader.TabIndex = 2;
        lblDetailHeader.Text = "Equipment Details";
        lblDetailHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblStatusMsg
        // 
        lblStatusMsg.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblStatusMsg.ForeColor = Color.Gray;
        lblStatusMsg.Location = new Point(0, 36);
        lblStatusMsg.Name = "lblStatusMsg";
        lblStatusMsg.Size = new Size(560, 22);
        lblStatusMsg.TabIndex = 0;
        // 
        // btnCreate
        // 
        btnCreate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnCreate.Location = new Point(548, 19);
        btnCreate.Name = "btnCreate";
        btnCreate.Size = new Size(90, 58);
        btnCreate.TabIndex = 5;
        btnCreate.Text = "Create";
        btnCreate.UseVisualStyleBackColor = true;
        // 
        // txtLocation
        // 
        txtLocation.Font = new Font("Segoe UI", 9.5F);
        txtLocation.Location = new Point(300, 36);
        txtLocation.Name = "txtLocation";
        txtLocation.Size = new Size(160, 24);
        txtLocation.TabIndex = 4;
        // 
        // lblLocation
        // 
        lblLocation.Font = new Font("Segoe UI", 9.5F);
        lblLocation.Location = new Point(230, 36);
        lblLocation.Name = "lblLocation";
        lblLocation.Size = new Size(64, 24);
        lblLocation.TabIndex = 6;
        lblLocation.Text = "Location";
        lblLocation.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtStatus
        // 
        txtStatus.Font = new Font("Segoe UI", 9.5F);
        txtStatus.Location = new Point(56, 36);
        txtStatus.Name = "txtStatus";
        txtStatus.Size = new Size(160, 24);
        txtStatus.TabIndex = 3;
        // 
        // lblStatus
        // 
        lblStatus.Font = new Font("Segoe UI", 9.5F);
        lblStatus.Location = new Point(0, 36);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(50, 24);
        lblStatus.TabIndex = 7;
        lblStatus.Text = "Status";
        lblStatus.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtCategory
        // 
        txtCategory.Font = new Font("Segoe UI", 9.5F);
        txtCategory.Location = new Point(300, 36);
        txtCategory.Name = "txtCategory";
        txtCategory.Size = new Size(160, 24);
        txtCategory.TabIndex = 2;
        // 
        // lblCategory
        // 
        lblCategory.Font = new Font("Segoe UI", 9.5F);
        lblCategory.Location = new Point(230, 36);
        lblCategory.Name = "lblCategory";
        lblCategory.Size = new Size(64, 24);
        lblCategory.TabIndex = 8;
        lblCategory.Text = "Category";
        lblCategory.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtName
        // 
        txtName.Font = new Font("Segoe UI", 9.5F);
        txtName.Location = new Point(56, 36);
        txtName.Name = "txtName";
        txtName.Size = new Size(160, 24);
        txtName.TabIndex = 1;
        // 
        // lblName
        // 
        lblName.Font = new Font("Segoe UI", 9.5F);
        lblName.Location = new Point(0, 36);
        lblName.Name = "lblName";
        lblName.Size = new Size(50, 24);
        lblName.TabIndex = 9;
        lblName.Text = "Name";
        lblName.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblCreateHeader
        // 
        lblCreateHeader.Dock = DockStyle.Top;
        lblCreateHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblCreateHeader.ForeColor = Color.FromArgb(30, 70, 160);
        lblCreateHeader.Location = new Point(14, 8);
        lblCreateHeader.Name = "lblCreateHeader";
        lblCreateHeader.Size = new Size(872, 28);
        lblCreateHeader.TabIndex = 10;
        lblCreateHeader.Text = "Add New Equipment";
        lblCreateHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 520);
        Controls.Add(splitMain);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(760, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Equipment Dashboard";
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        splitMain.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        splitTop.Panel1.ResumeLayout(false);
        splitTop.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitTop).EndInit();
        splitTop.ResumeLayout(false);
        pnlDetailFields.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    // ── Control declarations ───────────────────────────────────────────────
    private SplitContainer splitMain;
    private SplitContainer splitTop;
    private Label lblEquipmentHeader;
    private ListBox lstEquipment;
    private Label lblDetailHeader;
    private Label lblNoSelection;
    private Panel pnlDetailFields;
    private Label lblIdKey; private Label lblIdValue;
    private Label lblNameKey; private Label lblNameValue;
    private Label lblCategoryKey; private Label lblCategoryValue;
    private Label lblStatusKey; private Label lblStatusValue;
    private Label lblLocationKey; private Label lblLocationValue;
    private Label lblCreateHeader;
    private Label lblName; private TextBox txtName;
    private Label lblCategory; private TextBox txtCategory;
    private Label lblStatus; private TextBox txtStatus;
    private Label lblLocation; private TextBox txtLocation;
    private Button btnCreate;
    private Label lblStatusMsg;
}