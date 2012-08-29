// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace NotepadTheNextVersion.ListItems
{
    public partial class AboutAndTips : PhoneApplicationPage
    {
        private static Style TitleBlockStyle;
        private static Style DescriptionBlockStyle;

        public AboutAndTips()
        {
            InitializeComponent();
            _masterPivot.SelectionChanged += (s, e) =>
            {
                var pivot = (s as Pivot).SelectedItem as PivotItem;
                if (pivot.Header.Equals("tips"))
                {
                    if (TipsPanel.Children.Count == 0)
                        UpdateTips();
                }
                else if (pivot.Header.Equals("about"))
                {
                    if (AboutPanel.Children.Count == 0)
                        UpdateAbout();
                }
                else if (pivot.Header.Equals("contact"))
                {
                    if (ContactPanel.Children.Count == 0)
                        UpdateContact();
                }
            };
        }

        private void AboutAndTips_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTips();
            UpdateContact();
            UpdateAbout();
        }

        #region Private Helpers

        private void UpdateAbout()
        {
            AboutPanel.Margin = new Thickness(12, 0, 0, 0);

            AboutPanel.Children.Add(new TextBlock()
            {
                Text = "Notepad v5.0.0",
                FontSize = 35,
                FontFamily = new FontFamily("Segoe WP SemiLight"),
                Margin = new Thickness(0, 0, 0, 30)
            });

            var b = new HyperlinkButton();
            b.Content = "Visit Notepad's website.";
            b.Margin = new Thickness(-12, 0, 0, 30);
            b.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            b.FontSize = 26;
            b.Click += (s, e) =>
                {
                    var t = new WebBrowserTask();
                    t.Uri = new Uri("http://github.com/ireynolds/notepad/wiki");
                    t.Show();
                };
            AboutPanel.Children.Add(b);

            AboutPanel.Children.Add(new TextBlock() { Text = "Software Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Front-end Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Quality Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Senior Program Manager: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "President of Having Fun: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock()
            {
                Text = "Isaac, a university student of Computer Science at the University of Washington in Seattle, is the one man in the one-man crew of AppsForMe. He loves the Windows Phone platform, and enjoys software engineering and UI design.",
                Margin = new Thickness(0, 20, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
            AboutPanel.Children.Add(new TextBlock()
            {
                Text = "Notepad is released under the MS-PL (http://opensource.org/licenses/ms-pl).",
                Margin = new Thickness(0, 100, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });

            AboutScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void UpdateContact()
        {
            ContactPanel.Margin = new Thickness(12, 0, 0, 0);

            AddNewTitleBlock(ContactPanel, "questions or complaints");
            AddNewDescriptionBlock(ContactPanel, "If there's something going on with Notepad that confuses or bothers you.");

            AddNewTitleBlock(ContactPanel, "suggestions or how-ya-doin");
            AddNewDescriptionBlock(ContactPanel, "If there's something you'd really love to see from me or Notepad, or if you'd" + 
                " just like to say Hi.");

            HyperlinkButton b1 = new HyperlinkButton();
            b1.Content = "contact me";
            b1.HorizontalAlignment = HorizontalAlignment.Left;
            b1.Margin = new Thickness(-12, 0, 0, 0);
            b1.FontSize = 30;
            b1.Click += new RoutedEventHandler(b1_Click);
            ContactPanel.Children.Add(b1);

            HyperlinkButton b2 = new HyperlinkButton();
            b2.Content = "rate + review";
            b2.HorizontalAlignment = HorizontalAlignment.Left;
            b2.Margin = new Thickness(-12, 30, 0, 0);
            b2.Click += new RoutedEventHandler(b2_Click);
            b2.FontSize = 30;
            ContactPanel.Children.Add(b2);

            ContactScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void UpdateTips()
        {
            TipsPanel.Margin = new Thickness(12, 0, 0, 0);

            AddNewTitleBlock(TipsPanel, "open to document explorer");
            AddNewDescriptionBlock(TipsPanel, "Open the settings screen and enable \"Open to document explorer\". When you open Notepad, you'll skip the document editor page and open to the directory explorer instead.");

            AddNewTitleBlock(TipsPanel, "delete, move, rename");
            AddNewDescriptionBlock(TipsPanel, "While you’re using the document explorer, tap the \"select\" icon in the application bar. Select some items in the list and use the application bar to delete them, move them, or rename them.");

            AddNewTitleBlock(TipsPanel, "pin, favorite");
            AddNewDescriptionBlock(TipsPanel, "In addition to deleting, moving, and renaming items, you can also pin them to the start or add them as favorites so they’re easily accessible from the document explorer.");

            AddNewTitleBlock(TipsPanel, "send as email or sms");
            AddNewDescriptionBlock(TipsPanel, "Use documents as email or sms templates. While you’re editing a document, tap \"send as…\" in the application bar, then choose SMS or email.");
        }

        private void AddNewTitleBlock(StackPanel p, string text)
        {
            if (TitleBlockStyle == null)
            {
                TitleBlockStyle = new Style(typeof(TextBlock));
                TitleBlockStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 35));
                TitleBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                TitleBlockStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Segoe WP SemiLight")));
                TitleBlockStyle.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(0, 0, 0, 0)));
            }

            TextBlock t = new TextBlock();
            t.Text = text;
            t.Style = TitleBlockStyle;
            p.Children.Add(t);
        }

        private void AddNewDescriptionBlock(StackPanel p, string text)
        {
            if (DescriptionBlockStyle == null)
            {
                DescriptionBlockStyle = new Style(typeof(TextBlock));
                DescriptionBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                DescriptionBlockStyle.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(0, 0, 0, 30)));
            }

            TextBlock t = new TextBlock();
            t.Text = text;
            t.Style = DescriptionBlockStyle;
            p.Children.Add(t);
        }

        #endregion

        #region Event Handlers

        void b2_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask t = new MarketplaceReviewTask();
            t.Show();
        }

        void b1_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.To = "appsforme@outlook.com";
            emailTask.Show();
        }

        #endregion


    }
}