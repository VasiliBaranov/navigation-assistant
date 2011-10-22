using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Defines methods for the main navigation view.
    /// </summary>
    public interface INavigationView : IView
    {
        event EventHandler<ItemEventArgs<string>> TextChanged;

        event EventHandler<ItemEventArgs<string>> FolderSelected;

        void ShowMatches(List<MatchModel> matches);

        bool ShowInitializingScreen { get; set; }
    }
}
