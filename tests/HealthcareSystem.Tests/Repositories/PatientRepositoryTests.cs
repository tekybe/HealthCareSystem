using Xunit;
using FluentAssertions;
using HealthcareSystem.Infrastructure.Repositories;

namespace HealthcareSystem.Tests.Repositories;

public class PatientRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsPatient()
    {
        // Arrange
        var repository = new PatientRepository();
        var patientId = 1;

        // Act
        var patient = await repository.GetByIdAsync(patientId);

        // Assert
        patient.Should().NotBeNull();
        patient?.Id.Should().Be(1);
        patient?.Name.Should().Be("John Smith");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var repository = new PatientRepository();
        var patientId = 999;

        // Act
        var patient = await repository.GetByIdAsync(patientId);

        // Assert
        patient.Should().BeNull();
    }

    [Theory]
    [InlineData(1, "1234567890")]
    [InlineData(2, "0987654321")]
    [InlineData(3, "5678901234")]
    public async Task GetByIdAsync_WithValidIds_ReturnsCorrectNHSNumber(int id, string expectedNHSNumber)
    {
        // Arrange
        var repository = new PatientRepository();

        // Act
        var patient = await repository.GetByIdAsync(id);

        // Assert
        patient.Should().NotBeNull();
        patient?.NHSNumber.Should().Be(expectedNHSNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WithMultiplePatients_ReturnsCorrectPatient()
    {
        // Arrange
        var repository = new PatientRepository();

        // Act
        var patient1 = await repository.GetByIdAsync(1);
        var patient2 = await repository.GetByIdAsync(2);
        var patient3 = await repository.GetByIdAsync(3);

        // Assert
        patient1?.Name.Should().Be("John Smith");
        patient2?.Name.Should().Be("Jane Doe");
        patient3?.Name.Should().Be("Michael Johnson");
    }
}
