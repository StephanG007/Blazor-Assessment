namespace API.Controllers.Booking.DTOs;

public record BookingDetailsDto(
    int Id,
    int ClinicId,
    string ClinicName,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName,
    string PatientEmail,
    string? Notes,
    DateTime CreatedAt);
