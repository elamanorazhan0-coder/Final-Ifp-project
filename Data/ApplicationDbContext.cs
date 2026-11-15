using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoCare.Domain;

namespace AutoCare.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<RepairOrderPart> RepairOrderParts => Set<RepairOrderPart>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.RegistrationNumber)
            .IsUnique();
        builder.Entity<Part>()
            .HasIndex(part => part.Sku)
            .IsUnique();
        builder.Entity<RepairOrder>()
            .Property(order => order.LaborCost)
            .HasPrecision(10, 2);
        builder.Entity<RepairOrderPart>()
            .Property(item => item.UnitPrice)
            .HasPrecision(10, 2);
        builder.Entity<Part>()
            .Property(part => part.UnitPrice)
            .HasPrecision(10, 2);
        builder.Entity<Appointment>()
            .HasOne(appointment => appointment.Mechanic)
            .WithMany()
            .HasForeignKey(appointment => appointment.MechanicId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<RepairOrder>()
            .HasOne(order => order.Mechanic)
            .WithMany()
            .HasForeignKey(order => order.MechanicId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<RepairOrder>()
            .HasOne(order => order.Appointment)
            .WithOne(appointment => appointment.RepairOrder)
            .HasForeignKey<RepairOrder>(order => order.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<RepairOrderPart>()
            .HasKey(item => new { item.RepairOrderId, item.PartId });
        builder.Entity<RepairOrderPart>()
            .HasOne(item => item.Part)
            .WithMany(part => part.RepairOrders)
            .HasForeignKey(item => item.PartId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<RepairOrderPart>()
            .HasOne(item => item.RepairOrder)
            .WithMany(order => order.Parts)
            .HasForeignKey(item => item.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
