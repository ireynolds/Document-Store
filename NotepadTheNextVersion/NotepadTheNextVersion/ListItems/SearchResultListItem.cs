// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

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
using System.Collections.Generic;

namespace NotepadTheNextVersion.ListItems
{
    public class SearchResultListItem : StackPanel
    {
        private SearchResult _result;
        private TextBlock _textBlock;
        private TextBlock _titleBlock;

        public Document Source
        {
            get
            {
                return _result.Source;
            }
        }

        private static readonly int CHARS_TO_INCLUDE = 1000;
        private static readonly int LINES_TO_DISPLAY = 3;

        public UIElement GetAnimatedElement()
        {
            _titleBlock.RenderTransform = new CompositeTransform();
            return _titleBlock;
        }

        public UIElement GetNonanimatedElement()
        {
            return _textBlock;
        }

        public SearchResultListItem(SearchResult result)
        {
            _result = result;

            this.Orientation = Orientation.Vertical;
            this.Margin = new Thickness(12, 0, 0, 20);

            _titleBlock = new TextBlock() 
            {
                FontSize = 42,
                FontFamily = new FontFamily("Segoe WP SemiLight"),
                Margin = new Thickness(0, 0, 0, 0)
            };
            if (_result.HasTitleMatches)
            {
                foreach (Inline i in GetTitleInlines())
                    _titleBlock.Inlines.Add(i);
            }
            else
            {
                _titleBlock.Text = _result.SourceTitle;
                _titleBlock.Foreground = new SolidColorBrush(Colors.Gray);
            }
            this.Children.Add(_titleBlock);

            _textBlock = new TextBlock()
            {
                Margin = new Thickness(0, -3, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                LineHeight = 23
            };
            _textBlock.MaxHeight = _textBlock.LineHeight * LINES_TO_DISPLAY;
            if (_result.HasTextMatches)
            {
                foreach (Inline i in GetTextInlines())
                    _textBlock.Inlines.Add(i);
            }
            else
            {
                _textBlock.Text = _result.SourceText;
                _textBlock.Foreground = new SolidColorBrush(Colors.Gray);
            }
            this.Children.Add(_textBlock);
        }



        private IList<Inline> GetTextInlines()
        {
            var inlines = new List<Inline>();

            Match firstMatch = _result.TextMatches[0];
            string sourceText = _result.SourceText;

            if (firstMatch.Index >= CHARS_TO_INCLUDE)
            {
                inlines.Add(new Run()
                {
                    Text = sourceText.Substring(0, CHARS_TO_INCLUDE),
                    Foreground = new SolidColorBrush(Colors.Gray)
                });
            }
            else
            {
                var prev = 0;
                var curr = 0;
                var i = 0;
                while (curr <= CHARS_TO_INCLUDE && curr < sourceText.Length && i < _result.TextMatches.Count)
                {
                    var m = _result.TextMatches[i];
                    curr = m.Index;
                    inlines.Add(new Run()
                    {
                        Text = _result.SourceText.Substring(prev, curr - prev),
                        Foreground = new SolidColorBrush(Colors.Gray)
                    });
                    prev = curr;
                    curr += m.Value.Length;
                    inlines.Add(new Run()
                    {
                        Text = m.Value,
                        FontFamily = new FontFamily("Segoe WP SemiBold"),
                        Foreground = (Brush)App.Current.Resources["PhoneForegroundBrush"]
                    });
                    prev = curr;
                    i++;
                }

                if (curr <= CHARS_TO_INCLUDE && curr < sourceText.Length)
                {
                    curr = Math.Min(CHARS_TO_INCLUDE, sourceText.Length);
                    inlines.Add(new Run()
                    {
                        Text = sourceText.Substring(prev, curr - prev),
                        Foreground = new SolidColorBrush(Colors.Gray)
                    });
                }
            }

            return inlines;
        }

        private IList<Inline> GetTitleInlines()
        {
            var inlines = new List<Inline>();
            Match firstMatch = _result.TitleMatches[0];
            string sourceTitle = _result.SourceTitle;
            int startIndex = Math.Max(0, firstMatch.Index - CHARS_TO_INCLUDE);
            string prev = sourceTitle.Substring(startIndex, firstMatch.Index - startIndex);
            inlines.Add(new Run()
            {
                Text = prev,
                Foreground = new SolidColorBrush(Colors.Gray)
            });

            string match = firstMatch.Value;
            inlines.Add(new Run()
            {
                Text = match,
                FontFamily = new FontFamily("Segoe WP SemiBold"),
                Foreground = (Brush)App.Current.Resources["PhoneForegroundBrush"]
            });

            startIndex = firstMatch.Index + firstMatch.Value.Length;
            int length = Math.Min(sourceTitle.Length - startIndex, CHARS_TO_INCLUDE);
            string after = sourceTitle.Substring(startIndex, length);
            inlines.Add(new Run()
            {
                Text = after,
                Foreground = new SolidColorBrush(Colors.Gray)
            });
            return inlines;
        }
    }
}
