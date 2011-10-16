using System.IO;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents an item of the file system.
    /// </summary>
    /// <remarks>
    /// Currently the application works just with folders.
    /// </remarks>
    public class FileSystemItem
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public FileSystemItem()
        {
        }

        public FileSystemItem(string name, string path)
        {
            Name = name;
            FullPath = Path.GetFullPath(path);
        }

        public FileSystemItem(string path)
        {
            Name = Path.GetFileName(path);
            FullPath = Path.GetFullPath(path);
        }
    }
}
