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
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Views
{
    public class DirectoryListItem : ListingsListItem
    {
        private static readonly Thickness this_Margin = new  Thickness(0, 0, 0, 25);
        private static readonly Thickness image_Margin = new Thickness(12, 0, 10, 10);
        private static readonly Thickness displayNameTextBox_Margin = new Thickness(10, 0, 0, 0);
        private static readonly Thickness fullPathTextBox_Margin = new Thickness(12, -5, 0, 0);
        private static readonly FontFamily displayNameTextBox_FontFamily = new FontFamily("Segoe WP SemiLight");

        private TextBlock displayNameTextBlock;
        private TextBlock fullPathTextBlock;
        private Image image;
        private StackPanel verticalRightPanel;

        public DirectoryListItem(IActionable a)
            : base(a)
        {
            // Set up appearance
            verticalRightPanel = new StackPanel();
            displayNameTextBlock = new TextBlock();
            fullPathTextBlock = new TextBlock();
            image = new Image();

            this.Margin = this_Margin;
            this.Children.Add(image);
            this.Children.Add(verticalRightPanel);

            image.Source = new BitmapImage(getImageUri());
            image.Margin = image_Margin;
            image.Height = 45;
            image.Width = 41;
            image.Stretch = Stretch.Fill;

            verticalRightPanel.Orientation = Orientation.Vertical;
            verticalRightPanel.Children.Add(displayNameTextBlock);
            verticalRightPanel.Children.Add(fullPathTextBlock);

            displayNameTextBlock.Text = a.Name;
            displayNameTextBlock.FontSize = 45;
            displayNameTextBlock.Margin = displayNameTextBox_Margin;
            displayNameTextBlock.FontFamily = displayNameTextBox_FontFamily;
            displayNameTextBlock.TextWrapping = TextWrapping.NoWrap;

            fullPathTextBlock.Text = a.Path.PathString;
            fullPathTextBlock.Foreground = (Brush)App.Current.Resources["PhoneSubtleBrush"];
            fullPathTextBlock.Margin = fullPathTextBox_Margin;
            displayNameTextBlock.TextWrapping = TextWrapping.NoWrap;
        }

        public override UIElement GetAnimatedItemReference()
        {
            verticalRightPanel.RenderTransform = new CompositeTransform();
            return verticalRightPanel;
        }

        public override ICollection<UIElement> GetNotAnimatedItemsReference()
        {
            return new List<UIElement>() { fullPathTextBlock, image };
        }

        // Returns the URI of the folder icon depending on the user's chosen 
        // color theme
        private Uri getImageUri()
        {
            string path = "";
            if ((Color)Application.Current.Resources["PhoneBackgroundColor"] == Colors.Black)
                path = App.FolderIconLargeWhite;
            else
                path = App.FolderIconLargeWhite;
            return new Uri(path, UriKind.Relative);
        }
    }
}
