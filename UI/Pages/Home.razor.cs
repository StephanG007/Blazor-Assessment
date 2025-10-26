using System.Globalization;
using System.Net;
using Contracts.Account;
using Contracts.Bookings;
using Contracts.Clinics;
using Microsoft.AspNetCore.Components;
using UI.Services;

namespace UI.Pages;

public class HomePage : ComponentBase, IDisposable
{
    protected readonly BookingFormModel _bookingForm = new();
    protected readonly List<AvailableSlotResponse> _availableSlots = [];
    protected ClinicSummaryResponse? _clinicSummary;
    protected string? _clinicsLoadError;
    protected bool _isLoadingClinics;
    protected bool _isLoadingAvailability;
    protected string? _availabilityError;
    protected bool _isSubmitting;
    protected string? _submissionError;
    protected BookingDetailsResponse? _confirmation;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private CancellationTokenSource? _availabilityCts;

    [Inject]
    private BookingApiClient BookingApiClient { get; set; } = default!;

    [Inject]
    private AuthState AuthState { get; set; } = default!;

    protected bool HasClinics => _clinicSummary?.Clinics.Count > 0;

    protected int? SelectedClinicId => _bookingForm.ClinicId > 0 ? _bookingForm.ClinicId : null;

    protected ClinicSummaryDto? SelectedClinic => _clinicSummary?.Clinics.Find(clinic => clinic.Id == _bookingForm.ClinicId);

    protected string SelectedClinicHeading => SelectedClinic is { } clinic
        ? $"Available slots for {clinic.Name}"
        : "Available slots";

    protected string SelectedClinicSubtitle => SelectedClinic is { } clinic
        ? FormatLocation(clinic.City, clinic.Province)
        : "Choose a clinic to display its available slots.";

    protected string SelectedClinicStatus => GetSelectedClinicStatus();

    protected string SelectedDateLong => _selectedDate.ToString("dddd, dd MMMM yyyy", CultureInfo.CurrentCulture);

    protected string SelectedDateShort => _selectedDate.ToString("MMM d", CultureInfo.CurrentCulture);

    protected DateTime SelectedDate
    {
        get => _selectedDate.ToDateTime(TimeOnly.MinValue);
        set
        {
            var newDate = DateOnly.FromDateTime(value);
            if (newDate == _selectedDate)
            {
                return;
            }

            _selectedDate = newDate;
            _bookingForm.AppointmentSlotId = null;
            _ = InvokeAsync(() => LoadAvailabilityAsync());
        }
    }

    protected override void OnInitialized()
    {
        AuthState.StateChanged += OnAuthStateChanged;
        ApplyUserDetails(AuthState.CurrentUser);
    }

    protected override Task OnInitializedAsync() => LoadClinicsAsync();

