using StudentAPI.Models;
using System.Data.SqlClient;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var students = new List<Student>(); // In-memory list to store students for demo purposes

// POST /students
app.MapPost("/students", async (CreateStudentDto dto) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    // OUTPUT INSERTED.Id returns the auto-generated Id from SQL Server
    using var command = new SqlCommand(
        @"INSERT INTO Students (Name, Age, Major)
          OUTPUT INSERTED.Id
          VALUES (@Name, @Age, @Major)", connection);

    command.Parameters.AddWithValue("@Name", dto.Name);
    command.Parameters.AddWithValue("@Age", dto.Age);
    command.Parameters.AddWithValue("@Major", dto.Major);

    // ExecuteScalarAsync returns the single value from OUTPUT INSERTED.Id
    var newId = (int)(await command.ExecuteScalarAsync())!;

    return Results.Created($"/students/{newId}", new StudentResponseDto
    {
        Id = newId,
        Name = dto.Name,
        Major = dto.Major
    });
}).WithName("CreateStudent").WithOpenApi();

app.MapGet("/students", async () =>
{
    var students = new List<StudentResponseDto>();

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand("SELECT Id, Name, Major FROM Students", connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        students.Add(new StudentResponseDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Major = reader.GetString(reader.GetOrdinal("Major"))
        });
    }

    return Results.Ok(students);
}).WithName("GetStudents").WithOpenApi();

// GET /students/{id}
app.MapGet("/students/{id:int:min(1)}", async (int id) =>
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new SqlCommand(
        "SELECT Id, Name, Major FROM Students WHERE Id = @Id", connection);

    // Always use parameters — never concatenate user input into SQL
    command.Parameters.AddWithValue("@Id", id);

    using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound();

    return Results.Ok(new StudentResponseDto
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Major = reader.GetString(reader.GetOrdinal("Major"))
    });
}).WithName("GetStudentById").WithOpenApi();

app.Run();


