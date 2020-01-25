using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Model;

namespace StudioFreesia.Vivideo.Worker
{
    public class TranscodeVideoImpl : ITranscodeVideo
    {
        private readonly ILogger<TranscodeVideoImpl> logger;
        private readonly string workDir;
        private readonly string inputDir;
        private readonly string rootDir;

        private readonly string file;
        private readonly string args;

        public TranscodeVideoImpl(IConfiguration config, IHostEnvironment env, ILogger<TranscodeVideoImpl> logger)
        {
            this.logger = logger;
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.workDir = content?.Work ?? throw new ArgumentException();
            this.inputDir = content?.List ?? throw new ArgumentException();
            var trans = config.GetSection("Transcode").Get<TranscodeSetting>();
            this.file = trans.File ?? throw new ArgumentException();
            this.args = trans.Args ?? throw new ArgumentException();
            this.rootDir = env.ContentRootPath;
        }

        public void Transcode(TranscodeQueue queue)
        {
            var dir = Path.Combine(this.workDir, queue.Output);
            var name = Path.GetFileNameWithoutExtension(queue.Input);
            if (Directory.Exists(dir))
            {
                this.logger.LogInformation("トランスコードスキップ: {0}", name);
                return;
            }
            Directory.CreateDirectory(dir);
            var input = Path.IsPathRooted(queue.Input) ? queue.Input : Path.Combine(this.inputDir, queue.Input);
            var info = new ProcessStartInfo(this.file)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = this.rootDir,
                RedirectStandardError = true,
                Arguments = string.Format(this.args, input.Replace('\\', '/'), Path.Combine(dir, "master.mpd").Replace('\\', '/')),
            };
            this.logger.LogInformation("トランスコード開始:{0}", name);
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
            this.logger.LogInformation("トランスコード終了:{0}", name);
        }
    }
}
