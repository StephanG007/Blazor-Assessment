using Contracts.Clinics;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces;

public interface IClinicService
{
    public Task<List<ClinicDto>> GetClinics(CancellationToken cancellationToken);
}