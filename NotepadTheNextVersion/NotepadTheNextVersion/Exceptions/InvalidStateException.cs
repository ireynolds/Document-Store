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
    public class InvalidStateException : IOException
    {
        public InvalidStateException()
            : base() { }

        public InvalidStateException(string msg)
            : base(msg) { }

        public InvalidStateException(Exception innerException)
            : base("", innerException) { }

        public InvalidStateException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }
}
