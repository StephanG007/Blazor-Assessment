namespace Contracts.Bookings;

public record AvailableSlotResponse(int Id, DateTime StartTime, DateTime EndTime, bool IsReserved, int? BookingId);
