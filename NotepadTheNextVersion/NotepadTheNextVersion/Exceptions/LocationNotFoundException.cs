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
using System.IO;

namespace NotepadTheNextVersion.Exceptions
{
    public class LocationNotFoundException : IOException
    {
        public LocationNotFoundException()
            : base() { }

        public LocationNotFoundException(string msg)
            : base(msg) { }

        public LocationNotFoundException(Exception innerException)
            : base("", innerException) { }

        public LocationNotFoundException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }
}
