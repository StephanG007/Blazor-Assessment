namespace API.Controllers.Booking.DTOs;

public record AvailableSlotDto(int Id, DateTime StartTime, DateTime EndTime);
