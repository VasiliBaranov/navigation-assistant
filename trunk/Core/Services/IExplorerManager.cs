using Core.Model;

namespace Core.Services
{
    public interface IExplorerManager
    {
        bool IsExplorer(ApplicationWindow hostWindow);

        IExplorer GetExplorer(ApplicationWindow hostWindow);

        IExplorer CreateExplorer();
    }
}
