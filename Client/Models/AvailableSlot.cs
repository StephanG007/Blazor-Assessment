namespace Client.Models;

public sealed record AvailableSlot(int Id, DateTime StartTime, DateTime EndTime)
{
    public string DisplayLabel => $"{StartTime:MMM d, yyyy h:mm tt} - {EndTime:h:mm tt}";
}
