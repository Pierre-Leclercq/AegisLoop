using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Service de feedback analyste V1 — Confirmer/Invalider/Corriger.
/// Le feedback est immutable après la fenêtre d'annulation (5 min).
/// </summary>
public interface IAnalystFeedbackService
{
    Task<AnalystFeedback> SubmitAsync(Guid targetId, FeedbackAction action, string? details);
    Task<bool> CancelAsync(Guid feedbackId);
    Task<IReadOnlyList<AnalystFeedback>> GetHistoryAsync(Guid targetId);
}