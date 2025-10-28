using System;
using System.Collections.Generic;
using API.Data;
using API.Data.Entities;
using API.Extensions;
using API.Interfaces;
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
        {
            return ServiceResult<List<AvailableSlotResponse>>.NotFound("Clinic does not exist.");
        }

        var dayStart = startDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        var dayEnd = endDate?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.Today.AddDays(7);

        var availableSlots = await db.AppointmentSlots
            .AsNoTracking()
            .Include(slot => slot.Booking)
            .Where(slot => slot.ClinicId == clinicId
                && slot.IsActive
                && slot.StartTime >= dayStart
                && slot.StartTime < dayEnd)
            .OrderBy(slot => slot.StartTime)
            .Select(DtoConversions.ToSlotResponse)
            .ToListAsync(cancellationToken);

        return ServiceResult<List<AvailableSlotResponse>>.Success(availableSlots);
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
        {
            return ServiceResult<BookingDetailsResponse>.NotFound();
        }

        if (slot.Booking != null)
        {
            return ServiceResult<BookingDetailsResponse>.Conflict();
        }

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

        return ServiceResult<BookingDetailsResponse>.Success(booking.ToDetailsResponse());
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
        {
            return ServiceResult<BookingDetailsResponse>.NotFound();
        }

        return ServiceResult<BookingDetailsResponse>.Success(booking);
    }

    public async Task<ServiceResult<BookingDetailsResponse>> DeleteBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
        {
            return ServiceResult<BookingDetailsResponse>.NotFound();
        }

        db.Bookings.Remove(booking);
        await db.SaveChangesAsync(ct);

        return ServiceResult<BookingDetailsResponse>.Success();
    }
}

