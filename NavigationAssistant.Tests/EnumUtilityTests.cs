using System.Windows.Forms;
using NUnit.Framework;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class EnumUtilityTests
    {
        [Test]
        public void IsPresent_ModifierPresent_ReturnsTrue()
        {
            const Keys keys = Keys.L | Keys.Control;
            bool present = EnumUtility.IsPresent(keys, Keys.Control);
            Assert.That(present, Is.True);
        }

        [Test]
        public void IsPresent_ModifierAbsent_ReturnsFalse()
        {
            const Keys keys = Keys.L | Keys.Control;
            bool present = EnumUtility.IsPresent(keys, Keys.Shift);
            Assert.That(present, Is.False);
        }

        [Test]
        public void AddKey_IsCorrect()
        {
            Keys modified = EnumUtility.AddKey(Keys.L, Keys.Control);
            Assert.That(modified, Is.EqualTo(Keys.L | Keys.Control));
        }

        [Test]
        public void RemoveKey_ModifierPresent_ModifierRemoved()
        {
            Keys modified = EnumUtility.RemoveKey(Keys.L | Keys.Control, Keys.Control);
            Assert.That(modified, Is.EqualTo(Keys.L));
        }

        [Test]
        public void RemoveKey_ModifierAbsent_KeyUnchanged()
        {
            Keys modified = EnumUtility.RemoveKey(Keys.L | Keys.Control, Keys.Shift);
            Assert.That(modified, Is.EqualTo(Keys.L | Keys.Control));
        }
    }
}
