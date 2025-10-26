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
        : LogoBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? LogoBase64
            : $"{DetectMimePrefix(LogoBase64)}{LogoBase64}";
    
    private static string DetectMimePrefix(string b64)
    {
        if (b64.StartsWith("/9j/", StringComparison.Ordinal)) return "data:image/jpeg;base64,";
        if (b64.StartsWith("iVBOR", StringComparison.Ordinal)) return "data:image/png;base64,";
        if (b64.StartsWith("R0lGOD", StringComparison.Ordinal)) return "data:image/gif;base64,";
        if (b64.StartsWith("UklGR", StringComparison.Ordinal)) return "data:image/webp;base64,";
        if (b64.StartsWith("PHN2Zy", StringComparison.Ordinal)) return "data:image/svg+xml;base64,";

        return "data:image/png;base64,";
    }
}

public sealed record TimeSlotOption(int Id, TimeOnly StartTime, TimeOnly EndTime, bool IsReserved = false)
{
    public string DisplayLabel => StartTime.ToString("HH:mm");
}

public sealed record DailyAvailability(DateOnly Date, IReadOnlyList<TimeSlotOption> Slots);

public sealed record ClinicSchedule(int ClinicId, IReadOnlyList<DailyAvailability> Availability);

public sealed record ScheduledSlot(DateOnly Date, TimeSlotOption Slot);
