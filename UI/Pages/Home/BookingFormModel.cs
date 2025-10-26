using System.ComponentModel.DataAnnotations;

namespace UI.Pages.Home;

public sealed class BookingFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a clinic")]
    public int ClinicId { get; set; }

    [Required(ErrorMessage = "Select a slot")]
    public int? AppointmentSlotId { get; set; }

    [Required]
    [StringLength(128)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PatientEmail { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Notes { get; set; }
}
