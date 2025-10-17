using API.Controllers.Booking.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Booking;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("clinics")]
    public async Task<ActionResult<IEnumerable<ClinicSummaryDto>>> GetClinics(CancellationToken cancellationToken)
    {
        var clinics = await bookingService.GetClinicsAsync(cancellationToken);

        var response = clinics
            .Select(clinic => new ClinicSummaryDto(
                clinic.Id,
                clinic.Name,
                clinic.Address,
                clinic.PhoneNumber));

        return Ok(response);
    }

    [HttpGet("clinics/{clinicId:int}/availability")]
    public async Task<ActionResult<IEnumerable<AvailableSlotDto>>> GetAvailability(int clinicId, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        try
        {
            var slots = await bookingService.GetAvailableSlotsAsync(clinicId, targetDate, cancellationToken);

            var response = slots.Select(slot => new AvailableSlotDto(slot.Id, slot.StartTime, slot.EndTime));
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookingConfirmationDto>> CreateBooking([FromBody] BookingRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var booking = await bookingService.CreateBookingAsync(
                new BookingRequest(
                    request.ClinicId,
                    request.AppointmentSlotId,
                    request.PatientName,
                    request.PatientEmail,
                    request.Notes),
                cancellationToken);

            var confirmation = new BookingConfirmationDto(
                booking.Id,
                booking.Clinic!.Name,
                booking.AppointmentSlot!.StartTime,
                booking.AppointmentSlot.EndTime,
                booking.PatientName,
                booking.PatientEmail,
                booking.Notes);

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, confirmation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDetailsDto>> GetBookingById(int id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetBookingByIdAsync(id, cancellationToken);

        if (booking is null)
        {
            return NotFound();
        }

        var details = new BookingDetailsDto(
            booking.Id,
            booking.ClinicId,
            booking.Clinic!.Name,
            booking.AppointmentSlot!.StartTime,
            booking.AppointmentSlot.EndTime,
            booking.PatientName,
            booking.PatientEmail,
            booking.Notes,
            booking.CreatedAt);

        return Ok(details);
    }
}
