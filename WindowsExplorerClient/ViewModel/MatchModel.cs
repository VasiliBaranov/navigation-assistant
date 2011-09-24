using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Model;
using WindowsExplorerClient.PresentationServices;

namespace WindowsExplorerClient.ViewModel
{
    public class MatchModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MatchString _matchedText;

        private TextBlock _textBlock;

        private string _path;

        private bool _isFocused;

        private readonly IMatchModelMapper _matchModelMapper;

        public MatchModel(IMatchModelMapper matchModelMapper, MatchString matchedText, string path)
        {
            _matchedText = matchedText;
            _path = path;
            _matchModelMapper = matchModelMapper;

            _textBlock = GetTextBlock(_matchedText);
        }

        public MatchModel(IMatchModelMapper matchModelMapper, string text)
        {
            _path = null;
            _matchModelMapper = matchModelMapper;

            MatchSubstring substring = new MatchSubstring(text, false);
            List<MatchSubstring> substrings = new List<MatchSubstring> { substring };
            _matchedText = new MatchString(substrings);

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
            return _matchModelMapper.GetTextBlock(matchedText, TextDecorations.Underline, Brushes.Blue);
        }
    }
}
