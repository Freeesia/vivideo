using Hangfire;

namespace StudioFreesia.Vivideo.Core;

public interface ITranscodeVideo
{
    [AutomaticRetry(Attempts = 0)]
    Task Transcode(TranscodeQueue queue);
}
