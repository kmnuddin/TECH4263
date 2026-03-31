using Microsoft.EntityFrameworkCore;
using StudentAPI.Data;
using StudentAPI.Models;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




// POST /students
app.MapPost("/students", async (CreateStudentDto dto, AppDbContext context) =>
{
    var student = new Student(dto.Name, dto.Age, dto.Major);

    context.Students.Add(student);
    await context.SaveChangesAsync();

    return Results.Created($"/students/{student.Id}", new StudentResponseDto
    {
        Id = student.Id,
        Name = dto.Name,
        Major = dto.Major
    });
}).WithName("CreateStudent").WithOpenApi();

app.MapGet("/students", async (AppDbContext context) =>
{
    var students = await context.Students.ToListAsync();

   

    return Results.Ok(students.Select(s => new StudentResponseDto 
    {
        Id = s.Id,
        Name = s.Name,
        Major = s.Major
    }));
}).WithName("GetStudents").WithOpenApi();

// GET /students/{id}
app.MapGet("/students/{id:int:min(1)}", async (int id, AppDbContext context) =>
{
    var student = await context.Students.FindAsync(id);

    if (student is null)
        return Results.NotFound();

    return Results.Ok(new StudentResponseDto
    {
        Id = student.Id,
        Name = student.Name,
        Major = student.Major
    });
}).WithName("GetStudentById").WithOpenApi();

app.Run();


