namespace Core.Model
{
    public class MatchedFileSystemItem : FileSystemItem
    {
        //Add matched name

        public MatchedFileSystemItem()
        {
            
        }

        public MatchedFileSystemItem(FileSystemItem fileSystemItem)
        {
            ItemName = fileSystemItem.ItemName;
            ItemPath = fileSystemItem.ItemPath;
        }
    }
}
