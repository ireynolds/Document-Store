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
    public class File : IEquatable<File>
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string ParentDirectory { get; private set; }
        public DateTimeOffset DateEdited { get; private set; }

        public File() { }

        public File(string FilePath, string FileName, string ParentDirectory, DateTimeOffset DateEdited)
        {
            this.FilePath = FilePath;
            this.FileName = FileName;
            this.ParentDirectory = ParentDirectory;
            this.DateEdited = DateEdited;
        }

        public bool Equals(File other)
        {
            return this.FilePath.Equals(other.FilePath);
        }
    }
}
