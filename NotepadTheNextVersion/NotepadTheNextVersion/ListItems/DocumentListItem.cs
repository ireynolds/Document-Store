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
using System.Collections.Generic;

namespace NotepadTheNextVersion.ListItems
{
    public class DocumentListItem : IListingsListItem
    {
        private static readonly Thickness displayNameTextBox_Margin = new Thickness(10, 0, 0, 25);
        private static readonly FontFamily displayNameTextBox_FontFamily = new FontFamily("Segoe WP SemiLight");

        private TextBlock displayNameTextBlock;

        public DocumentListItem(IActionable a)
            : base(a)
        {
            // Set up appearance
            displayNameTextBlock = new TextBlock();

            this.Children.Add(displayNameTextBlock);

            displayNameTextBlock.Text = a.DisplayName;
            displayNameTextBlock.Margin = displayNameTextBox_Margin;
            displayNameTextBlock.FontFamily = displayNameTextBox_FontFamily;
            displayNameTextBlock.TextWrapping = TextWrapping.Wrap;
            displayNameTextBlock.FontSize = (Double)App.Current.Resources["PhoneFontSizeExtraLarge"];
        }

        public override ICollection<UIElement> GetNotAnimatedItemsReference()
        {
            return new List<UIElement>();
        }

        public override UIElement GetAnimatedItemReference()
        {
            displayNameTextBlock.RenderTransform = new CompositeTransform();
            return displayNameTextBlock;
        }
    }
}
