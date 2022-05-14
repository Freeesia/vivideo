namespace StudioFreesia.Vivideo.Server.Model;

public record ContentNode(string Name, bool IsDirectory, string ContentPath, DateTime CreatedAt, bool Transcoded, double Duration)
{
    public ContentNode(string path, bool isDirectory, DateTime createdAt, bool transcoded, double duration)
        : this(Path.GetFileNameWithoutExtension(path), isDirectory, path.Replace('\\', '/'), createdAt, transcoded, duration)
    {
    }
}
