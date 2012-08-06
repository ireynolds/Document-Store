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
    public class MoveListItem : StackPanel
    {
        public Directory DirectoryItem;

        public MoveListItem(Directory d)
        {
            DirectoryItem = d;

            // Set up appearance
            this.Orientation = Orientation.Vertical;
            this.Margin = new Thickness(12 + 15 * DirectoryItem.Path.Depth, 0, 0, 15);

            TextBlock name = new TextBlock();
            TextBlock path = new TextBlock();

            this.Children.Add(name);
            this.Children.Add(path);

            name.Text = DirectoryItem.Name;
            name.FontSize = 50;
            name.FontFamily = new FontFamily("Segoe WP SemiLight");
            name.Margin = new Thickness(0, 0, 0, -5);
            
            path.Text = DirectoryItem.Path.PathString;
            path.Foreground = (Brush)Application.Current.Resources["PhoneSubtleBrush"];
        }
    }
}
