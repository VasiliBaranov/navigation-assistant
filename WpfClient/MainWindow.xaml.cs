using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INavigationAssistant _navigationAssistant;

        public MainWindow()
        {
            InitializeComponent();

            _navigationAssistant = new NavigationAssistant(new FileSystemParser(), new MatchSearcher());
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            List<MatchedFileSystemItem> matches = _navigationAssistant.GetFolders(rootFolderTextBox.Text, matchTextBox.Text);

            IEnumerable<string> resultLines = matches.Select(m => m.ItemPath);
            string result = string.Join(Environment.NewLine, resultLines);

            resultsTextBox.Text = result;
        }
    }
}
