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
    public class SearchableFile : File
    {
        public string FileText { get; private set; }

        public SearchableFile(string filePath, string fileName, string parentDirectory, DateTimeOffset DateEdited, string fileText)
            : base(filePath, fileName, parentDirectory, DateEdited)
        {
            this.FileText = fileText.ToLower();
        }
    }
}
