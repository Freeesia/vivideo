using System;
using System.IO;

namespace StudioFreesia.Vivideo.Server.Model
{
    public class ContentNode
    {
        public string Name { get; }
        public bool IsDirectory { get; }
        public string ContentPath { get; }

        public DateTime CreatedAt { get; }

        public ContentNode(string path, bool isDirectory, DateTime createdAt)
        {
            this.Name = Path.GetFileNameWithoutExtension(path);
            this.IsDirectory = isDirectory;
            this.ContentPath = path.Replace('\\', '/');
            this.CreatedAt = createdAt;
        }
    }
}
