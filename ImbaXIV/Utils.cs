using System;
using System.Collections.Generic;
using System.Globalization;

namespace ImbaXIV
{
    class Utils
    {
        private class PatternInfo
        {
            public bool[] IsWildcardArray { get; }
            public byte[] PatternArray { get; }
            public int Length { get; }

            public PatternInfo(string pattern)
            {
                List<bool> IsWildcardList = new List<bool>();
                List<byte> PatternList = new List<byte>();
                string[] bytes = pattern.Split(' ');

                foreach (var b in bytes)
                {
                    if (b.Equals("??"))
                    {
                        IsWildcardList.Add(true);
                        PatternList.Add(0);
                    }
                    else
                    {
                        IsWildcardList.Add(false);
                        byte byteVal = (byte)Int32.Parse(b, NumberStyles.HexNumber);
                        PatternList.Add(byteVal);
                    }
                }
                IsWildcardArray = IsWildcardList.ToArray();
                PatternArray = PatternList.ToArray();
                Length = PatternList.Count;
            }
        }

        public static int BytePatternMatch(byte[] haystack, string pattern)
        {
            PatternInfo patternInfo = new PatternInfo(pattern);
            int i;
            bool found = false;
            for (i = 0; i < haystack.Length && i + patternInfo.Length <= haystack.Length; ++i)
            {
                found = true;
                for (int j = 0; j < patternInfo.Length; ++j)
                {
                    if (patternInfo.IsWildcardArray[j])
                        continue;
                    if (haystack[i + j] != patternInfo.PatternArray[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    break;
            }
            return found ? i : -1;
        }
    }
}
