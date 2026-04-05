using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Domain.Common.Handlers;

/// <summary>Handles a query of type <typeparamref name="TQuery"/> returning <typeparamref name="TResult"/>.</summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>Executes the query asynchronously and returns the result.</summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
