namespace API.Data.Entities;

public class AppointmentSlot
{
    public int Id { get; init; }

    public required int ClinicId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public bool IsActive { get; init; } = true;

    public virtual Clinic Clinic { get; set; }
    public virtual Booking? Booking { get; set; }

}
