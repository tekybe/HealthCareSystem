using FluentValidation;
using MediatR;
using Serilog;
using HealthcareSystem.Application.Queries;
using HealthcareSystem.Domain.Results;
using ILogger = Serilog.ILogger;

namespace HealthcareSystem.API.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.Information("Validating request: {RequestName}", requestName);

        if (!_validators.Any())
        {
            _logger.Debug("No validators found for {RequestName}", requestName);
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            _logger.Warning("Validation failed for {RequestName}. Errors: {@Errors}", requestName, failures.Select(f => f.ErrorMessage));
            throw new ValidationException(failures);
        }

        _logger.Information("Validation passed for {RequestName}", requestName);
        return await next();
    }
}