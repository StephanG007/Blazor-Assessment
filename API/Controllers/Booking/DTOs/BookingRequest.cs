using System.ComponentModel.DataAnnotations;

namespace API.Controllers.Booking.DTOs;

public class BookingRequest
{
    [Required]
    public int ClinicId { get; set; }

    [Required]
    public int AppointmentSlotId { get; set; }

    [Required]
    [StringLength(128)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PatientEmail { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Notes { get; set; }
}
