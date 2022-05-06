namespace StudioFreesia.Vivideo.Worker.Model;

public class TranscodeSetting
{
    public string? HWAccel { get; set; }
    public Dictionary<string, string> HWDecoders { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string HWEncoder { get; set; } = "h264";
    public string AdditionalParams { get; set; } = string.Empty;
    public Uri? OutputHost { get; set; }
}
