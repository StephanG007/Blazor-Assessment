using API.Data;
using API.Data.Entities;
using API.Extensions;
using API.Interfaces;
using Contracts.Bookings;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class BookingService(AppDbContext dbContext) : IBookingService
{
    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var clinicExists = await dbContext.Clinics
            .AsNoTracking()
            .AnyAsync(clinic => clinic.Id == clinicId, cancellationToken);

        if (!clinicExists)
            throw new KeyNotFoundException($"Clinic with id {clinicId} was not found.");

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var availableSlots = await dbContext.AppointmentSlots
            .AsNoTracking()
            .Include(a => a.Booking)
            .Where(slot => slot.ClinicId == clinicId
                && slot.IsActive
                && slot.Booking == null
                && slot.StartTime >= dayStart
                && slot.StartTime < dayEnd)
            .OrderBy(slot => slot.StartTime)
            .Select(slot => new AvailableSlotResponse (slot.Id, slot.StartTime, slot.EndTime))
            .ToListAsync(cancellationToken);

        return availableSlots;
    }

    public async Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default)
    {
        var slot = await dbContext.AppointmentSlots
            .Where(slot => slot.IsActive 
               && slot.ClinicId == request.ClinicId
               && slot.Id == request.AppointmentSlotId
               && slot.Booking == null)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(slot == null)
            throw new Exception($" with id {request.AppointmentSlotId} was not available.");   

        var booking = new Booking
        {
            AppointmentSlotId = request.AppointmentSlotId,
            AppointmentSlot = slot,
            PatientName = request.PatientName,
            PatientEmail = request.PatientEmail,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Bookings.Add(booking);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create booking.", ex);
        }

        return booking.ToDetailsResponse();
    }

    public async Task<BookingDetailsResponse?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Include(b => b.AppointmentSlot)
                .ThenInclude(a => a.Clinic)
            .Select(b => b.ToDetailsResponse())
            .FirstOrDefaultAsync(cancellationToken);
    }
}
