using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using HealthcareSystem.Application.Queries;
using ILogger = Serilog.ILogger;

namespace HealthcareSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public PatientsController(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        _logger.Information("HTTP GET request received for Patient. PatientId: {PatientId}", id);
        
        var query = new GetPatientQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.Warning("Patient retrieval failed. PatientId: {PatientId}, Error: {ErrorMessage}", id, result.ErrorMessage);
            return NotFound(new { message = result.ErrorMessage });
        }

        _logger.Information("Patient retrieved successfully. PatientId: {PatientId}", id);
        return Ok(result.Data);
    }
}