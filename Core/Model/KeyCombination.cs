using System.Collections.Generic;
using System.Linq;

namespace Core.Model
{
    public class KeyCombination : List<KeyEquivalents>
    {
        //public List<KeyEquivalents> Get

        public bool KeyEquivalentsArePresent(KeyEquivalents keyEquivalents)
        {
            foreach (KeyEquivalents currentEquivalents in this)
            {
                bool intersect = currentEquivalents.Intersect(keyEquivalents).Any();
                if (intersect)
                {
                    return true;
                }
            }

            return false;
        }

        public void InsertKeyEquivalents(KeyEquivalents keyEquivalents)
        {
            DeleteKeyEquivalents(keyEquivalents);

            Add(keyEquivalents);
        }

        public void DeleteKeyEquivalents(KeyEquivalents keyEquivalents)
        {
            List<KeyEquivalents> copy = new List<KeyEquivalents>(this);
            foreach (KeyEquivalents currentEquivalents in copy)
            {
                bool intersect = currentEquivalents.Intersect(keyEquivalents).Any();
                if (intersect)
                {
                    Remove(currentEquivalents);
                }
            }
        }

        public void UpdateKeyEquivalents(KeyEquivalents keyEquivalents, bool arePresent)
        {
            if (arePresent)
            {
                InsertKeyEquivalents(keyEquivalents);
            }
            else
            {
                DeleteKeyEquivalents(keyEquivalents);
            }
        }
    }
}
