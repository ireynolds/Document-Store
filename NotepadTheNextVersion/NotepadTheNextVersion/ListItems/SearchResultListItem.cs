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
using System.Text.RegularExpressions;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Models;

namespace NotepadTheNextVersion.ListItems
{
    public class SearchResultListItem : StackPanel
    {
        private SearchResult _result;

        public Document Source
        {
            get
            {
                return _result.Source;
            }
        }

        private static readonly int CHARS_TO_INCLUDE = 40;

        public SearchResultListItem(SearchResult result)
        {
            _result = result;

            this.Orientation = Orientation.Vertical;
            this.Margin = new Thickness(12, 0, 0, 20);

            TextBlock TitleBlock = new TextBlock()
            {
                Text = _result.SourceTitle,
                FontSize = 42,
                FontFamily = new FontFamily("Segoe WP SemiLight"),
                Margin = new Thickness(0, 0, 0, 0)
            };
            this.Children.Add(TitleBlock);

            TextBlock TextBlock = new TextBlock();
            TextBlock.Margin = new Thickness(0, 0, 0, 0);
            if (_result.HasTextMatches)
            {
                foreach (Inline i in GetInlines())
                    TextBlock.Inlines.Add(i);
            }
            else
            {
                TextBlock.Text = _result.SourceText;
            }
            this.Children.Add(TextBlock);
        }

        private Inline[] GetInlines()
        {
            Inline[] inlines = new Inline[3];

            Match firstMatch = _result.TextMatches[0];
            string sourceText = _result.SourceText;

            int startIndex = Math.Max(0, firstMatch.Index - CHARS_TO_INCLUDE);
            string prev = sourceText.Substring(startIndex, firstMatch.Index - startIndex);
            inlines[0] = new Run()
            {
                Text = prev,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            string match = firstMatch.Value;
            inlines[1] = new Run()
            {
                Text = match,
                Foreground = (Brush)App.Current.Resources["PhoneForegroundBrush"]
            };

            startIndex = firstMatch.Index + firstMatch.Value.Length;
            int length = Math.Min(sourceText.Length - startIndex, CHARS_TO_INCLUDE);
            string after = sourceText.Substring(startIndex, length);
            inlines[2] = new Run()
            {
                Text = after,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            return inlines;
        }
    }
}
