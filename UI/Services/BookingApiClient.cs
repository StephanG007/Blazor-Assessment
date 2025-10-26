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

        if (!response.IsSuccessStatusCode)
        {
            var message = response.StatusCode switch
            {
                HttpStatusCode.Conflict => "This time slot has already been booked. Please choose another slot.",
                HttpStatusCode.BadRequest => "Some booking details were invalid. Please review the information and try again.",
                _ => "We couldn't complete your booking right now. Please try again."
            };

            throw new BookingRequestException(message, response.StatusCode);
        }

        var confirmation = await response.Content.ReadFromJsonAsync<BookingDetailsResponse>(SerializerOptions, ct);

        return confirmation;
    }

    public async Task<BookingDetailsResponse?> GetBookingByIdAsync(int bookingId, CancellationToken ct = default)
    {
        using var response = await httpClient.GetAsync($"api/booking/{bookingId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new BookingRequestException("We couldn't load booking details right now. Please try again.", response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<BookingDetailsResponse>(SerializerOptions, ct);
    }

    public async Task DeleteBookingAsync(int bookingId, CancellationToken ct = default)
    {
        using var response = await httpClient.DeleteAsync($"api/booking/{bookingId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "We couldn't find that booking anymore.",
            _ => "We couldn't delete the booking right now. Please try again."
        };

        throw new BookingRequestException(message, response.StatusCode);
    }
}
