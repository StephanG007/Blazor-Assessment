using System.Net.Http.Json;
using System.Globalization;
using Contracts.Bookings;
using Contracts.Clinics;

namespace UI.Services;

public class BookingApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ClinicSummaryResponse>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        var clinics = await httpClient.GetFromJsonAsync<IReadOnlyList<ClinicSummaryResponse>>("api/clinics", cancellationToken);
        return clinics ?? Array.Empty<ClinicSummaryResponse>();
    }

    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailabilityAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var url = $"api/booking/clinics/{clinicId}/availability?date={date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var slots = await response.Content.ReadFromJsonAsync<IReadOnlyList<AvailableSlotResponse>>(cancellationToken: cancellationToken);
        return slots ?? Array.Empty<AvailableSlotResponse>();
    }

    public async Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/booking/create", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var confirmation = await response.Content.ReadFromJsonAsync<BookingDetailsResponse>(cancellationToken: cancellationToken);
        return confirmation ?? throw new InvalidOperationException("Received an empty booking confirmation from the server.");
    }
}
