using System.Net.Http.Json;
using System.Text.Json;
using StudentDashboard.Models;

namespace StudentDashboard;

public partial class MainForm : Form
{
    // ── HTTP Client ────────────────────────────────────────────────────────
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7297") // ← update to match your StudentAPI port
    };

    // ── State ──────────────────────────────────────────────────────────────
    private List<StudentResponseDto> _students = new();

    public MainForm()
    {
        InitializeComponent();
        _ = LoadStudentsAsync();
    }

    // ── Load students from API ─────────────────────────────────────────────
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

            var json = await response.Content.ReadAsStringAsync();
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

    // ── List selection changed ─────────────────────────────────────────────
    private void lstStudents_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = lstStudents.SelectedIndex;

        if (index < 0 || index >= _students.Count)
        {
            lblNoSelection.Visible = true;
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

    // ── Create button clicked ──────────────────────────────────────────────
    private async void btnCreate_Click(object sender, EventArgs e)
    {
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

        var dto = new CreateStudentDto { Name = name, Age = age, Major = major };

        try
        {
            btnCreate.Enabled = false;
            SetStatus("Creating...", Color.Gray);

            var response = await _httpClient.PostAsJsonAsync("/students", dto);

            if (response.IsSuccessStatusCode)
            {
                txtName.Clear();
                txtAge.Clear();
                txtMajor.Clear();
                SetStatus($"Student '{name}' created successfully.", Color.SeaGreen);
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

    // ── Status helper ──────────────────────────────────────────────────────
    private void SetStatus(string message, Color color)
    {
        lblStatus.Text      = message;
        lblStatus.ForeColor = color;
    }
}
