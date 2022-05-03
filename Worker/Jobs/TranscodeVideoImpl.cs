using Microsoft.Extensions.Options;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Model;
using Xabe.FFmpeg;

namespace StudioFreesia.Vivideo.Worker.Jobs;

public class TranscodeVideoImpl : ITranscodeVideo
{
    private readonly ILogger<TranscodeVideoImpl> logger;
    private readonly string workDir;
    private readonly string inputDir;
    private readonly TranscodeSetting transSetting;

    public TranscodeVideoImpl(IOptions<ContentDirSetting> contentOptions, IOptions<TranscodeSetting> transOptions, ILogger<TranscodeVideoImpl> logger)
    {
        this.logger = logger;
        var content = contentOptions.Value;
        this.workDir = content?.Work ?? throw new ArgumentException(nameof(content.Work));
        this.inputDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        this.transSetting = transOptions.Value;
    }

    public async Task Transcode(TranscodeQueue queue)
    {
        var dir = Path.Combine(this.workDir, queue.Output);
        var outPath = Path.Combine(dir, "master.mpd");
        var name = Path.GetFileNameWithoutExtension(queue.Input);
        if (File.Exists(outPath))
        {
            this.logger.LogInformation($"トランスコードスキップ: {name}");
            return;
        }
        Directory.CreateDirectory(dir);
        var input = Path.IsPathRooted(queue.Input) ? queue.Input : Path.Combine(this.inputDir, queue.Input);
        this.logger.LogInformation($"トランスコード開始:{name}");
        var conv = await GetConversion(input, this.transSetting);
        conv = conv.SetOutput(outPath);
        // conv.OnDataReceived += (s, e) => this.logger.LogTrace(e.Data);
        conv.OnProgress += (s, e) => this.logger.LogTrace($"Transcoding... {e.Percent}%");
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
            .AddParameter("-window_size 0 -hls_playlist 1 -movflags +faststart");
        if (!string.IsNullOrEmpty(setting.HWAccel))
        {
            conv = conv.UseHardwareAcceleration(setting.HWAccel, videos.First().Codec, setting.HWEncoder);
        }
        return conv;
    }
}
