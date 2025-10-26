using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Bookings;
using Contracts.Clinics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Bookings;
using UI.Services;

namespace UI.Pages.Bookings;

public sealed partial class Bookings : ComponentBase, IDisposable
{
    private IReadOnlyList<ClinicSummary> Clinics { get; set; } = Array.Empty<ClinicSummary>();
    private ClinicSummary? SelectedClinic { get; set; }
    private DateRange SelectedRange { get; set; } = new();
    private ScheduledSlot? SelectedSlot { get; set; }
    private bool IsLoadingClinics { get; set; } = true;
    private bool IsLoadingAvailability { get; set; }
    private string? ClinicsError { get; set; }
    private string? AvailabilityError { get; set; }

    private IReadOnlyList<DailyAvailability> ClinicAvailability =>
        SelectedClinic is not null && _currentAvailabilityClinicId == SelectedClinic.Id
            ? _currentAvailability
            : Array.Empty<DailyAvailability>();

    private IReadOnlyList<DailyAvailability> _currentAvailability = Array.Empty<DailyAvailability>();
    private int? _currentAvailabilityClinicId;
    private CancellationTokenSource? _availabilityCts;

    [Inject]
    private BookingApiClient BookingApiClient { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

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
        ResetAvailabilityState();

        Navigation.NavigateTo("/", true);
        return false;
    }

