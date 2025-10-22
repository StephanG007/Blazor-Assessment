namespace API.Data.Entities;

public class Clinic
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
