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
    public class MoveListItem : ContentControl
    {
        public Directory DirectoryItem;
        private StackPanel _panel;
        private TextBlock name;

        public new bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
                if (!base.IsEnabled)
                    name.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        public MoveListItem(Directory d)
        {
            DirectoryItem = d;

            // Set up appearance
            _panel = new StackPanel();
            _panel.Orientation = Orientation.Vertical;
            _panel.Margin = new Thickness(12 + 15 * DirectoryItem.Path.Depth, 0, 0, 15);
            this.Content = _panel;

            name = new TextBlock();
            TextBlock path = new TextBlock();

            _panel.Children.Add(name);
            _panel.Children.Add(path);

            name.Text = DirectoryItem.DisplayName;
            name.FontSize = 50;
            name.FontFamily = new FontFamily("Segoe WP SemiLight");
            name.Margin = new Thickness(0, 0, 0, -5);

            path.Text = DirectoryItem.Path.DisplayPathString;
            path.Foreground = (Brush)Application.Current.Resources["PhoneSubtleBrush"];
        }
    }
}
