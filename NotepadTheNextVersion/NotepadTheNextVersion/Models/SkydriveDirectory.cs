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
using NotepadTheNextVersion.Utilities;
using System.Collections.ObjectModel;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Navigation;

namespace NotepadTheNextVersion.Models
{
    public class SkydriveDirectory : Directory
    {
        public SkydriveDirectory(PathStr p)
            : base()
        {
            if (!FileUtils.IsDir(p.PathString))
                throw new Exception();
            _path = p;
        }

        public SkydriveDirectory(Directory parent, string name)
            : base()
        {
            if (!FileUtils.IsSkydriveDir(name))
                throw new Exception();
            _path = parent.Path.NavigateIn(name, ItemType.SkydriveDirectory);
        }
    }
}
