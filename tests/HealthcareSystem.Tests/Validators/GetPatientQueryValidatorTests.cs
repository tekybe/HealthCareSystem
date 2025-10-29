using Xunit;
using FluentAssertions;
using FluentValidation;
using HealthcareSystem.Application.Queries;

namespace HealthcareSystem.Tests.Validators;

public class GetPatientQueryValidatorTests
{
    [Fact]
    public async Task Validate_WithValidPatientId_PassesValidation()
    {
        // Arrange
        var validator = new GetPatientQueryValidator();
        var query = new GetPatientQuery(1);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WithInvalidPatientId_FailsValidation(int invalidId)
    {
        // Arrange
        var validator = new GetPatientQueryValidator();
        var query = new GetPatientQuery(invalidId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().PropertyName.Should().Be("PatientId");
    }

    [Fact]
    public async Task Validate_WithPositivePatientId_PassesValidation()
    {
        // Arrange
        var validator = new GetPatientQueryValidator();
        var query = new GetPatientQuery(100);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
