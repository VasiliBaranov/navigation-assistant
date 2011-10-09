using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class NavigationServiceTests
    {
        private Mock<IFileSystemParser> _fileSystemParserMock = new Mock<IFileSystemParser>();
        private Mock<IMatchSearcher> _matchSearcherMock = new Mock<IMatchSearcher>();
        private Mock<IExplorerManager> _primaryManagerMock = new Mock<IExplorerManager>();
        private Mock<IExplorerManager> _secondaryManagerMock = new Mock<IExplorerManager>();

        private IExplorerManager _primaryManager;
        private IExplorerManager _secondaryManager;
        private List<IExplorerManager> _supportedManagers;

        private INavigationService _navigationService;

        [SetUp]
        public void SetUp()
        {
            _fileSystemParserMock = new Mock<IFileSystemParser>();
            _matchSearcherMock = new Mock<IMatchSearcher>();
            _primaryManagerMock = new Mock<IExplorerManager>();
            _secondaryManagerMock = new Mock<IExplorerManager>();

            _primaryManager = _primaryManagerMock.Object;
            _secondaryManager = _secondaryManagerMock.Object;
            _supportedManagers = new List<IExplorerManager> {_primaryManager, _secondaryManager};

            _navigationService = new NavigationService(_fileSystemParserMock.Object,
                                                       _matchSearcherMock.Object,
                                                       _primaryManager,
                                                       _supportedManagers);
        }

        [Test]
        public void GetFolderMatches_ForNullSearch_ReturnsEmptyList()
        {
            List<MatchedFileSystemItem> folderMatches = _navigationService.GetFolderMatches(null);
            Assert.That(folderMatches.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetFolderMatches_ForNotNullSearch_ReturnsFilteredMatches()
        {
            List<FileSystemItem> subfolders = new List<FileSystemItem>();
            _fileSystemParserMock.Setup(parser => parser.GetSubFolders()).Returns(subfolders);

            List<MatchedFileSystemItem> expectedFolderMatches = new List<MatchedFileSystemItem>();
            _matchSearcherMock.Setup(
                searcher => searcher.GetMatches(subfolders, It.IsAny<string>()))
                .Returns(expectedFolderMatches);

            List<MatchedFileSystemItem> actualMatches = _navigationService.GetFolderMatches("asdasd");
            Assert.That(actualMatches, Is.EqualTo(expectedFolderMatches));
        }

        [Test]
        public void NavigateTo_ForNonNavigatorWindow_CreatesNewNavigator()
        {
            ApplicationWindow window = new ApplicationWindow();

            Mock<IExplorer> primaryNavigatorMock = new Mock<IExplorer>();
            _secondaryManagerMock.Setup(manager => manager.IsExplorer(window)).Returns(false);
            _primaryManagerMock.Setup(manager => manager.IsExplorer(window)).Returns(false);
            _primaryManagerMock.Setup(manager => manager.CreateExplorer()).Returns(primaryNavigatorMock.Object);

            _navigationService.NavigateTo("asdasd", window);
            primaryNavigatorMock.Verify(navigator => navigator.NavigateTo("asdasd"));
        }

        [Test]
        public void NavigateTo_ForSupportedNavigatorWindow_NavigatesWithThisNavigator()
        {
            ApplicationWindow window = new ApplicationWindow();

            Mock<IExplorer> secondaryNavigatorMock = new Mock<IExplorer>();
            _primaryManagerMock.Setup(manager => manager.IsExplorer(window)).Returns(false);
            _secondaryManagerMock.Setup(manager => manager.IsExplorer(window)).Returns(true);
            _secondaryManagerMock.Setup(manager => manager.GetExplorer(window)).Returns(secondaryNavigatorMock.Object);

            _navigationService.NavigateTo("asdasd", window);
            secondaryNavigatorMock.Verify(navigator => navigator.NavigateTo("asdasd"));
        }
    }
}
