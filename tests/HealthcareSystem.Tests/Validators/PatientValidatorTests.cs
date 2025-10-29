using Xunit;
using FluentAssertions;
using HealthcareSystem.Domain.Entities;
using HealthcareSystem.Domain.Validators;

namespace HealthcareSystem.Tests.Validators;

public class PatientValidatorTests
{
    [Fact]
    public async Task Validate_WithValidPatient_PassesValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    public async Task Validate_WithInvalidNHSNumber_FailsValidation(string invalidNHSNumber)
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = invalidNHSNumber,
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NHSNumber");
    }

    [Fact]
    public async Task Validate_WithNHSNumberContainingLetters_FailsValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "123456789A",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithValidNHSNumber_PassesValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_FailsValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithFutureDateOfBirth_FailsValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = DateTime.UtcNow.AddDays(1),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithInvalidPatientId_FailsValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patient = new Patient
        {
            Id = 0,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        // Act
        var result = await validator.ValidateAsync(patient);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithValidMultiplePatients_AllPassValidation()
    {
        // Arrange
        var validator = new PatientValidator();
        var patients = new List<Patient>
        {
            new Patient { Id = 1, NHSNumber = "1111111111", Name = "Patient One", DateOfBirth = new DateTime(1980, 01, 01), GPPractice = "Practice A" },
            new Patient { Id = 2, NHSNumber = "2222222222", Name = "Patient Two", DateOfBirth = new DateTime(1990, 01, 01), GPPractice = "Practice B" },
            new Patient { Id = 3, NHSNumber = "3333333333", Name = "Patient Three", DateOfBirth = new DateTime(2000, 01, 01), GPPractice = "Practice C" }
        };

        // Act & Assert
        foreach (var patient in patients)
        {
            var result = await validator.ValidateAsync(patient);
            result.IsValid.Should().BeTrue();
        }
    }
}
