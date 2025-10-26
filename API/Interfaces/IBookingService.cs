using Contracts.Bookings;

namespace API.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(int clinicId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default);

    Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDetailsResponse?> GetBookingByIdAsync(int bookingId, CancellationToken ct = default);

    Task DeleteBookingAsync(int bookingId, CancellationToken ct = default);
}
