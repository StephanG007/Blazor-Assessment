using System;
using API.Interfaces;
using Contracts.Clinics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController(IClinicService clinicService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ClinicSummaryResponse>> GetClinics(CancellationToken cancellationToken)
    {
        try
        {
            var clinics = await clinicService.GetClinics(cancellationToken);

            return Ok(new ClinicSummaryResponse { Success = true, Clinics = clinics });
        }
        catch (Exception ex)
        {
            NoteException("api/Clinics/GetClinics", null, ex);
            return Problem(title: "Unable to load clinics", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private void NoteException(string url, string? payload, Exception? ex)
    {
        // PRETEND FANCY IMPLEMENTATION 
    }
}

