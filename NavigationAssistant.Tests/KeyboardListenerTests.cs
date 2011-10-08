using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [Ignore("It seems to be too hard to send keyboard events to be handled with global listener.")]
    [TestFixture]
    public class KeyboardListenerTests
    {
        private IKeyboardListener _keyboardListener;
        private bool _keyPressHandled;

        [SetUp]
        public void SetUp()
        {
            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyCombinationPressed += HandleKeyCombinationPressed;
            _keyPressHandled = false;
        }

        [Test]
        [TestCaseSource("GetListeningData")]
        //For keysToPress refer to http://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.send.aspx
        public void TestListeningToKeyCombination(Keys keysToListen, string keysToPress, bool shouldRaiseEvent)
        {
            _keyboardListener.StartListening(keysToListen);

            SendKeys.SendWait(keysToPress);

            Thread.Sleep(200);

            Assert.That(_keyPressHandled, Is.EqualTo(shouldRaiseEvent));
        }

        private void HandleKeyCombinationPressed(object sender, System.EventArgs e)
        {
            _keyPressHandled = true;
        }

        public IEnumerable<TestCaseData> GetListeningData()
        {
            //Keys keysToListen;
            //Keys keysToPress;
            //bool shouldRaiseEvent;
            TestCaseData testCaseData;

            //keysToListen = Keys.L;
            //keysToPress = Keys.L;
            //shouldRaiseEvent = true;
            testCaseData = new TestCaseData(Keys.L, "L", true).SetName("ExpectSimpleKey_ListenThisKey_KeyHandled");
            yield return testCaseData;
        }
    }
}
