using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.ViewModel;
using Application = System.Windows.Application;

namespace NavigationAssistant.PresentationServices.Implementations
{
    /// <summary>
    /// Implements methods for performing supplementary presentation tasks.
    /// </summary>
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

        // See http://sanity-free.org/143/csharp_dotnet_single_instance_application.html
        public bool ApplicationIsRunning(Mutex mutex)
        {
            // So, if our app is running, WaitOne will return false
            return !mutex.WaitOne(TimeSpan.Zero, true);
        }

        public void MakeForeground(Window window)
        {
            //Both calls are necessary, as visibility and being a foreground window are independent.
            window.Show();
            window.Activate();

            ReallySetForegroundWindow(window);
        }

        //The code is taken from http://forums.purebasic.com/english/viewtopic.php?f=12&t=7424&hilit=real+SetForegroundWindow
        //Other solution also exist:
        // http://stackoverflow.com/questions/46030/c-force-form-focus
        //(not the main answer; the main answer is just equivalent to window.Show(); window.Activate();
        //and activate just calls SetForegroundWindow with all its restrictions).
        //http://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo.aspx

        //Stackoverflow solution seems to work worse 
        //(sometimes a black screen appears instead of a window, and sometimes focus is not set).
        private void ReallySetForegroundWindow(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;

            //Check to see if we are on the foreground thread
            uint foregroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint ourThreadId = GetCurrentThreadId();

            //If not, attach our thread's 'input' to the foreground thread's
            if (foregroundThreadId != ourThreadId)
            {
                AttachThreadInput(foregroundThreadId, ourThreadId, true);
            }

            //Bring our window to the foreground
            SetForegroundWindow(hWnd);

            //If we are attached to our thread, detach it now
            if (foregroundThreadId != ourThreadId)
            {
                AttachThreadInput(foregroundThreadId, ourThreadId, false);
            }

            //Force our window to redraw
            InvalidateRect(hWnd, IntPtr.Zero, true);
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
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        #endregion

    }
}
