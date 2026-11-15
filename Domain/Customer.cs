using System.ComponentModel.DataAnnotations;

namespace AutoCare.Domain;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [EmailAddress, StringLength(160)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public List<Vehicle> Vehicles { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
}
