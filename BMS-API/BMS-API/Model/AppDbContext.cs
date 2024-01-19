using Microsoft.EntityFrameworkCore;

namespace BMS.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceCapability> DeviceCapabilities { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }

        public DbSet<DeviceReading> DeviceReadings { get; set; }
    }
}
