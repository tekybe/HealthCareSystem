using HealthcareSystem.Domain.Entities;

namespace HealthcareSystem.Application.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
}
