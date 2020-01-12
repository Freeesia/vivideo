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

        public Task<string> Transcode(string path)
            => Task.Run(() =>
            {
                var hash = HashCode.Combine(path).ToString();
                var dir = Path.Combine(this.execDir, hash);
                Directory.CreateDirectory(dir);
                var info = new ProcessStartInfo(@"ffmpeg\bin\ffmpeg.exe")
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    WorkingDirectory = this.execDir,
                    // RedirectStandardError = true,
                    // RedirectStandardOutput = true,
                    // Arguments = $"-i {path} -c:v nvenc_h264 -q:v 20 -c:a copy -window_size 0 -hls_playlist 1 -movflags +faststart out/video.mpd",
                    ArgumentList = {
                        "-i", path.Replace('\\', '/'),
                        "-c:v", "h264_nvenc",
                        "-c:a", "copy",
                        "-window_size", "0",
                        "-hls_playlist", "1",
                        "-movflags", "+faststart",
                        Path.Combine(dir, "video.mpd").Replace('\\', '/'),
                    }
                };
                using var p = Process.Start(info);
                // p.OutputDataReceived += (s, e) => this.logger.LogTrace(e.Data);
                // p.ErrorDataReceived += (s, e) => this.logger.LogError(e.Data);
                // p.BeginErrorReadLine();
                // p.BeginOutputReadLine();
                p.WaitForExit();
                return string.Empty;
            });
    }
}
