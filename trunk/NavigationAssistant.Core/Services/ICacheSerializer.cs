using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for file system cache serialization/deserialization.
    /// </summary>
    public interface ICacheSerializer
    {
        string CacheFilePath { get; }

        void SerializeCache(FileSystemCache cache);

        FileSystemCache DeserializeCache();

        void DeleteCache();
    }
}
