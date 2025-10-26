using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Bookings;
using Contracts.Clinics;
using Microsoft.AspNetCore.Mvc;

namespace UI.Services;

public sealed class BookingApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task<ClinicSummaryResponse?> GetClinicsAsync(CancellationToken ct = default) =>
        httpClient.GetFromJsonAsync<ClinicSummaryResponse>("api/clinics", SerializerOptions, ct);

    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailabilityAsync(int clinicId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        var query = $"startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var endpoint = $"api/booking/clinics/{clinicId}/availability?{query}";
        var slots = await httpClient.GetFromJsonAsync<List<AvailableSlotResponse>>(endpoint, SerializerOptions, ct);
        return slots ?? [];
    }

    public async Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/booking/create", request, SerializerOptions, ct);

        if (response.IsSuccessStatusCode)
        {
            var confirmation = await response.Content.ReadFromJsonAsync<BookingDetailsResponse>(SerializerOptions, ct);
            return confirmation ?? throw new InvalidOperationException("Received an empty booking confirmation from the server.");
        }

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(SerializerOptions, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Ignore parsing failures and fall back to a generic message
        }

        var message = problem?.Detail;
        if (string.IsNullOrWhiteSpace(message))
        {
            message = problem?.Title;
        }

        message ??= "We couldn't complete your booking right now. Please try again soon.";

        throw new BookingRequestException(message, response.StatusCode);
    }
}
