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
using System.Text;
using System.Collections.Generic;

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

        public MyUri AddArg(string key, string value)
        {
            string s = string.Empty;
            if (!this.OriginalString.EndsWith("?") && !this.OriginalString.EndsWith("&"))
                s += "?";
            s += key + "=" + value + "&";
            return this + s;
        }

        public MyUri AddArg(IActionable value)
        {
            return this.AddArg("param", Uri.EscapeUriString(value.Path.PathString));
        }

        public MyUri AddArgs(IList<IActionable> values)
        {
            var sb = new StringBuilder();
            sb.Append("?");
            for (int i = 0; i < values.Count; i++)
                sb.Append(i.ToString() + "=" + Uri.EscapeUriString(values[i].Path.PathString) + "&");
            return this + sb.ToString();
        }
    }
}
