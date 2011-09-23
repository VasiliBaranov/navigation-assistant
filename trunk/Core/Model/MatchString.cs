using System.Collections.Generic;

namespace Core.Model
{
    public class MatchString : List<MatchSubstring>
    {
        public MatchString()
        {
            
        }

        public MatchString(IEnumerable<MatchSubstring> substrings) : base(substrings)
        {
            
        }
    }
}
