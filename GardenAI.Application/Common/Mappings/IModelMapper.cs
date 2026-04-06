namespace GardenAI.Application.Common.Mappings;

/// <summary>Abstraction for mapping between domain models and presentation contracts.</summary>
public interface IModelMapper
{
    /// <summary>Maps a source object to a target type.</summary>
    TTarget Map<TSource, TTarget>(TSource source)
        where TSource : class
        where TTarget : class;

    /// <summary>Maps a collection of source objects to target types.</summary>
    IReadOnlyList<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> sources)
        where TSource : class
        where TTarget : class;
}

