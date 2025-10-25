using System.Text.Json;
using API.Interfaces;
using Contracts.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BookingsAccess")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("clinics/{clinicId:int}/availability")]
    public async Task<ActionResult<IEnumerable<AvailableSlotResponse>>> GetAvailability(int clinicId, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var availableSlots = await bookingService.GetAvailableSlotsAsync(clinicId, startDate, endDate, cancellationToken);

            return Ok(availableSlots);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                title: "Clinic not found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Slot unavailable",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
        catch(Exception ex)
        {
            NoteException($"api/Booking/clinics/{clinicId}/availability{Request.QueryString.Value}", null, ex);
            return Problem(
                title: "An unexpected error occurred",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult<BookingDetailsResponse>> CreateBooking([FromBody] BookingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var confirmation = await bookingService.CreateBookingAsync(request, cancellationToken);

            return Ok(confirmation);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Slot unavailable",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            NoteException("api/Booking/Create", JsonSerializer.Serialize(request), ex);
            return Problem(
                title: "An unexpected error occurred",
                statusCode: StatusCodes.Status500InternalServerError);
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
            return Problem(
                title: "An unexpected error occurred",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private void NoteException(string url, string? payload, Exception ex)
    {
        // PRETEND FANCY IMPLEMENTATION LIVES HERE.
    }
}
