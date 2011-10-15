using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface ICacheSerializer
    {
        string CacheFilePath { get; }

        void SerializeCache(FileSystemCache cache);

        FileSystemCache DeserializeCache();

        void DeleteCache();
    }
}
