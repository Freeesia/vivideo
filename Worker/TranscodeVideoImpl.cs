using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Worker
{
    public class TranscodeVideoImpl : ITranscodeVideo
    {
        private readonly ILogger<TranscodeVideoImpl> logger;
        private readonly string workDir;
        private readonly string inputDir;
        private readonly string execDir;

        public TranscodeVideoImpl(IConfiguration config, ILogger<TranscodeVideoImpl> logger)
        {
            this.logger = logger;
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.workDir = content?.Work ?? throw new ArgumentException();
            this.inputDir = content?.List ?? throw new ArgumentException();
            this.execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException();
        }

        public void Transcode(TranscodeQueue queue)
        {
            var dir = Path.Combine(this.workDir, queue.Output);
            var name = Path.GetFileNameWithoutExtension(queue.Input);
            Directory.CreateDirectory(dir);
            var input = Path.IsPathRooted(queue.Input) ? queue.Input : Path.Combine(this.inputDir, queue.Input);
            var info = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = this.execDir,
                RedirectStandardError = true,
                ArgumentList = {
                    "-hide_banner",
                    "-v", "warning",
                    "-i", input.Replace('\\', '/'),
                    "-c:v", "h264_nvenc",
                    "-c:a", "copy",
                    "-window_size", "0",
                    "-hls_playlist", "1",
                    "-movflags", "+faststart",
                    Path.Combine(dir, "master.mpd").Replace('\\', '/'),
                }
            };
            this.logger.LogTrace("トランスコード開始:{0}", name);
            using var p = Process.Start(info);
            p.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    this.logger.LogError(e.Data);
                }
            };
            p.BeginErrorReadLine();
            p.WaitForExit();
            this.logger.LogTrace("トランスコード終了:{0}", name);
        }
    }
}
