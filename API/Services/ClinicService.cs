using API.Data;
using API.Extensions;
using API.Interfaces;
using Contracts.Clinics;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class ClinicService(AppDbContext db) : IClinicService
{
    public async Task<List<ClinicSummaryDto>> GetClinics(CancellationToken cancellationToken)
    {
        var clinics = await db.Clinics
            .AsNoTracking()
            .Select(clinic => clinic.ToDto())
            .ToListAsync(cancellationToken);

        return clinics;
    }
}