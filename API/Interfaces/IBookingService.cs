using Contracts.Bookings;

namespace API.Interfaces;

public interface IBookingService
{
    Task<ServiceResult<List<AvailableSlotResponse>>> GetAvailableSlotsAsync(int clinicId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default);

    Task<ServiceResult<BookingDetailsResponse>> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<BookingDetailsResponse>> GetBookingByIdAsync(int bookingId, CancellationToken ct = default);

    Task<ServiceResult<BookingDetailsResponse>> DeleteBookingAsync(int bookingId, CancellationToken ct = default);
}
