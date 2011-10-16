namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents an item of the filesystem and the part of its name that was matched by the search query.
    /// </summary>
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
