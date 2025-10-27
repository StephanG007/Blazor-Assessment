using System.Text.Json.Serialization;

namespace API.Data.Entities;

public class AppointmentSlot
{
    public int Id { get; set; }

    public required int ClinicId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsActive { get; set; } = true;
    
    public virtual Clinic? Clinic { get; set; }
    public virtual Booking? Booking { get; set; }

}