    private async Task LoadClinicsAsync()
    {
        _clinicsLoadError = null;
        _isLoadingClinics = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            _clinicSummary = await BookingApiClient.GetClinicsAsync();

            if (HasClinics)
            {
                var clinics = _clinicSummary!.Clinics;
                if (_bookingForm.ClinicId == 0 || !clinics.Exists(clinic => clinic.Id == _bookingForm.ClinicId))
                {
                    _bookingForm.ClinicId = clinics[0].Id;
                }

                await LoadAvailabilityAsync();
            }
            else
            {
                _bookingForm.ClinicId = 0;
                _bookingForm.AppointmentSlotId = null;
                _availableSlots.Clear();
            }
        }
        catch (Exception)
        {
            _clinicsLoadError = "We couldn't load clinics at the moment. Please try again later.";
            _clinicSummary = null;
            _bookingForm.ClinicId = 0;
            _bookingForm.AppointmentSlotId = null;
            _availableSlots.Clear();
        }
        finally
        {
            _isLoadingClinics = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void OnClinicSelected(int? clinicId)
    {
        if (clinicId is null || clinicId.Value <= 0 || clinicId.Value == _bookingForm.ClinicId)
        {
            return;
        }

        _bookingForm.ClinicId = clinicId.Value;
        _bookingForm.AppointmentSlotId = null;
        _ = InvokeAsync(() => LoadAvailabilityAsync());
    }

    private async Task LoadAvailabilityAsync(bool preserveOutcome = false)
    {
        _availabilityCts?.Cancel();
        _availabilityCts?.Dispose();
        _availabilityCts = null;
        if (!preserveOutcome)
        {
            _confirmation = null;
            _submissionError = null;
        }

        if (_bookingForm.ClinicId == 0)
        {
            _availableSlots.Clear();
            _availabilityError = null;
            _isLoadingAvailability = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        var cts = new CancellationTokenSource();
        _availabilityCts = cts;

        _isLoadingAvailability = true;
        _availabilityError = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            var slots = await BookingApiClient.GetAvailabilityAsync(_bookingForm.ClinicId, _selectedDate, cts.Token);
            _availableSlots.Clear();
            _availableSlots.AddRange(slots);

            if (_availableSlots.Count == 0)
            {
                _bookingForm.AppointmentSlotId = null;
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _availabilityError = "Please log in to view the clinic availability.";
            _availableSlots.Clear();
            _bookingForm.AppointmentSlotId = null;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            _availabilityError = "We couldn't retrieve availability. Please try again.";
            _availableSlots.Clear();
            _bookingForm.AppointmentSlotId = null;
        }
        finally
        {
            if (!cts.IsCancellationRequested)
            {
                _isLoadingAvailability = false;
                await InvokeAsync(StateHasChanged);
            }

            _availabilityCts?.Dispose();
            _availabilityCts = null;
        }
    }

    protected void OnSlotSelected(int? slotId)
    {
        _bookingForm.AppointmentSlotId = slotId;
        _submissionError = null;
    }

    protected Task OnDateChanged(DateTime value)
    {
        SelectedDate = value;
        return Task.CompletedTask;
    }

    private string GetSelectedClinicStatus()
    {
        if (_isLoadingAvailability)
        {
            return "Loading slots...";
        }

        if (!string.IsNullOrEmpty(_availabilityError))
        {
            return _availabilityError!;
        }

        if (_availableSlots.Count == 0)
        {
            return $"No slots on {SelectedDateShort}";
        }

        var count = _availableSlots.Count;
        var suffix = count == 1 ? "slot" : "slots";
        return $"{count} available {suffix} on {SelectedDateShort}";
    }

    protected async Task SubmitAsync()
    {
        _submissionError = null;
        _confirmation = null;

        if (_bookingForm.AppointmentSlotId is null)
        {
            _submissionError = "Please select an available slot.";
            await InvokeAsync(StateHasChanged);
            return;
        }

        _isSubmitting = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            var request = new BookingRequest
            {
                ClinicId = _bookingForm.ClinicId,
                AppointmentSlotId = _bookingForm.AppointmentSlotId.Value,
                PatientName = _bookingForm.PatientName,
                PatientEmail = _bookingForm.PatientEmail,
                Notes = string.IsNullOrWhiteSpace(_bookingForm.Notes) ? null : _bookingForm.Notes
            };

            _confirmation = await BookingApiClient.CreateBookingAsync(request);
            _availableSlots.RemoveAll(slot => slot.Id == request.AppointmentSlotId);
            _bookingForm.AppointmentSlotId = null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _submissionError = "You need to be logged in to complete a booking.";
        }
        catch (Exception)
        {
            _submissionError = "We couldn't save the booking. Please try again.";
        }
        finally
        {
            _isSubmitting = false;
            await LoadAvailabilityAsync(preserveOutcome: true);
        }
    }

    private static string FormatLocation(string? city, string? province)
    {
        if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(province))
        {
            return "Location to be confirmed";
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return province!;
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            return city!;
        }

        return $"{city}, {province}";
    }

    internal static string FormatSlotRange(AvailableSlotResponse slot) =>
        $"{slot.StartTime.ToLocalTime():HH:mm} - {slot.EndTime.ToLocalTime():HH:mm}";

    private void OnAuthStateChanged()
    {
        ApplyUserDetails(AuthState.CurrentUser);
        _ = InvokeAsync(() => LoadAvailabilityAsync());
    }

    private void ApplyUserDetails(LoginResponse? user)
    {
        if (user is null)
            return;

        _bookingForm.PatientEmail = user.Email;
        if (string.IsNullOrWhiteSpace(_bookingForm.PatientName))
            _bookingForm.PatientName = user.DisplayName;
    }

    public void Dispose()
    {
        AuthState.StateChanged -= OnAuthStateChanged;
        _availabilityCts?.Cancel();
        _availabilityCts?.Dispose();
        _availabilityCts = null;
    }
}
