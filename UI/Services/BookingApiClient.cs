using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Bookings;
using Contracts.Clinics;

namespace UI.Services;

public sealed class BookingApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task<ClinicSummaryResponse?> GetClinicsAsync(CancellationToken ct = default) =>
        httpClient.GetFromJsonAsync<ClinicSummaryResponse>("api/clinics", SerializerOptions, ct);

    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailabilityAsync(int clinicId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        var query = $"startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var url = $"api/booking/clinics/{clinicId}/availability?{query}";
        
        return await httpClient.GetFromJsonAsync<List<AvailableSlotResponse>>(url, SerializerOptions, ct) ?? [];
    }

    public async Task<BookingDetailsResponse?> CreateBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/booking/create", request, SerializerOptions, ct);

        if (response.IsSuccessStatusCode)
            throw new Exception("We couldn't complete your booking right now. Please try again soon.");
        
        return await response.Content.ReadFromJsonAsync<BookingDetailsResponse>(SerializerOptions, ct);
    }
}
