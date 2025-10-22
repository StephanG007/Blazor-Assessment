using System.ComponentModel.DataAnnotations;

namespace Client.Models;

public sealed class BookingRequest
{
    [Required]
    public int? ClinicId { get; set; }

    [Required]
    public int? AppointmentSlotId { get; set; }

    [Required]
    [StringLength(100)]
    public string? PatientName { get; set; }

    [Required]
    [EmailAddress]
    public string? PatientEmail { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
