using System.IO;

namespace NavigationAssistant.Core.Model
{
    public class FileSystemItem
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public bool IsHidden { get; set; }

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
