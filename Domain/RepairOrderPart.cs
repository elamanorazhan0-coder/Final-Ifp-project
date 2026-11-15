namespace AutoCare.Domain;

public class RepairOrderPart
{
    public int RepairOrderId { get; set; }
    public RepairOrder RepairOrder { get; set; } = null!;
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
