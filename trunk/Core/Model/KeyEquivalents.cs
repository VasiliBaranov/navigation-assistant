using System.Collections.Generic;
using System.Windows.Forms;

namespace Core.Model
{
    //I know that Keys enumeration has a Flags attribute;
    //but the hack which is used to listen to global keyboard events (see HookManager)
    //returns Keys.LControlKey if left control is used (which is correct, btw);
    //but Keys.LControlKey == RButton | Space | F17 (god knows, why).
    //So using custom lists is a more reliable solution.
    public class KeyEquivalents : List<Keys>
    {
        public KeyEquivalents()
        {

        }

        public KeyEquivalents(Keys key)
        {
            Add(key);
        }

        public static KeyEquivalents Control
        {
            get { return new KeyEquivalents {Keys.LControlKey, Keys.RControlKey}; }
        }

        public static KeyEquivalents Shift
        {
            get { return new KeyEquivalents { Keys.LShiftKey, Keys.RShiftKey }; }
        }

        public static KeyEquivalents Alt
        {
            get { return new KeyEquivalents(Keys.Alt); }
        }
    }
}
