using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Core.Model;

namespace WindowsExplorerClient.ViewModel
{
    public class MatchModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MatchString _matchedText;

        private TextBlock _textBlock;

        private string _path;

        private bool _isFocused;

        public MatchModel(MatchString matchedText, string path)
        {
            _matchedText = matchedText;
            _path = path;

            _textBlock = GetTextBlock(_matchedText);
        }

        public MatchString MatchedText
        {
            get
            {
                return _matchedText;
            }
            set
            {
                _matchedText = value;
                _textBlock = GetTextBlock(_matchedText);

                OnPropertyChanged("MatchedText");
                OnPropertyChanged("TextBlock");
            }
        }

        public TextBlock TextBlock
        {
            get
            {
                return _textBlock;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }

        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                _isFocused = value;
                OnPropertyChanged("IsFocused");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private TextBlock GetTextBlock(MatchString matchedText)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;

            foreach (MatchSubstring matchSubstring in matchedText)
            {
                Run substringControl = new Run(matchSubstring.Value);
                if (matchSubstring.IsMatched)
                {
                    substringControl.TextDecorations = TextDecorations.Underline;
                    substringControl.Foreground = Brushes.Blue;
                }

                textBlock.Inlines.Add(substringControl);
            }

            return textBlock;
        }
    }
}
