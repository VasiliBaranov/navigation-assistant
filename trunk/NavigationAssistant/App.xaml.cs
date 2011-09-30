using System.Windows;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;

namespace NavigationAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            IPresentationService presentationService = new PresentationService();
            bool appIsRunning = presentationService.ApplicationIsRunning();

            if(appIsRunning)
            {
                MessageBox.Show("Navigation Assistant is already running");
                Shutdown();
            }
        }
    }
}
