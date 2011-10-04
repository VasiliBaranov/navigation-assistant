using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface ICacheSerializer
    {
        void SerializeCache(List<FileSystemItem> cache);

        List<FileSystemItem> DeserializeCache();

        string CacheFolder { get; set; }
    }
}
