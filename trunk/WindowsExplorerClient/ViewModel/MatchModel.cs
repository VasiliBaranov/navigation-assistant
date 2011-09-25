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
        #region Fields

        public event PropertyChangedEventHandler PropertyChanged;

        private MatchString _matchedText;

        private TextBlock _textBlock;

        private string _path;

        private bool _isSelected;

        private readonly IMatchModelMapper _matchModelMapper;

        #endregion

        #region Constructors

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

        #endregion

        #region Properties

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

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region Non Public Methods

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

        #endregion
    }
}
