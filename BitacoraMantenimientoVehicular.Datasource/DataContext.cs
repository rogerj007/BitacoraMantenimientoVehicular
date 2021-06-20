using System;
using System.ComponentModel.DataAnnotations.Schema;
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

            builder.Entity<ClientEntity>(entity =>
                {
                    entity.HasIndex(t => new {t.Dni, t.Name}).IsUnique();
                    entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                    entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");
                   
                });
             
            builder.Entity<ColorEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<ColorEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");

            builder.Entity<CountryEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<CountryEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");

            builder.Entity<FuelEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<FuelEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");


            builder.Entity<VehicleEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<VehicleEntity>()
                .Property(b => b.CreatedDate)
                .HasColumnType("datetime");
            builder.Entity<VehicleEntity>()
                .Property(b => b.ModifiedDate)
                .HasColumnType("datetime");


            builder.Entity<VehicleBrandEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<VehicleBrandEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");

            builder.Entity<VehicleStatusEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<VehicleStatusEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");

            builder.Entity<UserEntity>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();


            builder.Entity<RecordNotificationEntity>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<RecordNotificationEntity>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");
          
        }

        public DbSet<ClientEntity> Client { get; set; }
        public DbSet<ColorEntity> Color { get; set; }
        public DbSet<CountryEntity> Country { get; set; }
   
        public DbSet<FuelEntity> Fuel { get; set; }
        public DbSet<VehicleBrandEntity> VehicleBrand { get; set; }
        public DbSet<VehicleEntity> Vehicle { get; set; }
        public DbSet<VehicleStatusEntity> VehicleStatus { get; set; }
        public DbSet<ComponentEntity> Component { get; set; }

        public DbSet<VehicleRecordActivityEntity> VehicleRecordActivity { get; set; }

        public DbSet<RecordNotificationEntity> RecordNotifications { get; set; }

        public DbSet<ComponentNextChangeEntity> ComponentNextChange { get; set; }

      

        public DbSet<ClientEntityVehicleEntity> ClientEntityVehicle { get; set; }
    }
}
