using Core.Model;

namespace Core.Services
{
    public interface IExplorerManager
    {
        bool IsWindowExplorer(ApplicationWindow hostWindow);

        IExplorer GetExplorer(ApplicationWindow hostWindow);

        IExplorer CreateExplorer();
    }
}
