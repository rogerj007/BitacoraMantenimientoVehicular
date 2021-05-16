using BitacoraMantenimientoVehicular.Datasource.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BitacoraMantenimientoVehicular.Datasource
{
    public class DataContext : IdentityDbContext<UserEntity>//DbContext//
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ClientEntity>()
                .HasIndex(t => new {t.Dni,t.Name})
                .IsUnique();

            builder.Entity<ColorEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<CountryEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<FuelEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<UserFunctionEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

          
            builder.Entity<VehicleBrandEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<VehicleStatusEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<MaintenanceTaskEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

           

            builder.Entity<UserEntity>().HasOne(u => u.UserFunction);

            builder.Entity<UserFunctionEntity>().HasOne(u => u.CreatedBy);
            builder.Entity<UserFunctionEntity>().HasOne(u => u.ModifiedBy);
        }

        public DbSet<ClientEntity> Client { get; set; }
        public DbSet<ColorEntity> Color { get; set; }
        public DbSet<MaintenanceTaskEntity> MaintenanceTask { get; set; }
        public DbSet<CountryEntity> Country { get; set; }
   
        public DbSet<FuelEntity> Fuel { get; set; }
        public DbSet<UserFunctionEntity> UserFunction { get; set; }
     
        public DbSet<VehicleBrandEntity> VehicleBrand { get; set; }
        public DbSet<VehicleEntity> Vehicle { get; set; }
        public DbSet<VehicleStatusEntity> VehicleStatus { get; set; }
        public DbSet<ComponentEntity> Component { get; set; }
        public DbSet<VehicleRecordActivityEntity> VehicleRecordActivities { get; set; }
      
    }
}
