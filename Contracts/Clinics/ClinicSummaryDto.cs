namespace Contracts.Clinics;

public class ClinicSummaryResponse{
    public required bool Success { get; set; }
    public List<ClinicSummaryDto> Clinics { get; set; } = [];
};

public record ClinicSummaryDto(
    int Id,
    string Name,
    string? City,
    string? Province,
    string? PhoneNumber);
