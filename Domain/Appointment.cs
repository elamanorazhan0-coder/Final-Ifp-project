using System.ComponentModel.DataAnnotations;
using AutoCare.Data;

namespace AutoCare.Domain;

public class Appointment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    [Required, StringLength(120)]
    public string RequestedService { get; set; } = "";

    public DateTime StartsAt { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; } = 60;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? MechanicId { get; set; }
    public ApplicationUser? Mechanic { get; set; }
    public RepairOrder? RepairOrder { get; set; }
}
