using FluentValidation;
using HealthcareSystem.Domain.Entities;

namespace HealthcareSystem.Domain.Validators;

/// <summary>
/// Domain-level validator for Patient entity business rules.
/// 
/// Note: Currently not used in the existing GET endpoint (which only reads existing data).
/// This validator is prepared for future CRUD operations (Create, Update, Delete) where
/// new Patient entities would be created or modified. It ensures that business rules
/// are enforced at the domain layer, independent of any request/command.
/// 
/// Example future usage:
/// - CreatePatientCommand: Validates new patient data before creating entity
/// - UpdatePatientCommand: Validates modified patient data before persisting changes
/// </summary>
public class PatientValidator : AbstractValidator<Patient>
{
    public PatientValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0");

        RuleFor(p => p.NHSNumber)
            .NotEmpty()
            .WithMessage("NHS Number is required")
            .Length(10)
            .WithMessage("NHS Number must be exactly 10 digits long")
            .Matches(@"^\d+$")
            .WithMessage("NHS Number must contain only digits");

        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Patient name is required")
            .MaximumLength(200)
            .WithMessage("Patient name cannot exceed 200 characters");

        RuleFor(p => p.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .WithMessage("Date of birth must be in the past");

        RuleFor(p => p.GPPractice)
            .NotEmpty()
            .WithMessage("GP Practice is required")
            .MaximumLength(200)
            .WithMessage("GP Practice name cannot exceed 200 characters");
    }
}
