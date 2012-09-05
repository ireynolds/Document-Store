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
using NotepadTheNextVersion.Models;

namespace NotepadTheNextVersion.ListItems
{
    public class SkydriveDirectoryListItem : DirectoryListItem
    {
        public SkydriveDirectoryListItem(IActionable a)
            : base(a)
        {
            fullPathTextBlock.Text = "skydrive";
        }
    }
}
