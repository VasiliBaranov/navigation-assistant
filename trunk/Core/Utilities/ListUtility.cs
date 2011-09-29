using System.Collections.Generic;

namespace Core.Utilities
{
    public static class ListUtility
    {
        public static bool IsNullOrEmpty<T>(ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}
