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
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => new {t.Dni, t.Name}).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");
               
            });

            builder.Entity<ColorEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t =>  t.Name ).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<CountryEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<FuelEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<VehicleEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<VehicleBrandEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<VehicleStatusEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");

            });

            builder.Entity<RecordNotificationEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();

            });

            builder.Entity<ComponentNextChangeEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                //entity.Property(b => b.Component).
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("getutcdate()").HasColumnType("datetimeoffset").IsRequired();
                entity.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");
            });

            builder.Entity<UserEntity>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
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
