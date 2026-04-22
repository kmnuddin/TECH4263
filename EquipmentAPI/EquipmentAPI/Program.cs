using EquipmentAPI.Data;
using EquipmentAPI.Models;
using Microsoft.EntityFrameworkCore;

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


app.MapPost("/equipment", async (CreateEquipmentDto equipmentDto, AppDbContext context) =>
{
    var equipment = new Equipment
    {
        Name = equipmentDto.Name,
        Category = equipmentDto.Category,
        Status = equipmentDto.Status,
        Location = equipmentDto.Location
    };
    context.Equipments.Add(equipment);
    await context.SaveChangesAsync();
    return Results.Created($"/equipment/{equipment.Id}", equipment);
}).WithName("CreateEquipment").WithOpenApi();

app.MapGet("/equipment", async (AppDbContext context) =>
{
    var equipments = await context.Equipments.ToListAsync();
    return Results.Ok(equipments);
}).WithName("GetEquipments").WithOpenApi();

app.MapGet("/equipment/{id}", async (int id, AppDbContext context) =>
{
    var eq = await context.Equipments.FindAsync(id);
    return eq != null ? Results.Ok(new ResponseEquipmentDto
    {
        Id = eq.Id,
        Name = eq.Name,
        Category = eq.Category,
        Status = eq.Status
    }) : Results.NotFound();
}).WithName("GetEquipmentById").WithOpenApi();


app.Run();


