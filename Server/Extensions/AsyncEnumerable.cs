using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace StudioFreesia.Vivideo.Server.Extensions;

public static class AsyncEnumerable
{
    public static async IAsyncEnumerable<TResult> WhenEach<TResult>(this Task<TResult>[] tasks, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (tasks == null) throw new ArgumentNullException(nameof(tasks));
        var channel = Channel.CreateUnbounded<Task<TResult>>();
        using var completionCts = new CancellationTokenSource();
        var continuations = new List<Task>(tasks.Length);
        try
        {
            int pendingCount = tasks.Length;
            foreach (var task in tasks)
            {
                if (task == null) throw new ArgumentException(
                    $"The tasks argument included a null value.", nameof(tasks));
                continuations.Add(task.ContinueWith(t =>
                {
                    bool accepted = channel.Writer.TryWrite(t);
                    if (Interlocked.Decrement(ref pendingCount) == 0)
                        channel.Writer.Complete();
                }, completionCts.Token, TaskContinuationOptions.ExecuteSynchronously |
                    TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default));
            }

            await foreach (var task in channel.Reader.ReadAllAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                yield return await task.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        finally
        {
            completionCts.Cancel();
            try { await Task.WhenAll(continuations).ConfigureAwait(false); }
            catch (OperationCanceledException) { } // Ignore
        }
    }
}
