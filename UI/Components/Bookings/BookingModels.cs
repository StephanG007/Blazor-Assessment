namespace UI.Components.Bookings;

public sealed record ClinicSummary(Guid Id, string Name, string City, string Province, int AvailableSlotsToday)
{
    public string DisplayLocation => string.IsNullOrWhiteSpace(Province)
        ? City
        : $"{City}, {Province}";

    public string SlotSummary => AvailableSlotsToday switch
    {
        0 => "No availability today",
        1 => "1 available slot today",
        _ => $"{AvailableSlotsToday} available slots today"
    };
}

public sealed record TimeSlotOption(TimeOnly StartTime, bool IsReserved = false)
{
    public string DisplayLabel => StartTime.ToString("HH:mm");
}

public sealed record DailyAvailability(DateOnly Date, IReadOnlyList<TimeSlotOption> Slots);

public sealed record ClinicSchedule(Guid ClinicId, IReadOnlyList<DailyAvailability> Availability);
