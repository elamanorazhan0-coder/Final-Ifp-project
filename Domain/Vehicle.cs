using System.ComponentModel.DataAnnotations;

namespace AutoCare.Domain;

public class Vehicle
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    [Required, StringLength(60)]
    public string Make { get; set; } = "";

    [Required, StringLength(60)]
    public string Model { get; set; } = "";

    [Range(1950, 2100)]
    public int Year { get; set; }

    [Required, StringLength(20)]
    public string RegistrationNumber { get; set; } = "";

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<Appointment> Appointments { get; set; } = [];
    public List<RepairOrder> RepairOrders { get; set; } = [];
}
