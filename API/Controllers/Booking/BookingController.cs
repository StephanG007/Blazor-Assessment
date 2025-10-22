using System.Text.Json;
using API.Interfaces;
using Contracts.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Booking;


[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdminRole")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("clinics/{clinicId:int}/availability")]
    public async Task<ActionResult<IEnumerable<AvailableSlotResponse>>> GetAvailability(int clinicId, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now.Date);

        try
        {
            var availableSlots = await bookingService.GetAvailableSlotsAsync(clinicId, targetDate, cancellationToken);
            
            return Ok(availableSlots);
        }
        catch(Exception ex)
        {
            NoteException($"api/Booking/clinics/{clinicId}/availability{Request.QueryString.Value}", null, ex);
            return BadRequest();
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult<BookingConfirmationResponse>> CreateBooking([FromBody] BookingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            await bookingService.CreateBookingAsync(request, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            NoteException("api/Booking/Create", JsonSerializer.Serialize(request), ex);
            return BadRequest();
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDetailsResponse>> GetBookingById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await bookingService.GetBookingByIdAsync(id, cancellationToken);

            if (booking == null) return NotFound();

            return Ok(booking);
        }
        catch(Exception ex)
        {
            NoteException($"api/Booking/{id}", null, ex);
            return BadRequest();
        }
    }

    private void NoteException(string url, string? payload, Exception ex)
    {
        // PRETEND FANCY IMPLEMENTATION LIVES HERE.
    }
}
