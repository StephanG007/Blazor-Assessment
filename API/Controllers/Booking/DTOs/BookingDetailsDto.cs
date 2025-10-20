namespace API.Controllers.Booking.DTOs;

public record BookingDetailsDto(
    string ClinicName,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName,
    string PatientEmail,
    string? Notes);
