using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public sealed class BookingApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IReadOnlyList<ClinicSummary>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        var clinics = await _httpClient.GetFromJsonAsync<IReadOnlyList<ClinicSummary>>(
            "booking/clinics",
            cancellationToken);

        return clinics ?? Array.Empty<ClinicSummary>();
    }

    public async Task<IReadOnlyList<AvailableSlot>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var path = $"booking/clinics/{clinicId}/availability?date={date:yyyy-MM-dd}";
        var slots = await _httpClient.GetFromJsonAsync<IReadOnlyList<AvailableSlot>>(path, cancellationToken);
        return slots ?? Array.Empty<AvailableSlot>();
    }

    public async Task<BookingConfirmation> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ClinicId is null || request.AppointmentSlotId is null)
        {
            throw new InvalidOperationException("Clinic and appointment slot must be selected before booking.");
        }

        var response = await _httpClient.PostAsJsonAsync("booking", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Booking request failed ({response.StatusCode}). {detail}");
        }

        var confirmation = await response.Content.ReadFromJsonAsync<BookingConfirmation>(cancellationToken: cancellationToken);
        if (confirmation is null)
        {
            throw new InvalidOperationException("Booking confirmation could not be parsed.");
        }

        return confirmation;
    }
}
