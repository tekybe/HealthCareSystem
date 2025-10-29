using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using HealthcareSystem.Application.Queries;
using HealthcareSystem.IntegrationTests.Fixtures;

namespace HealthcareSystem.IntegrationTests.Controllers;

public class PatientsControllerIntegrationTests : IClassFixture<HealthcareSystemWebApplicationFactory>
{
    private readonly HealthcareSystemWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PatientsControllerIntegrationTests(HealthcareSystemWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPatient_WithValidId_ReturnsOkWithPatientData()
    {
        // Arrange
        var patientId = 1;
        var endpoint = $"/api/patients/{patientId}";

        // Act
        var response = await _client.GetAsync(endpoint);
        var content = await response.Content.ReadFromJsonAsync<GetPatientResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
        content!.Name.Should().Be("John Smith");
        content.NHSNumber.Should().Be("1234567890");
    }

    [Fact]
    public async Task GetPatient_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var patientId = 999;
        var endpoint = $"/api/patients/{patientId}";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, "John Smith", "1234567890")]
    [InlineData(2, "Jane Doe", "0987654321")]
    [InlineData(3, "Michael Johnson", "5678901234")]
    public async Task GetPatient_WithValidIds_ReturnsCorrectPatientData(int id, string expectedName, string expectedNHSNumber)
    {
        // Arrange
        var endpoint = $"/api/patients/{id}";

        // Act
        var response = await _client.GetAsync(endpoint);
        var content = await response.Content.ReadFromJsonAsync<GetPatientResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content?.Name.Should().Be(expectedName);
        content?.NHSNumber.Should().Be(expectedNHSNumber);
    }

    [Fact]
    public async Task GetPatient_ReturnsContentTypeApplicationJson()
    {
        // Arrange
        var patientId = 1;
        var endpoint = $"/api/patients/{patientId}";

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetPatient_NotFoundResponse_ContainsErrorMessage()
    {
        // Arrange
        var patientId = 999;
        var endpoint = $"/api/patients/{patientId}";

        // Act
        var response = await _client.GetAsync(endpoint);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        content?.Message.Should().NotBeNullOrEmpty();
        content?.Message.Should().Contain("not found");
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
