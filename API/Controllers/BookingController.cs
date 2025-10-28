using System;
using System.Collections.Generic;
using System.Linq;
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
            var result = await bookingService.GetAvailableSlotsAsync(clinicId, startDate, endDate, cancellationToken);

            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleUnexpected($"api/Booking/clinics/{clinicId}/availability{Request.QueryString.Value}", null, ex);
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

            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleUnexpected("api/Booking/Create", JsonSerializer.Serialize(request), ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDetailsResponse>> GetBookingById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await bookingService.GetBookingByIdAsync(id, cancellationToken);

            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleUnexpected($"api/Booking/{id}", null, ex);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await bookingService.DeleteBookingAsync(id, cancellationToken);

            return HandleResult(result, _ => NoContent());
        }
        catch (Exception ex)
        {
            return HandleUnexpected($"api/Booking/{id}", null, ex);
        }
    }

    private const string UnexpectedErrorTitle = "An unexpected error occurred";

    private IActionResult HandleResult<T>(ServiceResult<T> result, Func<T?, IActionResult>? onSuccess = null) =>
        result.Status switch
        {
            ServiceStatus.Success => onSuccess?.Invoke(result.Data) ?? Ok(result.Data),
            ServiceStatus.NotFound => result.Errors.Any() ? NotFound(result.Errors) : NotFound(),
            ServiceStatus.Conflict => result.Errors.Any() ? Conflict(result.Errors) : Conflict(),
            ServiceStatus.Unauthorized => result.Errors.Any() ? Unauthorized(result.Errors) : Unauthorized(),
            _ => Problem(title: UnexpectedErrorTitle, statusCode: StatusCodes.Status500InternalServerError)
        };

    private ObjectResult HandleUnexpected(string url, string? payload, Exception ex)
    {
        NoteException(url, payload, ex);
        return Problem(title: UnexpectedErrorTitle, statusCode: StatusCodes.Status500InternalServerError);
    }

    private void NoteException(string url, string? payload, Exception ex)
    {
        // PRETEND FANCY IMPLEMENTATION LIVES HERE.
    }
}

