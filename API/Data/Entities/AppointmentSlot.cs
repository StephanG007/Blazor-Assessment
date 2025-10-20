namespace API.Data.Entities;

public class AppointmentSlot
{
    public int Id { get; set; }

    public int ClinicId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsActive { get; set; } = true;

    public Clinic? Clinic { get; set; }

}
