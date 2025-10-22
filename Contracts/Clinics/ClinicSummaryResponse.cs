namespace Contracts.Clinics;

public record ClinicSummaryResponse(
    int Id,
    string Name,
    string City,
    string Province,
    string PhoneNumber);
