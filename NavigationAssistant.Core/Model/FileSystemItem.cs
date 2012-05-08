using System.IO;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents an item of the file system.
    /// </summary>
    /// <remarks>
    /// Currently the application works just with folders.
    /// </remarks>
    /// <remarks>
    /// We are calling GetFullPath inside the constructors, though it may throw PathTooLong exception, instead of requiring to pass fullPath explicitly, as:
    /// 1. the requirement to pass full path will create a hidden constraint, not all clients may remember to pass full path
    /// 2. some clients may prefer to catch exception at once, some may prefer to bubble it up by call stack

    /// Also, we do not introduce a property "IsValid" (to be set to false in case of PathTooLong exception) not to spoil structured exception handling ideology.
    /// TODO: think of that, it may be more explicit.
    /// </remarks>
    public class FileSystemItem
    {
        /// <summary>
        /// Gets or sets the name of the folder or file. This field is introduce for performance (to avoid searching folder name each time it's needed).
        /// </summary>
        public string Name { get; set; }

        public string FullPath { get; set; }

        public FileSystemItem()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="PathTooLongException"></exception>
        public FileSystemItem(string name, string path)
        {
            Name = name;
            FullPath = Path.GetFullPath(path);
        }

        /// <exception cref="PathTooLongException"></exception>
        public FileSystemItem(string path)
        {
            Name = Path.GetFileName(path);
            FullPath = Path.GetFullPath(path);
        }
    }
}
