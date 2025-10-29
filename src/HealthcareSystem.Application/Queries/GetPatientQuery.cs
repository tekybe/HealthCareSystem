using MediatR;
using HealthcareSystem.Domain.Results;

namespace HealthcareSystem.Application.Queries;

public record GetPatientQuery(int PatientId) : IRequest<Result<GetPatientResponse>>;

public class GetPatientResponse
{
    public string NHSNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string GPPractice { get; set; } = string.Empty;
}