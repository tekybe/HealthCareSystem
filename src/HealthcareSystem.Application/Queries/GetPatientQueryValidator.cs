using FluentValidation;

namespace HealthcareSystem.Application.Queries;

public class GetPatientQueryValidator : AbstractValidator<GetPatientQuery>
{
    public GetPatientQueryValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0");
    }
}
