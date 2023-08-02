using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.FileFormats {
    static class StringEx {
        static readonly char[] whiteChars = new[] { ' ', '\t' };

        public static string[] SplitOnWhitespace(this string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return new string[0];
            }

            return input.Split(whiteChars, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
