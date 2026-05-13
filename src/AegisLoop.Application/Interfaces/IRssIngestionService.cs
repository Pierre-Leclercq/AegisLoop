using AegisLoop.Application.Dtos;

namespace AegisLoop.Application.Interfaces;

public interface IIngestionService
{
    Task<IngestionResponse> RunAsync(IngestionRequest request, CancellationToken cancellationToken = default);
}
