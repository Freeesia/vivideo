using System.Collections.Generic;

namespace StudioFreesia.Vivideo.Worker.Model
{
    public class TranscodeSetting
    {
        public string? File { get; set; }
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
    }
}
