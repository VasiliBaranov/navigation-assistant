namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents a part of the match string that was either fully matched or not.
    /// </summary>
    public class MatchSubstring
    {
        public string Value { get; set; }

        public bool IsMatched { get; set; }

        public MatchSubstring()
        {
        }

        public MatchSubstring(string text, bool isMatched)
        {
            Value = text;
            IsMatched = isMatched;
        }
    }
}
