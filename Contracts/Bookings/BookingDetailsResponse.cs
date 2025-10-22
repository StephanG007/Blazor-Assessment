namespace Contracts.Bookings;

public record BookingDetailsResponse(
    string ClinicName,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName,
    string PatientEmail,
    string? Notes);
