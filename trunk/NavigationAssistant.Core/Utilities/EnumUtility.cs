using System.Windows.Forms;

namespace NavigationAssistant.Core.Utilities
{
    public static class EnumUtility
    {
        public static bool IsPresent(Keys key, Keys keyToCheck)
        {
            return (key & keyToCheck) != 0;
        }

        public static Keys AddKey(Keys initial, Keys addition)
        {
            return initial | addition;
        }

        public static Keys RemoveKey(Keys initial, Keys addition)
        {
            return initial & ~addition;
        }

        public static Keys UpdateKey(Keys initial, Keys addition, bool isPresent)
        {
            if (isPresent)
            {
                return AddKey(initial, addition);
            }
            else
            {
                return RemoveKey(initial, addition);
            }
        }

        //I.e. a key without modifiers
        public static Keys ExtractNotModifiedKey(Keys key)
        {
            return key & Keys.KeyCode;
        }
    }
}
