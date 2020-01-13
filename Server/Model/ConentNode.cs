using System;
using System.IO;

namespace StudioFreesia.Vivideo.Server.Model
{
    public class ContentNode
    {
        public string Name { get; }
        public bool IsDirectory { get; }
        public string ContentPath { get; }

        public ContentNode(string path, bool isDirectory)
        {
            this.Name = Path.GetFileNameWithoutExtension(path);
            this.IsDirectory = isDirectory;
            this.ContentPath = path.Replace('\\', '/');
        }
    }
}
