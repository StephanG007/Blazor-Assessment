using API.Data;
using API.Data.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class BookingService(AppDbContext dbContext) : IBookingService
{
    public async Task<IReadOnlyList<Clinic>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Clinics
            .AsNoTracking()
            .OrderBy(clinic => clinic.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var clinicExists = await dbContext.Clinics
            .AsNoTracking()
            .AnyAsync(clinic => clinic.Id == clinicId, cancellationToken);

        if (!clinicExists)
        {
            throw new KeyNotFoundException($"Clinic with id {clinicId} was not found.");
        }

        var dayStart = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var bookedSlotIds = await dbContext.Bookings
            .Where(booking => booking.ClinicId == clinicId)
            .Join(
                dbContext.AppointmentSlots,
                booking => booking.AppointmentSlotId,
                slot => slot.Id,
                (booking, slot) => new { booking.AppointmentSlotId, slot.StartTime })
            .Where(result => result.StartTime >= dayStart && result.StartTime < dayEnd)
            .Select(result => result.AppointmentSlotId)
            .ToListAsync(cancellationToken);

        var availableSlots = await dbContext.AppointmentSlots
            .AsNoTracking()
            .Where(slot => slot.ClinicId == clinicId
                && slot.IsActive
                && slot.StartTime >= dayStart
                && slot.StartTime < dayEnd)
            .OrderBy(slot => slot.StartTime)
            .ToListAsync(cancellationToken);

        if (bookedSlotIds.Count == 0)
        {
            return availableSlots;
        }

        var bookedSet = bookedSlotIds.ToHashSet();
        return availableSlots.Where(slot => !bookedSet.Contains(slot.Id)).ToList();
    }

    public async Task<Booking> CreateBookingAsync(BookingRequest request, CancellationToken cancellationToken = default)
    {
        var slot = await dbContext.AppointmentSlots
            .Include(slot => slot.Clinic)
            .FirstOrDefaultAsync(slot => slot.Id == request.AppointmentSlotId
                && slot.ClinicId == request.ClinicId
                && slot.IsActive,
                cancellationToken);

        if (slot is null)
        {
            throw new KeyNotFoundException("The requested appointment slot could not be found.");
        }

        if (slot.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment slots in the past cannot be booked.");
        }

        var slotIsBooked = await dbContext.Bookings
            .AnyAsync(booking => booking.AppointmentSlotId == slot.Id, cancellationToken);

        if (slotIsBooked)
        {
            throw new InvalidOperationException("The selected appointment slot has already been booked.");
        }

        var booking = new Booking
        {
            ClinicId = request.ClinicId,
            AppointmentSlotId = request.AppointmentSlotId,
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
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Unable to create booking because the appointment slot is no longer available.", ex);
        }

        await dbContext.Entry(booking).Reference(b => b.AppointmentSlot).LoadAsync(cancellationToken);
        await dbContext.Entry(booking).Reference(b => b.Clinic).LoadAsync(cancellationToken);

        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.Clinic)
            .Include(booking => booking.AppointmentSlot)
            .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken);
    }
}
