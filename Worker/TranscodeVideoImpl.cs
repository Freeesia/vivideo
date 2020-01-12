using System;
using System.Threading.Tasks;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Worker
{
    public class TranscodeVideoImpl : ITranscodeVideo
    {
        public Task<string> Transcode(string path)
        {
            Console.WriteLine($"path: {path}");
            return Task<string>.FromResult(string.Empty);
        }
    }
}
