using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WindowsExplorerClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _searchText;

        private readonly ObservableCollection<string> _matches = new ObservableCollection<string>();

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;

                _matches.Clear();
                _matches.Add(_searchText + "1");
                _matches.Add(_searchText + "2");

                PropertyChanged(this, new PropertyChangedEventArgs("Matches"));
                PropertyChanged(this, new PropertyChangedEventArgs("SearchText"));
            }
        }

        public ObservableCollection<string> Matches
        {
            get
            {
                return _matches;
            }
        }
    }
}