    private async Task LoadClinicsAsync()
    {
        IsLoadingClinics = true;
        ClinicsError = null;
        IsLoadingAvailability = true;
        AvailabilityError = null;

        try
        {
            var response = await BookingApiClient.GetClinicsAsync();

            if (response is null || !response.Success)
            {
                ClinicsError = "We couldn't load clinics right now. Please try again soon.";
                ClearSelection();
                ResetAvailabilityState();
                AvailabilityError = ClinicsError;
                return;
            }

            var clinicDtos = response.Clinics ?? new List<ClinicSummaryDto>();

            Clinics = clinicDtos
                .Select(MapClinic)
                .OrderBy(clinic => clinic.Name)
                .ToList();

            SelectedClinic = Clinics.FirstOrDefault();
            SelectedRange = BuildInitialRange();
            SelectedSlot = null;

            if (SelectedClinic is not null)
            {
                await LoadAvailabilityAsync();
            }
            else
            {
                ResetAvailabilityState();
            }
        }
        catch
        {
            ClinicsError = "We couldn't load clinics right now. Please try again soon.";
            ClearSelection();
            ResetAvailabilityState();
            AvailabilityError = ClinicsError;
        }
        finally
        {
            IsLoadingClinics = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static ClinicSummary MapClinic(ClinicSummaryDto dto) => new(
        dto.Id,
        dto.Name,
        dto.City,
        dto.Province,
        dto.PhoneNumber,
        dto.ClinicLogo,
        0);

    private async Task HandleClinicChanged(ClinicSummary? clinic)
    {
        if (SelectedClinic?.Id == clinic?.Id)
        {
            return;
        }

        SelectedClinic = clinic;
        SelectedRange = BuildInitialRange();
        SelectedSlot = null;

        await LoadAvailabilityAsync();
    }

    private async Task HandleRangeChanged(DateRange range)
    {
        SelectedRange = range ?? new DateRange();
        SelectedSlot = null;

        await LoadAvailabilityAsync();
    }

    private async Task HandleSlotChanged(ScheduledSlot? slot)
    {
        SelectedSlot = slot;

        if (slot is null || SelectedClinic is null)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        var parameters = new DialogParameters
        {
            { nameof(BookTimeslotDialog.Clinic), SelectedClinic },
            { nameof(BookTimeslotDialog.Slot), slot }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            BackdropClick = false,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<BookTimeslotDialog>("Book appointment", parameters, options);

        DialogResult result;
        try
        {
            result = await dialog.Result ?? DialogResult.Cancel();
        }
        catch (TaskCanceledException)
        {
            SelectedSlot = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (!result.Canceled && result.Data is BookingDetailsResponse confirmation)
        {
            ShowBookingSuccess(confirmation);
            SelectedSlot = null;
            await LoadAvailabilityAsync();
            return;
        }

        SelectedSlot = null;
        await InvokeAsync(StateHasChanged);
    }

    private static DateRange BuildInitialRange() => new(
        DateTime.Today,
        DateTime.Today.AddDays(6));

    private void ClearSelection()
    {
        Clinics = Array.Empty<ClinicSummary>();
        SelectedClinic = null;
        SelectedRange = new DateRange();
        SelectedSlot = null;
    }

    private void ResetAvailabilityState()
    {
        _availabilityCts?.Cancel();
        _availabilityCts?.Dispose();
        _availabilityCts = null;

        _currentAvailability = Array.Empty<DailyAvailability>();
        _currentAvailabilityClinicId = null;
        AvailabilityError = null;
        IsLoadingAvailability = false;
    }

    private async Task LoadAvailabilityAsync()
    {
        _availabilityCts?.Cancel();
        _availabilityCts?.Dispose();
        _availabilityCts = null;

        if (SelectedClinic is null)
        {
            ResetAvailabilityState();
            await InvokeAsync(StateHasChanged);
            return;
        }

        var cts = new CancellationTokenSource();
        _availabilityCts = cts;

        IsLoadingAvailability = true;
        AvailabilityError = null;
        _currentAvailability = [];
        _currentAvailabilityClinicId = null;

        await InvokeAsync(StateHasChanged);

        try
        {
            var (start, end) = DetermineRequestedRange();
            var slots = await BookingApiClient.GetAvailabilityAsync(SelectedClinic.Id, start, end, cts.Token);
            var availability = BuildAvailability(slots);

            if (cts.IsCancellationRequested)
            {
                return;
            }

            _currentAvailabilityClinicId = SelectedClinic.Id;
            _currentAvailability = availability;

            UpdateClinicSummary(SelectedClinic.Id, availability);
            EnsureSelectedSlotIsValid(availability);
            EnsureRangeInitialized(availability);
        }
        catch (OperationCanceledException)
        {
            // Ignored
        }
        catch
        {
            if (!cts.IsCancellationRequested)
            {
                AvailabilityError = "We couldn't load availability right now. Please try again soon.";
            }
        }
        finally
        {
            if (_availabilityCts == cts)
            {
                _availabilityCts.Dispose();
                _availabilityCts = null;
            }

            if (!cts.IsCancellationRequested)
            {
                IsLoadingAvailability = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private (DateOnly Start, DateOnly End) DetermineRequestedRange()
    {
        if (SelectedRange.Start is null && SelectedRange.End is null)
        {
            var initialStart = DateOnly.FromDateTime(DateTime.Today);
            return (initialStart, initialStart.AddDays(6));
        }

        var rangeStart = SelectedRange.Start is not null
            ? DateOnly.FromDateTime(SelectedRange.Start.Value)
            : DateOnly.FromDateTime(SelectedRange.End!.Value).AddDays(-6);

        var rangeEnd = SelectedRange.End is not null
            ? DateOnly.FromDateTime(SelectedRange.End.Value)
            : rangeStart.AddDays(6);

        if (rangeEnd < rangeStart)
        {
            (rangeStart, rangeEnd) = (rangeEnd, rangeStart);
        }

        return (rangeStart, rangeEnd);
    }

    private static IReadOnlyList<DailyAvailability> BuildAvailability(IEnumerable<AvailableSlotResponse> slots)
    {
        return slots
            .GroupBy(slot => DateOnly.FromDateTime(slot.StartTime))
            .OrderBy(group => group.Key)
            .Select(group => new DailyAvailability(
                group.Key,
                group
                    .OrderBy(slot => slot.StartTime)
                    .Select(slot => new TimeSlotOption(
                        slot.Id,
                        TimeOnly.FromDateTime(slot.StartTime),
                        TimeOnly.FromDateTime(slot.EndTime),
                        slot.IsReserved))
                    .ToList()))
            .ToList();
    }

    private void UpdateClinicSummary(int clinicId, IReadOnlyList<DailyAvailability> availability)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var availableToday = availability
            .FirstOrDefault(day => day.Date == today)?
            .Slots.Count(slot => !slot.IsReserved) ?? 0;

        Clinics = Clinics
            .Select(clinic => clinic.Id == clinicId ? clinic with { AvailableSlotsToday = availableToday } : clinic)
            .OrderBy(clinic => clinic.Name)
            .ToList();

        if (SelectedClinic?.Id == clinicId)
        {
            SelectedClinic = Clinics.FirstOrDefault(clinic => clinic.Id == clinicId);
        }
    }

    private void EnsureSelectedSlotIsValid(IReadOnlyList<DailyAvailability> availability)
    {
        if (SelectedSlot is null)
        {
            return;
        }

        var day = availability.FirstOrDefault(a => a.Date == SelectedSlot.Date);
        if (day is null)
        {
            SelectedSlot = null;
            return;
        }

        var slot = day.Slots.FirstOrDefault(s => s.Id == SelectedSlot.Slot.Id && !s.IsReserved);
        if (slot is null)
        {
            SelectedSlot = null;
        }
        else
        {
            SelectedSlot = new ScheduledSlot(SelectedSlot.Date, slot);
        }
    }

    private void ShowBookingSuccess(BookingDetailsResponse confirmation)
    {
        var formatted = confirmation.StartTime.ToString("dddd, MMM d 'at' HH:mm");
        Snackbar.Add($"Appointment confirmed for {formatted} at {confirmation.ClinicName}.", Severity.Success);
    }

    private void EnsureRangeInitialized(IReadOnlyList<DailyAvailability> availability)
    {
        if (availability.Count == 0)
        {
            return;
        }

        if (SelectedRange.Start is not null && SelectedRange.End is not null)
        {
            return;
        }

        var first = availability.Min(a => a.Date);
        var last = availability.Max(a => a.Date);

        SelectedRange = new DateRange(
            first.ToDateTime(TimeOnly.MinValue),
            last.ToDateTime(TimeOnly.MinValue));
    }

    private void HandleAuthStateChanged()
    {
        if (AuthState.CurrentUser is null)
        {
            _availabilityCts?.Cancel();
            _ = InvokeAsync(() => Navigation.NavigateTo("/", true));
        }
    }

    public void Dispose()
    {
        AuthState.StateChanged -= HandleAuthStateChanged;
        _availabilityCts?.Cancel();
        _availabilityCts?.Dispose();
    }
}
