using API.Data.Entities;

namespace API.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<Clinic>> GetClinicsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default);

    Task<Booking> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default);

    Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
}

public record BookingRequest(
    int ClinicId,
    int AppointmentSlotId,
    string PatientName,
    string PatientEmail,
    string? Notes);
