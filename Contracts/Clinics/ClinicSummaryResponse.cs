namespace Contracts.Clinics;

public class ClinicSummaryResponse
{
    public required bool Success { get; set; }

    public List<ClinicDto> Clinics { get; set; } = new();
}

public record ClinicDto(
    int Id,
    string Name,
    string? City,
    string? Province,
    string? PhoneNumber,
    string? ClinicLogo);
