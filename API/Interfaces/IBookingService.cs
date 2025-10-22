using Contracts.Bookings;

namespace API.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default);

    Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDetailsResponse?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
}
