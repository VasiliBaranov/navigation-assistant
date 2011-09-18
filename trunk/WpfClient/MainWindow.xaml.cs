using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            List<MatchedFileSystemItem> matches = _navigationAssistant.GetFolderMatches(new List<string>{rootFolderTextBox.Text}, matchTextBox.Text);

            IEnumerable<string> resultLines = matches.Select(m => m.ItemPath);
            string result = string.Join(Environment.NewLine, resultLines.ToArray());

            resultsTextBox.Text = result;
        }
    }
}
