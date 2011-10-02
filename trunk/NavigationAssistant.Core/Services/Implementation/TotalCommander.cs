using System.Diagnostics;
using System.IO;
using Core.Model;

namespace Core.Services.Implementation
{
    //Command line arguments can be found here: http://www.ghisler.ch/wiki/index.php/Command_line_parameters
    //If eventually you need a more sophisticated interaction, 
    //please see http://www.purebasic.fr/english/viewtopic.php?f=13&t=47321 (total commander send commands via SendMessage)
    public class TotalCommander : IExplorer
    {
        private readonly string _totalCommanderFolder;
        private readonly string _totalCommanderFileName;
        private readonly bool _openNewCommander;
        private readonly ApplicationWindow _hostWindow;

        public TotalCommander(ApplicationWindow hostWindow, string totalCommanderPath, bool openNewCommander)
        {
            _totalCommanderFolder = Path.GetFullPath(Path.GetDirectoryName(totalCommanderPath));
            _totalCommanderFileName = Path.GetFileName(totalCommanderPath);
            _openNewCommander = openNewCommander;
            _hostWindow = hostWindow;
        }

        public void NavigateTo(string path)
        {
            //Switches from help:
            // /T Opens the passed dir(s) in new tab(s). Now also works when Total Commander hasn't been open yet;
            // /L= Set path in left window;
            // /O If Total Commander is already running, activate it and pass the path(s) in the command line to that instance 
            //    (overrides the settings in the configuration dialog to have multiple windows);
            // /S Interprets the passed dirs as source/target instead of left/right (for usage with /O).
            //We use /S to make total commander set path for the currently active panel and tab.

            //A small hack is used: if there are several open total commanders 
            //and /O option is used, totalcmd.exe /O will open a currently topmost visible commander
            //(even if it's not a foreground/active currently; indeed, current foreground window is NavigationAssistant).
            //If _openNewCommander is false, a hostWindow will always be the visible commander,
            //so there is no need to search for the commander with the given window handle.
            string template = _openNewCommander
                                  ? "/T /L=\"{0}\""
                                  : "/O /S /L=\"{0}\"";

            string arguments = string.Format(template, Path.GetFullPath(path));

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = _totalCommanderFolder;
            processStartInfo.FileName = _totalCommanderFileName;
            processStartInfo.Arguments = arguments;
            Process.Start(processStartInfo);
        }
    }
}
