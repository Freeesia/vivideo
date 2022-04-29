namespace StudioFreesia.Vivideo.Worker.Model;

public class TranscodeSetting
{
    public string? Ffmpeg { get; set; }
    public string? Ffprobe { get; set; }
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}
