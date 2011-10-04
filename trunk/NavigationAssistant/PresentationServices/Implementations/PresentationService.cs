﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.ViewModel;
using Application = System.Windows.Application;

namespace NavigationAssistant.PresentationServices.Implementations
{
    public class PresentationService : IPresentationService
    {
        #region Public Methods

        public MatchModel MoveSelectionUp(ObservableCollection<MatchModel> matches, MatchModel selectedMatch)
        {
            if (ListUtility.IsNullOrEmpty(matches))
            {
                return null;
            }

            int selectionIndex = matches.IndexOf(selectedMatch);
            selectionIndex--;

            if (selectionIndex < 0)
            {
                selectionIndex = matches.Count - 1;
            }

            return matches[selectionIndex];
        }

        public MatchModel MoveSelectionDown(ObservableCollection<MatchModel> matches, MatchModel selectedMatch)
        {
            if (ListUtility.IsNullOrEmpty(matches))
            {
                return null;
            }

            int selectionIndex = matches.IndexOf(selectedMatch);
            selectionIndex++;

            if (selectionIndex == matches.Count)
            {
                selectionIndex = 0;
            }

            return matches[selectionIndex];
        }

        public double GetMaxMatchesListHeight(double searchTextBoxTop, double searchTextBoxHeight)
        {
            double windowTopPosition = CurrentWindow.Top;
            double screenHeight = ScreenWorkingArea.Height;

            double availableHeight = screenHeight - windowTopPosition - searchTextBoxTop - searchTextBoxHeight;
            return availableHeight * Constants.MaxScreenFillingRatio;
        }

        public double GetMaxMatchesListWidth(double searchTextBoxLeft)
        {
            double windowLeftPosition = CurrentWindow.Left;
            double screenWidth = ScreenWorkingArea.Width;

            double availableWidth = screenWidth - windowLeftPosition - searchTextBoxLeft;
            return availableWidth * Constants.MaxScreenFillingRatio;
        }

        public bool ApplicationIsRunning()
        {
            Process[] processes = Process.GetProcessesByName("NavigationAssistant");

            return processes.Length > 1;
        }

        //The code is taken from http://stackoverflow.com/questions/46030/c-force-form-focus
        //(not the main answer; the main answer is just equivalent to window.Show(); window.Activate();
        //and activate just calls SetForegroundWindow with all its restrictions).
        //Other solution also exist:
        //http://forums.purebasic.com/english/viewtopic.php?f=12&t=7424&hilit=real+SetForegroundWindow
        //http://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo.aspx
        public void MakeForeground(Window window)
        {
            //Both calls are necessary, as visibility and being a foreground window are independent.
            window.Show();
            window.Activate();

            IntPtr hWnd = new WindowInteropHelper(window).Handle;

            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            ShowWindowAsync(hWnd, SW_SHOW);

            SetForegroundWindow(hWnd);

            // Code from Karl E. Peterson, www.mvps.org/vb/sample.htm
            // Converted to Delphi by Ray Lischner
            // Published in The Delphi Magazine 55, page 16
            // Converted to C# by Kevin Gale
            IntPtr foregroundWindow = GetForegroundWindow();
            IntPtr zeroPointer = IntPtr.Zero;

            uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, zeroPointer);
            uint thisThreadId = GetWindowThreadProcessId(hWnd, zeroPointer);

            if (AttachThreadInput(thisThreadId, foregroundThreadId, true))
            {
                BringWindowToTop(hWnd); // IE 5.5 related hack
                SetForegroundWindow(hWnd);
                AttachThreadInput(thisThreadId, foregroundThreadId, false);
            }

            if (GetForegroundWindow() != hWnd)
            {
                // Code by Daniel P. Stasinski
                // Converted to C# by Kevin Gale
                IntPtr timeout = IntPtr.Zero;
                SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);
                SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, zeroPointer, SPIF_SENDCHANGE);
                BringWindowToTop(hWnd); // IE 5.5 related hack
                SetForegroundWindow(hWnd);
                SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, SPIF_SENDCHANGE);
            }
        }

        #endregion

        #region Non Public Methods

        private Rectangle ScreenWorkingArea
        {
            get
            {
                //Need to do some hacks to handle multiple monitors.
                //For a single monitor System.Windows.SystemParameters.WorkArea or System.Windows.SystemParameters.PrimaryScreenHeight
                //would have been sufficient (as they return primary monitor parameters).
                //See http://stackoverflow.com/questions/254197/how-can-i-get-the-active-screen-dimensions
                Screen screen = GetScreen(CurrentWindow);

                return screen.WorkingArea;
            }
        }

        private Window CurrentWindow
        {
            get { return Application.Current.MainWindow; }
        }

        private static Screen GetScreen(Window window)
        {
            return Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

        #endregion

        #region Win APi

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, ref Int32 lpdwProcessId);
        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_NORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_MAX = 10;

        private const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        private const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        private const int SPIF_SENDCHANGE = 0x2;

        #endregion

    }
}