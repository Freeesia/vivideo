using System.Diagnostics;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Model;

namespace StudioFreesia.Vivideo.Worker.Jobs;

public class TranscodeVideoImpl : ITranscodeVideo
{
    private readonly ILogger<TranscodeVideoImpl> logger;
    private readonly string workDir;
    private readonly string inputDir;
    private readonly string rootDir;

    private readonly string ffmpeg;
    private readonly string ffprobe;
    private readonly Dictionary<string, string> args;

    public TranscodeVideoImpl(IOptions<ContentDirSetting> contentOptions, IOptions<TranscodeSetting> transOptions, IHostEnvironment env, ILogger<TranscodeVideoImpl> logger)
    {
        this.logger = logger;
        var content = contentOptions.Value;
        this.workDir = content?.Work ?? throw new ArgumentException(nameof(content.Work));
        this.inputDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        var trans = transOptions.Value;
        this.ffmpeg = trans.Ffmpeg ?? throw new ArgumentException(nameof(trans.Ffmpeg));
        this.ffprobe = trans.Ffprobe ?? throw new ArgumentException(nameof(trans.Ffprobe));
        this.args = trans.Args ?? throw new ArgumentException(nameof(trans.Args));
        this.rootDir = env.ContentRootPath;
    }

    public void Transcode(TranscodeQueue queue)
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
        RunProcess(this.ffmpeg, string.Format(GetArgs(input), input.Replace('\\', '/'), outPath.Replace('\\', '/')));
        if (!File.Exists(outPath))
        {
            throw new Exception($"「{name}」の出力に失敗しました");
        }
        var tmp = outPath + ".tmp";
        while (File.Exists(tmp))
        {
            try
            {
                File.Move(tmp, outPath, true);
            }
            catch (IOException)
            {
                //  IOException が発生したらもう一度
                Thread.Sleep(100);
            }
        }
        this.logger.LogInformation($"トランスコード終了:{name}");
    }

    private string GetArgs(string input)
    {
        var json = RunProcess(this.ffprobe, $"-hide_banner -v warning -of json -show_streams \"{input.Replace('\\', '/')}\"");

        var info = JObject.Parse(json);

        var codec = info.SelectToken("$.streams[?(@.codec_type == 'video')].codec_name")?.Value<string>().ToUpper();

        if (!string.IsNullOrEmpty(codec) && this.args.TryGetValue(codec, out var args) || this.args.TryGetValue("DEFAULT", out args))
        {
            return args;
        }
        else
        {
            throw new InvalidOperationException("デフォルト用のffmpegの設定が存在しません");
        }
    }

    private string RunProcess(string file, string args)
    {
        var info = new ProcessStartInfo(file)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = this.rootDir,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = args,
        };
        using var p = Process.Start(info);
        p.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    this.logger.LogWarning(e.Data);
                }
            };
        p.BeginErrorReadLine();
        p.WaitForExit();
        return p.StandardOutput.ReadToEnd();
    }
}
