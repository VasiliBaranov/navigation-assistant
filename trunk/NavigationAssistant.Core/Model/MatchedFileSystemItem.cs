namespace NavigationAssistant.Core.Model
{
    public class MatchedFileSystemItem : FileSystemItem
    {
        public MatchString MatchedItemName { get; set; }

        public MatchedFileSystemItem()
        {
            
        }

        public MatchedFileSystemItem(FileSystemItem fileSystemItem, MatchString matchedString)
        {
            Name = fileSystemItem.Name;
            FullPath = fileSystemItem.FullPath;
            MatchedItemName = matchedString;
        }
    }
}
