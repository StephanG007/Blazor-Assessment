using API.Controllers.Booking.DTOs;
using API.Data.Entities;

namespace API.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default);

    Task<BookingDetailsDto> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
}
