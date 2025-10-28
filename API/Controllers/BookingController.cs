using System.Collections.Generic;
using System.Text.Json;
using API.Interfaces;
using Contracts.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var result = await bookingService.GetAvailableSlotsAsync(clinicId, startDate, endDate, cancellationToken);

            if(result.Status == ServiceStatus.NotFound)
                return NotFound();

            return Ok(result.Data);
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
            var result = await bookingService.CreateBookingAsync(request, cancellationToken);

            switch (result.Status)
            {
                case ServiceStatus.Success:
                    return Ok(result.Data);
                case ServiceStatus.Conflict:
                    return Conflict(result.Data);
                case ServiceStatus.NotFound:
                    return NotFound();
                default:
                    throw new Exception($"Unexpected Service Status: {result.Status}");
            }
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
            var result = await bookingService.GetBookingByIdAsync(id, cancellationToken);

            switch (result.Status)
            {
                case ServiceStatus.Success:
                    return Ok(result.Data);
                case ServiceStatus.NotFound:
                    return NotFound();
                case ServiceStatus.Conflict:
                    return Conflict(result.Data);
                default:
                    throw new Exception($"Unexpected Service Status: {result.Status}");
            }
        }
        catch(Exception ex)
        {
            NoteException($"api/Booking/{id}", null, ex);
            return Problem(
                title: "An unexpected error occurred",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await bookingService.DeleteBookingAsync(id, cancellationToken);
            
            if(result.Status != ServiceStatus.Success)
                throw new Exception("An unexpected error occurred");
            
            return Ok("Successfully Deleted");
        }
        catch (Exception ex)
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
