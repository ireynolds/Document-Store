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

namespace NotepadTheNextVersion.Models
{
    public class MyUri : Uri
    {
        public MyUri(string s, UriKind k)
            : base(s, k) { }

        public static MyUri operator +(MyUri u1, string s2)
        {
            return new MyUri(u1.OriginalString + s2, UriKind.Relative);
        }
    }
}
