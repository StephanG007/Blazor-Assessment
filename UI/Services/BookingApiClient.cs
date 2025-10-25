using System.Text.Json;
using Contracts.Bookings;
using Contracts.Clinics;

namespace UI.Services;

public class BookingApiClient(HttpClient httpClient)
{
    public async Task<ClinicSummaryResponse?> GetClinicsAsync(CancellationToken ct = default)
    {
        using var response = await httpClient.GetAsync("api/clinics", ct);
        response.EnsureSuccessStatusCode();
        
        var body =  await response.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<ClinicSummaryResponse>(body);
    }

    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailabilityAsync(int clinicId, DateOnly date, CancellationToken ct = default)
    {
        using var response = await httpClient.GetAsync($"api/booking/clinics/{clinicId}/availability?date={date:yyyy-MM-dd}", ct);
        response.EnsureSuccessStatusCode();
        
        var body = await response.Content.ReadAsStringAsync(ct);

        var slots =  JsonSerializer.Deserialize<List<AvailableSlotResponse>>(body);
        return slots ?? [];
    }

    public async Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsync(
            "api/booking/create", 
            new StringContent(JsonSerializer.Serialize(request))
        , ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        
        return JsonSerializer.Deserialize<BookingDetailsResponse>(body) 
               ?? throw new InvalidOperationException("Received an empty booking confirmation from the server.");
    }
}
