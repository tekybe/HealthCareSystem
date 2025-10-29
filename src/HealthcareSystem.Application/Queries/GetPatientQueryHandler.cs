using MediatR;
using Serilog;
using HealthcareSystem.Application.Repositories;
using HealthcareSystem.Domain.Results;
using ILogger = Serilog.ILogger;

namespace HealthcareSystem.Application.Queries;

public class GetPatientQueryHandler : IRequestHandler<GetPatientQuery, Result<GetPatientResponse>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger _logger;

    public GetPatientQueryHandler(IPatientRepository patientRepository, ILogger logger)
    {
        _patientRepository = patientRepository;
        _logger = logger;
    }

    public async Task<Result<GetPatientResponse>> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Processing GetPatientQuery for PatientId: {PatientId}", request.PatientId);

        var patient = await _patientRepository.GetByIdAsync(request.PatientId);

        if (patient is null)
        {
            _logger.Warning("Patient not found. PatientId: {PatientId}", request.PatientId);
            return Result<GetPatientResponse>.Failure($"Patient with ID {request.PatientId} not found.");
        }

        _logger.Information("Patient found. PatientId: {PatientId}, NHSNumber: {NHSNumber}", request.PatientId, patient.NHSNumber);

        var response = new GetPatientResponse
        {
            NHSNumber = patient.NHSNumber,
            Name = patient.Name,
            DateOfBirth = patient.DateOfBirth,
            GPPractice = patient.GPPractice
        };

        return Result<GetPatientResponse>.Success(response);
    }
}
