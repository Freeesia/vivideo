using System.Reactive.Linq;

namespace StudioFreesia.Vivideo.Worker.Extensions;

public static class ObservableExtensions
{
    public static IObservable<(T? previous, T current)> WithPrevious<T>(this IObservable<T> source)
        => source.Scan((previous: default(T), current: default(T)!), (previous, current) => (previous.current, current));
}
