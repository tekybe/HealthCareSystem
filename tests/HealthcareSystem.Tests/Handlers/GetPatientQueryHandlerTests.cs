using Xunit;
using FluentAssertions;
using Moq;
using Serilog;
using HealthcareSystem.Application.Repositories;
using HealthcareSystem.Application.Queries;
using HealthcareSystem.Domain.Entities;

namespace HealthcareSystem.Tests.Handlers;

public class GetPatientQueryHandlerTests
{
    private readonly Mock<ILogger> _mockLogger = new();

    [Fact]
    public async Task Handle_WithValidPatientId_ReturnsPatientSuccessfully()
    {
        // Arrange
        var patientId = 1;
        var patient = new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        };

        var mockRepository = new Mock<IPatientRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(patientId))
            .ReturnsAsync(patient);

        var handler = new GetPatientQueryHandler(mockRepository.Object, _mockLogger.Object);
        var query = new GetPatientQuery(patientId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data?.Name.Should().Be("John Smith");
        result.Data?.NHSNumber.Should().Be("1234567890");
        mockRepository.Verify(r => r.GetByIdAsync(patientId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidPatientId_ReturnsNotFoundError()
    {
        // Arrange
        var patientId = 999;
        var mockRepository = new Mock<IPatientRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(patientId))
            .ReturnsAsync((Patient?)null);

        var handler = new GetPatientQueryHandler(mockRepository.Object, _mockLogger.Object);
        var query = new GetPatientQuery(patientId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("not found");
        mockRepository.Verify(r => r.GetByIdAsync(patientId), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var patientId = 5;
        var mockRepository = new Mock<IPatientRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Patient?)null);

        var handler = new GetPatientQueryHandler(mockRepository.Object, _mockLogger.Object);
        var query = new GetPatientQuery(patientId);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.GetByIdAsync(patientId), Times.Once);
    }
}
