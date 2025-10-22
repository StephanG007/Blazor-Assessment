namespace API.Data.Entities;

public class Booking
{
    public int Id { get; set; }

    public int ClinicId { get; set; }

    public int AppointmentSlotId { get; set; }

    public required string PatientName { get; set; }

    public required string PatientEmail { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Clinic? Clinic { get; set; }

    public AppointmentSlot? AppointmentSlot { get; set; }
}
