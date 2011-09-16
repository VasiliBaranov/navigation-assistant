using System.Collections.Generic;

namespace Core
{
    public static class Utilities
    {
        public static List<string> SplitStringByUpperChars(string input)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            int previousUpperCharIndex = 0;
            int inputLength = input.Length;

            for (int i = 0; i < inputLength; i++)
            {
                char currentChar = input[i];

                bool shouldAddSubstring = char.IsUpper(currentChar) && (i != 0);

                if (!shouldAddSubstring)
                {
                    continue;
                }

                //E.g. input = myInput; previousUpperCharIndex = 0, i = 2.
                string substring = input.Substring(previousUpperCharIndex, i - previousUpperCharIndex);
                result.Add(substring);

                previousUpperCharIndex = i;
            }

            //Add the last substring
            string lastSubstring = input.Substring(previousUpperCharIndex);
            result.Add(lastSubstring);

            return result;
        }

        public static bool IsNullOrEmpty<T>(ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}
