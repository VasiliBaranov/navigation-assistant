using System.IO;

namespace NavigationAssistant.Tests
{
    public static class Utility
    {
        public static void MakeFolderHidden(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            folder.Attributes = folder.Attributes | FileAttributes.Hidden;
        }

        public static void MakeFolderVisible(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            folder.Attributes = folder.Attributes & ~FileAttributes.Hidden;
        }

        public static void MakeFileHidden(string path)
        {
            FileInfo file = new FileInfo(path);
            file.Attributes = file.Attributes | FileAttributes.Hidden;
        }

        public static void MakeFileVisible(string path)
        {
            FileInfo folder = new FileInfo(path);
            folder.Attributes = folder.Attributes & ~FileAttributes.Hidden;
        }
    }
}
