namespace Client.Models;

public sealed record BookingConfirmation(
    int Id,
    string ClinicName,
    DateTime StartTime,
    DateTime EndTime,
    string PatientName,
    string PatientEmail,
    string? Notes);
