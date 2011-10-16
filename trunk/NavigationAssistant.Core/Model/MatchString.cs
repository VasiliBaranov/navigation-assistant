using System.Collections.Generic;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents a string, containing information about matches to the search query.
    /// </summary>
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
