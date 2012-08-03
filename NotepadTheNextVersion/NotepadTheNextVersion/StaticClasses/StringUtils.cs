using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NotepadTheNextVersion.StaticClasses
{
    public static class StringUtils
    {
        public static bool IsNullOrWhitespace(string s)
        {
            if (s == null)
                return true;

            return s.Trim().Equals(string.Empty);
        }

        public static bool EqualsIgnoreCase(this string s, string other)
        {
            return s.ToLower().Equals(other.ToLower());
        }

        public static bool StartsWithIgnoreCase(this string s, string other)
        {
            return s.ToLower().StartsWith(other.ToLower());
        }
    }
}
