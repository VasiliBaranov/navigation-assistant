using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface ICacheSerializer
    {
        void SerializeCache(FileSystemCache cache);

        FileSystemCache DeserializeCache();
    }
}
