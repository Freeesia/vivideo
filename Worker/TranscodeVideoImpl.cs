using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Worker
{
    public class TranscodeVideoImpl : ITranscodeVideo
    {
        private readonly ILogger<TranscodeVideoImpl> logger;
        private readonly string execDir;

        public TranscodeVideoImpl(ILogger<TranscodeVideoImpl> logger)
        {
            this.logger = logger;
            this.execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
        }

        public void Transcode(TranscodeQueue queue)
        {
            var dir = Path.Combine(this.execDir, queue.Output);
            var name = Path.GetFileNameWithoutExtension(queue.Input);
            Directory.CreateDirectory(dir);
            var info = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = this.execDir,
                RedirectStandardError = true,
                ArgumentList = {
                    "-hide_banner",
                    "-v", "warning",
                    "-i", queue.Input.Replace('\\', '/'),
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
            }
            p.BeginErrorReadLine();
            p.WaitForExit();
            this.logger.LogTrace("トランスコード終了:{0}", name);
        }
    }
}
