using System;

namespace StudioFreesia.Vivideo.Core
{
    public class LogoQueue
    {
        public Uri Url { get; }
        public string Output { get; }

        public LogoQueue(Uri url, string output)
            => (this.Url, this.Output) = (url, output);
    }
}
