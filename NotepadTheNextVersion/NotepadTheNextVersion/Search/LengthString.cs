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

namespace NotepadTheNextVersion.Search
{
    public class LengthString : IComparable<LengthString>
    {
        public string Value;

        public LengthString(string value)
        {
            Value = value;
        }

        // longer strings come first
        public int CompareTo(LengthString other)
        {
            return other.Value.Length - this.Value.Length;
        }
    }
}
