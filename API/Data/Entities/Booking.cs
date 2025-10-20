using System.ComponentModel.DataAnnotations;

namespace API.Data.Entities;

public class Booking
{
    public int Id { get; set; }

    public int AppointmentSlotId { get; set; }

    [MaxLength(100)]
    public required string PatientName { get; set; }

    [MaxLength(200)]
    public required string PatientEmail { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Clinic? Clinic { get; set; }

    public virtual required AppointmentSlot AppointmentSlot { get; set; }
}
