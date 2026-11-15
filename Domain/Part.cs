using System.ComponentModel.DataAnnotations;

namespace AutoCare.Domain;

public class Part
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Required, StringLength(40)]
    public string Sku { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 1000000)]
    public int Quantity { get; set; }

    [Range(0, 1000000)]
    public int MinimumStock { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal UnitPrice { get; set; }

    public List<RepairOrderPart> RepairOrders { get; set; } = [];
    public bool IsLowStock => Quantity <= MinimumStock;
}
