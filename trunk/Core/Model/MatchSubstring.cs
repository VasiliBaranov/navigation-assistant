namespace Core.Model
{
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
