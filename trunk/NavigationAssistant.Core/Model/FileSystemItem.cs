using System.IO;

namespace NavigationAssistant.Core.Model
{
    public class FileSystemItem
    {
        public string ItemName { get; set; }

        public string ItemPath { get; set; }

        public FileSystemItem()
        {
        }

        public FileSystemItem(string itemName, string itemPath)
        {
            ItemName = itemName;
            ItemPath = itemPath;
        }

        public FileSystemItem(string itemPath)
        {
            ItemName = Path.GetFileName(itemPath);
            ItemPath = itemPath;
        }
    }
}
