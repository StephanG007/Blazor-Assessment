using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Linq;
using UI.Components.Bookings;

namespace UI.Pages.Bookings;

public sealed partial class Bookings : ComponentBase
{
    private readonly IReadOnlyList<ClinicSummary> Clinics = new List<ClinicSummary>
    {
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Clinic A", "City", "Province", 12),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Clinic B", "City", "Province", 8),
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Clinic C", "City", "Province", 4),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Clinic D", "City", "Province", 3)
    };

    private readonly IReadOnlyDictionary<Guid, ClinicSchedule> _mockSchedule;

    private ClinicSummary? SelectedClinic { get; set; }

    private DateRange SelectedRange { get; set; } = new();

    private ScheduledSlot? SelectedSlot { get; set; }

    private IReadOnlyList<DailyAvailability> ClinicAvailability
    {
        get
        {
            if (SelectedClinic is null)
            {
                return Array.Empty<DailyAvailability>();
            }

            if (!_mockSchedule.TryGetValue(SelectedClinic.Id, out var schedule))
            {
                return Array.Empty<DailyAvailability>();
            }

            return schedule.Availability;
        }
    }

    public Bookings()
    {
        _mockSchedule = BuildMockSchedule();
    }

    protected override void OnInitialized()
    {
        SelectedClinic = Clinics.FirstOrDefault();
        if (SelectedClinic is null)
        {
            return;
        }

        SelectedRange = BuildDefaultRange();
    }

    private void HandleClinicChanged(ClinicSummary? clinic)
    {
        SelectedClinic = clinic;
        SelectedRange = BuildDefaultRange();
        SelectedSlot = null;
    }

    private void HandleRangeChanged(DateRange range)
    {
        SelectedRange = range ?? new DateRange();
        SelectedSlot = null;
    }

    private void HandleSlotChanged(ScheduledSlot? slot)
    {
        SelectedSlot = slot;
    }

    private DateRange BuildDefaultRange()
    {
        var availability = ClinicAvailability;
        if (availability.Count == 0)
        {
            return new DateRange();
        }

        var first = availability.Min(a => a.Date);
        var last = availability.Max(a => a.Date);

        return new DateRange(
            first.ToDateTime(TimeOnly.MinValue),
            last.ToDateTime(TimeOnly.MinValue));
    }

    private static IReadOnlyDictionary<Guid, ClinicSchedule> BuildMockSchedule()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var dayAfter = today.AddDays(2);

        TimeSlotOption[] BuildSlots(params int[] hours) => hours
            .Select(h => new TimeSlotOption(new TimeOnly(h, 0)))
            .ToArray();

        return new Dictionary<Guid, ClinicSchedule>
        {
            [Guid.Parse("11111111-1111-1111-1111-111111111111")] = new ClinicSchedule(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                new[]
                {
                    new DailyAvailability(today, BuildSlots(9, 10, 11, 13, 15)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 12, 14)),
                    new DailyAvailability(dayAfter, BuildSlots(10, 11, 14, 16))
                }),
            [Guid.Parse("22222222-2222-2222-2222-222222222222")] = new ClinicSchedule(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                new[]
                {
                    new DailyAvailability(today, BuildSlots(8, 9, 10, 11)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 11, 15)),
                    new DailyAvailability(dayAfter, BuildSlots(9, 10, 12))
                }),
            [Guid.Parse("33333333-3333-3333-3333-333333333333")] = new ClinicSchedule(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                new[]
                {
                    new DailyAvailability(today, BuildSlots(10, 11, 12)),
                    new DailyAvailability(tomorrow, BuildSlots(11, 13, 15)),
                    new DailyAvailability(dayAfter, BuildSlots(9, 10, 11))
                }),
            [Guid.Parse("44444444-4444-4444-4444-444444444444")] = new ClinicSchedule(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                new[]
                {
                    new DailyAvailability(today, BuildSlots(8, 10, 12)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 12)),
                    new DailyAvailability(dayAfter, BuildSlots(10, 13, 15))
                })
        };
    }
}
