namespace Contracts.Bookings;

public record BookingConfirmationResponse(
    int Id,
    int ClinicId,
    string ClinicName,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName,
    string PatientEmail,
    string? Notes,
    DateTime CreatedAt);
