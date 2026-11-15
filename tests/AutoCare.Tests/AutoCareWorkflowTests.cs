using AutoCare.Data;
using AutoCare.Domain;
using AutoCare.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Tests;

public class AutoCareWorkflowTests
{
    [Fact]
    public void RepairOrder_CalculatesPartsAndLaborTotal()
    {
        var order = new RepairOrder
        {
            LaborCost = 125m,
            Parts =
            [
                new RepairOrderPart { Quantity = 2, UnitPrice = 30m },
                new RepairOrderPart { Quantity = 1, UnitPrice = 17.50m }
            ]
        };

        Assert.Equal(77.50m, order.PartsCost);
        Assert.Equal(202.50m, order.TotalCost);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    public void Part_ReportsLowStockAtMinimum(int quantity, int minimum, bool expected)
    {
        Assert.Equal(expected, new Part { Quantity = quantity, MinimumStock = minimum }.IsLowStock);
    }

    [Fact]
    public async Task SaveAppointment_RejectsOverlappingMechanicSchedule()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var factory = CreateFactory(connection);
        await using (var db = await factory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
            var mechanic = new ApplicationUser { Id = "mechanic-1", UserName = "mechanic@example.local" };
            var customer = new Customer { Name = "Test Customer" };
            var vehicle = new Vehicle
            {
                Customer = customer, Make = "Honda", Model = "Civic", Year = 2022,
                RegistrationNumber = "TEST-001"
            };
            db.Users.Add(mechanic);
            db.Appointments.Add(new Appointment
            {
                Customer = customer, Vehicle = vehicle, RequestedService = "Inspection",
                StartsAt = DateTime.Now.AddDays(2).Date.AddHours(10), DurationMinutes = 60,
                MechanicId = mechanic.Id
            });
            await db.SaveChangesAsync();
        }

        var appointment = new Appointment
        {
            CustomerId = 1,
            VehicleId = 1,
            RequestedService = "Oil change",
            StartsAt = DateTime.Now.AddDays(2).Date.AddHours(10).AddMinutes(30),
            DurationMinutes = 30,
            MechanicId = "mechanic-1"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AutoCareService(factory).SaveAppointmentAsync(appointment));
    }

    [Fact]
    public async Task UsePart_UpdatesStockAndRepairRecordAtomically()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var factory = CreateFactory(connection);
        int orderId;
        int partId;
        await using (var db = await factory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
            var customer = new Customer { Name = "Test Customer" };
            var vehicle = new Vehicle
            {
                Customer = customer, Make = "Honda", Model = "Civic", Year = 2022,
                RegistrationNumber = "TEST-002"
            };
            var order = new RepairOrder { Vehicle = vehicle, LaborCost = 40m };
            var part = new Part { Name = "Oil filter", Sku = "OIL-01", Quantity = 5, MinimumStock = 1, UnitPrice = 12m };
            db.AddRange(order, part);
            await db.SaveChangesAsync();
            orderId = order.Id;
            partId = part.Id;
        }

        var service = new AutoCareService(factory);
        await service.UsePartAsync(orderId, partId, 2, "admin", isAdministrator: true);

        await using (var db = await factory.CreateDbContextAsync())
        {
            var part = await db.Parts.SingleAsync(item => item.Id == partId);
            var link = await db.RepairOrderParts.SingleAsync(item => item.RepairOrderId == orderId && item.PartId == partId);
            Assert.Equal(3, part.Quantity);
            Assert.Equal(2, link.Quantity);
            Assert.Equal(12m, link.UnitPrice);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UsePartAsync(orderId, partId, 4, "admin", isAdministrator: true));
    }

    [Fact]
    public async Task UpdateRepairStatus_RejectsMechanicNotAssignedToOrder()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var factory = CreateFactory(connection);
        int orderId;
        await using (var db = await factory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
            var customer = new Customer { Name = "Test Customer" };
            var vehicle = new Vehicle
            {
                Customer = customer, Make = "Honda", Model = "Civic", Year = 2022,
                RegistrationNumber = "TEST-003"
            };
            var order = new RepairOrder { Vehicle = vehicle, Status = RepairStatus.Pending };
            db.RepairOrders.Add(order);
            await db.SaveChangesAsync();
            orderId = order.Id;
        }

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new AutoCareService(factory).UpdateRepairStatusAsync(orderId, RepairStatus.InProgress, "mechanic-2", isAdministrator: false));
    }

    private static async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static IDbContextFactory<ApplicationDbContext> CreateFactory(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        return new TestDbContextFactory(options);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => new(options);
    }
}
