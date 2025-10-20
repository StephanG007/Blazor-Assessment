using System.ComponentModel.DataAnnotations;

namespace API.Data.Entities;

public class Clinic
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public Address? Address { get; set; }

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
}
