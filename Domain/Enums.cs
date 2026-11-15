namespace AutoCare.Domain;

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled
}

public enum RepairStatus
{
    Pending,
    InProgress,
    WaitingForParts,
    Completed,
    Cancelled
}
