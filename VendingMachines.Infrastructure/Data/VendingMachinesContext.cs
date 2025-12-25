using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Core.Models;

namespace VendingMachines.Infrastructure.Data;

public partial class VendingMachinesContext : DbContext
{
    public VendingMachinesContext()
    {
    }

    public VendingMachinesContext(DbContextOptions<VendingMachinesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceModel> DeviceModels { get; set; }

    public virtual DbSet<DeviceStatus> DeviceStatuses { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Modem> Modems { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookings_pkey");

            entity.Property(e => e.Insurance).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::character varying");

            entity.HasOne(d => d.Company).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_companies");

            entity.HasOne(d => d.Device).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("companies_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contracts_pkey");

            entity.HasOne(d => d.Company).WithMany(p => p.Contracts)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_companies");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("devices_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.DeviceStatusId).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Company).WithMany(p => p.Devices)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_companies");

            entity.HasOne(d => d.DeviceModel).WithMany(p => p.Devices)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_device_models");

            entity.HasOne(d => d.DeviceStatus).WithMany(p => p.Devices)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_device_statuses");

            entity.HasOne(d => d.Location).WithMany(p => p.Devices).HasConstraintName("fk_locations");

            entity.HasOne(d => d.Modem).WithMany(p => p.Devices)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_modems");
        });

        modelBuilder.Entity<DeviceModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_models_pkey");

            entity.HasOne(d => d.DeviceType).WithMany(p => p.DeviceModels)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_device_types");
        });

        modelBuilder.Entity<DeviceStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_statuses_pkey");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_types_pkey");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.Property(e => e.DateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Device).WithMany(p => p.Events)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_pkey");

            entity.HasOne(d => d.Device).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_products");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("locations_pkey");
        });

        modelBuilder.Entity<Modem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("modems_pkey");

            entity.Property(e => e.Balance).HasDefaultValueSql("0");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.Property(e => e.Confirmed).HasDefaultValue(false);
            entity.Property(e => e.DateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Priority).HasDefaultValue(1);

            entity.HasOne(d => d.Device).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_users");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_methods_pkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SalesPopularity).HasDefaultValueSql("0");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_pkey");

            entity.Property(e => e.SaleDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Device).WithMany(p => p.Sales)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Sales)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_payment_methods");

            entity.HasOne(d => d.Product).WithMany(p => p.Sales)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_products");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("services_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Device).WithMany(p => p.Services)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_devices");

            entity.HasOne(d => d.User).WithMany(p => p.Services)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Language).HasDefaultValueSql("'en'::character varying");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Company).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_companies");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
