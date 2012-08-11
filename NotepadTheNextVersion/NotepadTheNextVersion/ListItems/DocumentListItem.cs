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
using System.IO.IsolatedStorage;

namespace NotepadTheNextVersion.ListItems
{
    public class DocumentListItem : IListingsListItem
    {
        private static readonly Thickness this_Margin = new Thickness(-6, 0, 0, 0);
        private static readonly Thickness displayNameTextBox_Margin = new Thickness(10, 0, 0, 25);
        private static readonly FontFamily displayNameTextBox_FontFamily = new FontFamily("Segoe WP SemiLight");

        private TextBlock displayNameTextBlock;

        public DocumentListItem(IActionable a)
            : base(a)
        {
            // Set up appearance
            _contentPanel.Margin = this_Margin;

            displayNameTextBlock = new TextBlock();

            _contentPanel.Children.Add(displayNameTextBlock);

            displayNameTextBlock.Text = a.Name;
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

        protected override void InitCheckBox()
        {
            base.InitCheckBox();
            _checkBox.Margin = _checkBox.Margin = new Thickness(0, -18, 0, 0);
        }

        protected override void OverrideHighlightColor()
        {
            SolidColorBrush b = (SolidColorBrush)App.Current.Resources["PhoneForegroundBrush"];
            displayNameTextBlock.Foreground = b;
        }
    }
}
