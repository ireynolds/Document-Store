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
using Microsoft.Live;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Models
{
    public class SDirectory : LDirectory
    {
        public SDirectory(PathStr p)
            : base()
        {
            if (!FileUtils.IsSkydriveDir(p.PathString))
                throw new Exception();
            _path = p;
        }

        public SDirectory(LDirectory parent, string name)
            : base()
        {
            if (!FileUtils.IsSkydriveDir(name))
                throw new Exception();
            _path = parent.Path.NavigateIn(name, ItemType.SkydriveDirectory);
        }

        public override IActionable Delete(bool permanently = false)
        {
            MessageBox.Show("You cannot delete directories from your SkyDrive. Please use SkyDrive's web client.", "Cannot Delete", MessageBoxButton.OK);
            return this;
        }

        public override IActionable Move(LDirectory newParent)
        {
            MessageBox.Show("You cannot move directories from your SkyDrive. Please use SkyDrive's web client.", "Cannot Delete", MessageBoxButton.OK);
            return this;
        }

        public override IActionable Rename(string newDirectoryName)
        {
            MessageBox.Show("You cannot rename directories from your SkyDrive. Please use SkyDrive's web client.", "Cannot Delete", MessageBoxButton.OK);
            return this;
        }
    }
}
