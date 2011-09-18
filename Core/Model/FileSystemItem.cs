namespace Core.Model
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
    }
}
