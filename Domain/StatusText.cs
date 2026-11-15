namespace AutoCare.Domain;

public static class StatusText
{
    public static string Format(string status) => status switch
    {
        "InProgress" => "In Progress",
        "WaitingForParts" => "Waiting for Parts",
        _ => status
    };
}
