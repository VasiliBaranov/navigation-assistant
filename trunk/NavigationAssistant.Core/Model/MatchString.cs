using System.Collections.Generic;

namespace NavigationAssistant.Core.Model
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
