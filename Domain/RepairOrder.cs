using System.ComponentModel.DataAnnotations;
using AutoCare.Data;

namespace AutoCare.Domain;

public class RepairOrder
{
    public int Id { get; set; }
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string? MechanicId { get; set; }
    public ApplicationUser? Mechanic { get; set; }
    public RepairStatus Status { get; set; } = RepairStatus.Pending;

    [StringLength(1000)]
    public string Diagnosis { get; set; } = "";

    [StringLength(2000)]
    public string WorkDescription { get; set; } = "";

    [Range(0, 1000000)]
    public decimal LaborCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<RepairOrderPart> Parts { get; set; } = [];

    public decimal PartsCost => Parts.Sum(item => item.Quantity * item.UnitPrice);
    public decimal TotalCost => LaborCost + PartsCost;
}
