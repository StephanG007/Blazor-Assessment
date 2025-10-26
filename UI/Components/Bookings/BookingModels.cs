using System;

namespace UI.Components.Bookings;

public sealed record ClinicSummary(
    int Id,
    string Name,
    string? City,
    string? Province,
    string? PhoneNumber,
    string? LogoBase64,
    int AvailableSlotsToday)
{
    public string DisplayLocation => (City, Province) switch
    {
        ({ } city, { } province) when !string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(province)
            => $"{city}, {province}",
        ({ } city, _) when !string.IsNullOrWhiteSpace(city)
            => city,
        _ => "Location unavailable"
    };

    public string SlotSummary => AvailableSlotsToday switch
    {
        0 => "No availability today",
        1 => "1 available slot today",
        _ => $"{AvailableSlotsToday} available slots today"
    };

    public string? LogoDataUrl => string.IsNullOrWhiteSpace(LogoBase64)
        ? null
        : $"{DetermineMimePrefix(LogoBase64!)}{LogoBase64}";

    private static string DetermineMimePrefix(string base64)
    {
        if (base64.StartsWith("/9j/", StringComparison.Ordinal))
        {
            return "data:image/jpeg;base64,";
        }

        return "data:image/png;base64,";
    }
}

public sealed record TimeSlotOption(TimeOnly StartTime, bool IsReserved = false)
{
    public string DisplayLabel => StartTime.ToString("HH:mm");
}

public sealed record DailyAvailability(DateOnly Date, IReadOnlyList<TimeSlotOption> Slots);

public sealed record ClinicSchedule(int ClinicId, IReadOnlyList<DailyAvailability> Availability);

public sealed record ScheduledSlot(DateOnly Date, TimeSlotOption Slot);
