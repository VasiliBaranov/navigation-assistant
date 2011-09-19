namespace WindowsExplorerClient
{
    public class MatchModel
    {
        public string Text { get; set; }

        public string Path { get; set; }

        public MatchModel(string text, string path)
        {
            Text = text;
            Path = path;
        }
    }
}
