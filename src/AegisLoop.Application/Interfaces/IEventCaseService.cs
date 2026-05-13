using AegisLoop.Application.Dtos;

namespace AegisLoop.Application.Interfaces;

public interface IEventCaseService
{
    Task<EventRebuildResultDto> RebuildAsync(CancellationToken cancellationToken = default);
}