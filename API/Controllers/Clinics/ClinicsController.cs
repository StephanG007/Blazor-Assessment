using API.Data;
using Contracts.Clinics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Clinics;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ClinicSummaryResponse>>> GetClinics(CancellationToken cancellationToken)
    {
        var clinics = await dbContext.Clinics
            .AsNoTracking()
            .Select(clinic => new ClinicSummaryResponse(
                clinic.Id,
                clinic.Name,
                clinic.Address != null ? clinic.Address.City : string.Empty,
                clinic.Address != null ? clinic.Address.Province : string.Empty,
                clinic.PhoneNumber ?? string.Empty))
            .ToListAsync(cancellationToken);

        return clinics;
    }
}
