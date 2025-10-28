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
    public async Task<ServiceResult<List<AvailableSlotResponse>>> GetAvailableSlotsAsync(int clinicId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default)
    {
        var clinicExists = await db.Clinics
            .AsNoTracking()
            .AnyAsync(clinic => clinic.Id == clinicId, cancellationToken);

        if (!clinicExists)
            return new ServiceResult<List<AvailableSlotResponse>>(){ Status = ServiceStatus.NotFound,Data = [], Errors = ["Clinic does not exist."] };
        
        var dayStart = startDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        var dayEnd = endDate?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.Today.AddDays(7);
        
        var availableSlots = await db.AppointmentSlots
            .AsNoTracking()
            .Include(s => s.Booking)
            .Where(s => s.ClinicId == clinicId
                && s.IsActive
                && s.StartTime >= dayStart
                && s.StartTime < dayEnd)
            .OrderBy(s => s.StartTime)
            .Select(s => new AvailableSlotResponse(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Booking != null,
                s.Booking != null ? (int?)s.Booking.Id : null))
            .ToListAsync(cancellationToken);

        return new ServiceResult<List<AvailableSlotResponse>> { Status = ServiceStatus.Success, Data = availableSlots};
    }

    public async Task<ServiceResult<BookingDetailsResponse>> CreateBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        var slot = await db.AppointmentSlots
            .Include(slot => slot.Clinic)
            .Include(slot => slot.Booking)
            .FirstOrDefaultAsync(slot => slot.Id == request.AppointmentSlotId
                                    && slot.ClinicId == request.ClinicId
                                    && slot.IsActive, ct);
        if (slot == null)
            return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.NotFound };

        if (slot.Booking != null)
            return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.Conflict };

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

        return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.Success, Data = booking.ToDetailsResponse() };
    }

    public async Task<ServiceResult<BookingDetailsResponse>> GetBookingByIdAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Include(b => b.AppointmentSlot)
                .ThenInclude(a => a.Clinic)
            .Select(b => b.ToDetailsResponse())
            .FirstOrDefaultAsync(ct);

        if (booking == null)
            return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.NotFound };
        
        return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.Success,  Data = booking };
    }

    public async Task<ServiceResult<BookingDetailsResponse>> DeleteBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
            return new ServiceResult<BookingDetailsResponse>() { Status = ServiceStatus.NotFound };

        db.Bookings.Remove(booking);
        await db.SaveChangesAsync(ct);
        
        return new ServiceResult<BookingDetailsResponse>(){Status = ServiceStatus.Success };
    }
}
