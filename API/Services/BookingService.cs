using System.Collections.Generic;
using API.Data;
using API.Data.Entities;
using API.Extensions;
using API.Interfaces;
using Bogus.DataSets;
using Contracts.Bookings;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class BookingService(AppDbContext db) : IBookingService
{
    public async Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(int clinicId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default)
    {
        var clinicExists = await db.Clinics
            .AsNoTracking()
            .AnyAsync(clinic => clinic.Id == clinicId, cancellationToken);

        if (!clinicExists)
            return null;
        
        var dayStart = startDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        var dayEnd = endDate?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.Today.AddDays(7);
        
        var availableSlots = await db.AppointmentSlots
            .AsNoTracking()
            .Include(a => a.Booking)
            .Where(slot => slot.ClinicId == clinicId
                && slot.IsActive
                && slot.StartTime >= dayStart
                && slot.StartTime < dayEnd)
            .OrderBy(slot => slot.StartTime)
            .Select(slot => new AvailableSlotResponse(
                slot.Id,
                slot.StartTime,
                slot.EndTime,
                slot.Booking != null,
                slot.Booking != null ? slot.Booking.Id : null))
            .ToListAsync(cancellationToken);

        return availableSlots;
    }

    public async Task<BookingDetailsResponse> CreateBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        var slot = await db.AppointmentSlots
            .Include(slot => slot.Clinic)
            .FirstOrDefaultAsync(slot => slot.Id == request.AppointmentSlotId
                                    && slot.ClinicId == request.ClinicId
                                    && slot.IsActive, ct);
            

        if (slot == null)
            throw new InvalidOperationException($"Appointment slot with id {request.AppointmentSlotId} was not available.");

        var booking = new Booking {
            AppointmentSlotId = request.AppointmentSlotId,
            AppointmentSlot = slot,
            PatientName = request.PatientName,
            PatientEmail = request.PatientEmail,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);
        
        return booking.ToDetailsResponse();
    }

    public async Task<BookingDetailsResponse?> GetBookingByIdAsync(int bookingId, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Include(b => b.AppointmentSlot)
                .ThenInclude(a => a.Clinic)
            .Select(b => b.ToDetailsResponse())
            .FirstOrDefaultAsync(ct);
    }

    public async Task DeleteBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");
        }

        db.Bookings.Remove(booking);
        await db.SaveChangesAsync(ct);
    }
}
