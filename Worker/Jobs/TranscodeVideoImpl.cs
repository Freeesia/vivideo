using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Model;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace StudioFreesia.Vivideo.Worker.Jobs;

public class TranscodeVideoImpl : ITranscodeVideo
{
    private readonly ILogger<TranscodeVideoImpl> logger;
    private readonly string inputDir;
    private readonly TranscodeSetting transSetting;

    public TranscodeVideoImpl(IOptions<ContentDirSetting> contentOptions, IOptions<TranscodeSetting> transOptions, ILogger<TranscodeVideoImpl> logger)
    {
        this.logger = logger;
        var content = contentOptions.Value;
        this.inputDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        this.transSetting = transOptions.Value;
    }

    public async Task Transcode(TranscodeQueue queue)
    {
        var name = Path.GetFileNameWithoutExtension(queue.Input);
        var input = Path.IsPathRooted(queue.Input) ? queue.Input : Path.Combine(this.inputDir, queue.Input);
        this.logger.LogInformation($"トランスコード開始:{name}");
        var conv = await GetConversion(input.Normalize(), this.transSetting);
        var uri = this.transSetting.OutputHost ?? throw new InvalidOperationException($"{nameof(TranscodeSetting.OutputHost)}が設定されていません");
        conv = conv.SetOutput(new Uri(uri, $"{queue.Output}/master.mpd").ToString());
        this.logger.LogInformation(conv.Build());
        using var progress = Observable.FromEvent<ConversionProgressEventHandler, ConversionProgressEventArgs>(
            h => (s, e) => h(e),
            h => conv.OnProgress += h,
            h => conv.OnProgress -= h)
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(e => this.logger.LogTrace($"Transcoding... {e.Percent:d3}% ({e.Duration}/{e.TotalLength})"));
        try
        {
            await conv.Start();
        }
        catch (Exception e)
        {
            throw new Exception($"「{name}」の出力に失敗しました", e);
        }
        this.logger.LogInformation($"トランスコード終了:{name}");
    }

    private static async Task<IConversion> GetConversion(string input, TranscodeSetting setting)
    {
        var info = await FFmpeg.GetMediaInfo(input);
        var videos = info.VideoStreams.Select(v => v.SetCodec(VideoCodec.h264));
        var audios = info.AudioStreams;
        var conv = FFmpeg.Conversions
            .New()
            .UseShortest(true)
            .UseMultiThread(true)
            .AddStream(videos)
            .AddStream(audios)
            .AddParameter(setting.AdditionalParams)
            .AddParameter("-hls_playlist 1 -http_persistent 1 -movflags +faststart")
            .SetOutputFormat(Format.dash);
        if (!string.IsNullOrEmpty(setting.HWAccel))
        {
            var decoder = videos.First().Codec;
            if (setting.HWDecoders.TryGetValue(decoder, out var hwDecoder))
            {
                decoder = hwDecoder;
            }
            conv = conv.UseHardwareAcceleration(setting.HWAccel, decoder, setting.HWEncoder);
        }
        return conv;
    }
}
