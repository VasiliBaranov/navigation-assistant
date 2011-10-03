﻿using System;
using System.Collections.Generic;
using NavigationAssistant.Utilities;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.Views
{
    public interface INavigationView : IView
    {
        event EventHandler<ItemEventArgs<string>> TextChanged;

        event EventHandler<ItemEventArgs<string>> FolderSelected;

        void ShowMatches(List<MatchModel> matches);
    }
}
