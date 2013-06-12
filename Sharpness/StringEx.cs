using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Sharpness
{
    public static class StringEx
    {
        public static string JoinStrings<T>(this IEnumerable<T> source,
                                                Func<T, string> projection, string separator)
        {
            var builder = new StringBuilder();
            bool first = true;
            foreach (T element in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(separator);
                }
                builder.Append(projection(element));
            }
            return builder.ToString();
        }

        public static string JoinStrings<T>(this IEnumerable<T> source, string separator)
        {
            return JoinStrings(source, t => t.ToString(), separator);
        }

        public static bool StartsWithOrdinal(this string self, string value)
        {
            return self.StartsWith(value, StringComparison.Ordinal);
        }

        public static bool StartsWithOrdinalNoCase(this string self, string value)
        {
            return self.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithOrdinal(this string self, string value)
        {
            return self.EndsWith(value, StringComparison.Ordinal);
        }

        public static bool EndsWithOrdinalNoCase(this string self, string value)
        {
            return self.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}