using AutoCare.Data;
using AutoCare.Domain;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Services;

public sealed record DashboardSummary(
    int TotalAppointments,
    int ActiveRepairs,
    int CompletedRepairs,
    int LowStockParts,
    int AssignedRepairs,
    int TodayAppointments);

public class AutoCareService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<DashboardSummary> GetDashboardAsync(string? mechanicId = null)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var assignedOrders = db.RepairOrders.Where(order => order.MechanicId == mechanicId);
        return new DashboardSummary(
            await db.Appointments.CountAsync(appointment => appointment.Status != AppointmentStatus.Cancelled),
            await db.RepairOrders.CountAsync(order => order.Status == RepairStatus.Pending || order.Status == RepairStatus.InProgress || order.Status == RepairStatus.WaitingForParts),
            await db.RepairOrders.CountAsync(order => order.Status == RepairStatus.Completed),
            await db.Parts.CountAsync(part => part.Quantity <= part.MinimumStock),
            mechanicId is null ? 0 : await assignedOrders.CountAsync(order => order.Status != RepairStatus.Completed && order.Status != RepairStatus.Cancelled),
            mechanicId is null ? 0 : await db.Appointments.CountAsync(appointment => appointment.MechanicId == mechanicId && appointment.StartsAt.Date == today && appointment.Status != AppointmentStatus.Cancelled));
    }

    public async Task SaveAppointmentAsync(Appointment appointment)
    {
        if (appointment.StartsAt <= DateTime.Now)
            throw new InvalidOperationException("Appointment must be scheduled in the future.");

        await using var db = await contextFactory.CreateDbContextAsync();
        if (appointment.MechanicId is not null)
        {
            var endsAt = appointment.StartsAt.AddMinutes(appointment.DurationMinutes);
            var conflict = await db.Appointments.AnyAsync(existing =>
                existing.MechanicId == appointment.MechanicId &&
                existing.Id != appointment.Id &&
                existing.Status != AppointmentStatus.Cancelled &&
                existing.Status != AppointmentStatus.Completed &&
                appointment.StartsAt < existing.StartsAt.AddMinutes(existing.DurationMinutes) &&
                endsAt > existing.StartsAt);
            if (conflict)
                throw new InvalidOperationException("This mechanic already has an appointment at that time.");
        }

        if (appointment.Id == 0)
            db.Appointments.Add(appointment);
        else
            db.Appointments.Update(appointment);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var appointment = await db.Appointments.FindAsync(appointmentId)
            ?? throw new InvalidOperationException("Appointment was not found.");
        appointment.Status = status;
        await db.SaveChangesAsync();
    }

    public async Task CreateRepairOrderAsync(RepairOrder order)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        if (order.AppointmentId is not null &&
            await db.RepairOrders.AnyAsync(existing => existing.AppointmentId == order.AppointmentId))
            throw new InvalidOperationException("This appointment already has a repair order.");
        db.RepairOrders.Add(order);
        await db.SaveChangesAsync();
    }

    public async Task UpdateRepairStatusAsync(int orderId, RepairStatus status, string userId, bool isAdministrator)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var order = await db.RepairOrders.SingleOrDefaultAsync(item => item.Id == orderId)
            ?? throw new InvalidOperationException("Repair order was not found.");
        if (!isAdministrator && order.MechanicId != userId)
            throw new UnauthorizedAccessException("Mechanics can update only their assigned repair orders.");
        if (!isAdministrator && status is RepairStatus.Pending or RepairStatus.Cancelled)
            throw new InvalidOperationException("Mechanics cannot set this repair status.");
        if (order.Status is RepairStatus.Completed or RepairStatus.Cancelled)
            throw new InvalidOperationException("A closed repair order cannot be changed.");
        if (status == RepairStatus.Completed)
            order.CompletedAt = DateTime.UtcNow;
        order.Status = status;
        await db.SaveChangesAsync();
    }

    public async Task UsePartAsync(int orderId, int partId, int quantity, string userId, bool isAdministrator)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var part = await db.Parts.SingleOrDefaultAsync(item => item.Id == partId)
            ?? throw new InvalidOperationException("Part was not found.");
        var order = await db.RepairOrders.SingleOrDefaultAsync(item => item.Id == orderId && item.Status != RepairStatus.Cancelled);
        if (order is null)
            throw new InvalidOperationException("An active repair order is required.");
        if (!isAdministrator && order.MechanicId != userId)
            throw new UnauthorizedAccessException("Mechanics can use parts only on their assigned repair orders.");
        if (part.Quantity < quantity)
            throw new InvalidOperationException($"Only {part.Quantity} units are in stock.");

        part.Quantity -= quantity;
        var orderPart = await db.RepairOrderParts.FindAsync(orderId, partId);
        if (orderPart is null)
            db.RepairOrderParts.Add(new RepairOrderPart
            {
                RepairOrderId = orderId,
                PartId = partId,
                Quantity = quantity,
                UnitPrice = part.UnitPrice
            });
        else
            orderPart.Quantity += quantity;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task SavePartAsync(Part part)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        if (part.Id == 0)
            db.Parts.Add(part);
        else
            db.Parts.Update(part);
        await db.SaveChangesAsync();
    }

    public async Task DeletePartAsync(int partId)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var part = await db.Parts.FindAsync(partId)
            ?? throw new InvalidOperationException("Part was not found.");
        if (await db.RepairOrderParts.AnyAsync(item => item.PartId == partId))
            throw new InvalidOperationException("Parts already used in repairs cannot be deleted.");
        db.Parts.Remove(part);
        await db.SaveChangesAsync();
    }
}
