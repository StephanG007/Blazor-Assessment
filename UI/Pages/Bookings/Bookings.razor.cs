using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Clinics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Bookings;
using UI.Services;

namespace UI.Pages.Bookings;

public sealed partial class Bookings : ComponentBase, IDisposable
{
    private readonly IReadOnlyDictionary<int, ClinicSchedule> _mockSchedule;

    private IReadOnlyList<ClinicSummary> Clinics { get; set; } = Array.Empty<ClinicSummary>();
    private ClinicSummary? SelectedClinic { get; set; }
    private DateRange SelectedRange { get; set; } = new();
    private ScheduledSlot? SelectedSlot { get; set; }
    private bool IsLoadingClinics { get; set; } = true;
    private string? ClinicsError { get; set; }

    [Inject]
    private BookingApiClient BookingApiClient { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

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
        AuthState.StateChanged += HandleAuthStateChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        if (!EnsureAuthenticated())
        {
            return;
        }

        await LoadClinicsAsync();
    }

    private bool EnsureAuthenticated()
    {
        if (AuthState.CurrentUser is not null)
        {
            return true;
        }

        IsLoadingClinics = false;
        Clinics = Array.Empty<ClinicSummary>();
        SelectedClinic = null;
        SelectedRange = new();
        SelectedSlot = null;

        Navigation.NavigateTo("/", true);
        return false;
    }

    private async Task LoadClinicsAsync()
    {
        IsLoadingClinics = true;
        ClinicsError = null;

        try
        {
            var response = await BookingApiClient.GetClinicsAsync();

            if (response is null || !response.Success)
            {
                ClinicsError = "We couldn't load clinics right now. Please try again soon.";
                Clinics = Array.Empty<ClinicSummary>();
                SelectedClinic = null;
                SelectedRange = new();
                SelectedSlot = null;
                return;
            }

            var clinicDtos = response.Clinics ?? new List<ClinicSummaryDto>();

            Clinics = clinicDtos
                .Select(MapClinic)
                .OrderBy(clinic => clinic.Name)
                .ToList();

            SelectedClinic = Clinics.FirstOrDefault();
            SelectedRange = BuildDefaultRange();
            SelectedSlot = null;
        }
        catch
        {
            ClinicsError = "We couldn't load clinics right now. Please try again soon.";
            Clinics = Array.Empty<ClinicSummary>();
            SelectedClinic = null;
            SelectedRange = new();
            SelectedSlot = null;
        }
        finally
        {
            IsLoadingClinics = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private ClinicSummary MapClinic(ClinicSummaryDto dto)
    {
        var availableToday = 0;

        if (_mockSchedule.TryGetValue(dto.Id, out var schedule))
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayAvailability = schedule.Availability.FirstOrDefault(day => day.Date == today);
            if (todayAvailability is not null)
            {
                availableToday = todayAvailability.Slots.Count(slot => !slot.IsReserved);
            }
        }

        return new ClinicSummary(
            dto.Id,
            dto.Name,
            dto.City,
            dto.Province,
            dto.PhoneNumber,
            dto.ClinicLogo,
            availableToday);
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

    private static IReadOnlyDictionary<int, ClinicSchedule> BuildMockSchedule()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var dayAfter = today.AddDays(2);

        TimeSlotOption[] BuildSlots(params int[] hours) => hours
            .Select(h => new TimeSlotOption(new TimeOnly(h, 0)))
            .ToArray();

        return new Dictionary<int, ClinicSchedule>
        {
            [1] = new ClinicSchedule(
                1,
                new[]
                {
                    new DailyAvailability(today, BuildSlots(9, 10, 11, 13, 15)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 12, 14)),
                    new DailyAvailability(dayAfter, BuildSlots(10, 11, 14, 16))
                }),
            [2] = new ClinicSchedule(
                2,
                new[]
                {
                    new DailyAvailability(today, BuildSlots(8, 9, 10, 11)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 11, 15)),
                    new DailyAvailability(dayAfter, BuildSlots(9, 10, 12))
                }),
            [3] = new ClinicSchedule(
                3,
                new[]
                {
                    new DailyAvailability(today, BuildSlots(10, 11, 12)),
                    new DailyAvailability(tomorrow, BuildSlots(11, 13, 15)),
                    new DailyAvailability(dayAfter, BuildSlots(9, 10, 11))
                }),
            [4] = new ClinicSchedule(
                4,
                new[]
                {
                    new DailyAvailability(today, BuildSlots(8, 10, 12)),
                    new DailyAvailability(tomorrow, BuildSlots(9, 12)),
                    new DailyAvailability(dayAfter, BuildSlots(10, 13, 15))
                })
        };
    }

    private void HandleAuthStateChanged()
    {
        if (AuthState.CurrentUser is null)
        {
            _ = InvokeAsync(() => Navigation.NavigateTo("/", true));
        }
    }

    public void Dispose()
    {
        AuthState.StateChanged -= HandleAuthStateChanged;
    }
}
