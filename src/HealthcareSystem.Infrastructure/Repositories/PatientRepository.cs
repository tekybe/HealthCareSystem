using Serilog;
using HealthcareSystem.Application.Repositories;
using HealthcareSystem.Domain.Entities;
using ILogger = Serilog.ILogger;

namespace HealthcareSystem.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ILogger _logger;

    private static readonly List<Patient> Patients =
    [
        new Patient
        {
            Id = 1,
            NHSNumber = "1234567890",
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 05, 15),
            GPPractice = "Central Medical Practice"
        },
        new Patient
        {
            Id = 2,
            NHSNumber = "0987654321",
            Name = "Jane Doe",
            DateOfBirth = new DateTime(1975, 08, 22),
            GPPractice = "Westside Health Centre"
        },
        new Patient
        {
            Id = 3,
            NHSNumber = "5678901234",
            Name = "Michael Johnson",
            DateOfBirth = new DateTime(1990, 12, 03),
            GPPractice = "North Street Surgery"
        },
        new Patient
        {
            Id = 4,
            NHSNumber = "1122334455",
            Name = "Sarah Williams",
            DateOfBirth = new DateTime(1985, 03, 27),
            GPPractice = "Springfield Medical Clinic"
        },
        new Patient
        {
            Id = 5,
            NHSNumber = "5544332211",
            Name = "David Brown",
            DateOfBirth = new DateTime(1972, 11, 10),
            GPPractice = "Riverside Health Practice"
        }
    ];

    public PatientRepository()
    {
        _logger = Log.ForContext<PatientRepository>();
    }

    public Task<Patient?> GetByIdAsync(int id)
    {
        _logger.Debug("Querying patient repository for Id: {PatientId}", id);
        var patient = Patients.FirstOrDefault(p => p.Id == id);
        
        if (patient is not null)
        {
            _logger.Debug("Patient record found in repository. Id: {PatientId}", id);
        }
        else
        {
            _logger.Debug("No patient record found in repository. Id: {PatientId}", id);
        }

        return Task.FromResult(patient);
    }
}
