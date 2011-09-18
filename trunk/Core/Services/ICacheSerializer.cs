using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface ICacheSerializer
    {
        void SerializeCache(List<FileSystemItem> cache);

        List<FileSystemItem> DeserializeCache();
    }
}
