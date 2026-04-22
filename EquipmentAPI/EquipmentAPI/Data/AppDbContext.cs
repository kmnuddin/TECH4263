using EquipmentAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Equipment> Equipments { get; set; }
    }
}
