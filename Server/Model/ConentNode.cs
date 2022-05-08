namespace StudioFreesia.Vivideo.Server.Model;

public record ContentNode(string Name, bool IsDirectory, string ContentPath, DateTime CreatedAt, bool Transcoded)
{
    public ContentNode(string path, bool isDirectory, DateTime createdAt, bool transcoded)
        : this(Path.GetFileNameWithoutExtension(path), isDirectory, path.Replace('\\', '/'), createdAt, transcoded)
    {
    }
}
