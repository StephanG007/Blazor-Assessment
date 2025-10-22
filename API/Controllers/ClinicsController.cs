using API.Data;
using API.Extensions;
using API.Interfaces;
using Contracts.Clinics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Clinics;

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

            return new ClinicSummaryResponse { Success = true, Clinics = clinics };
        }
        catch (Exception ex)
        {
            NoteException("api/Clinics/GetClinics", null, ex);
            return BadRequest();
        }
    }

    private void NoteException(string url, string? payload, Exception? ex)
    {
        // PRETEND FANCY IMPLEMENTATION 
    }
}
