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
            AboutPanel.Children.Add(new TextBlock() { Text = "New Features Guru: You" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Software Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Front-end Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Quality Engineer: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Senior Program Manager: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock() { Text = "Director of Fun: Isaac Reynolds" });
            AboutPanel.Children.Add(new TextBlock()
            {
                Text = "Isaac, a university student of Computer Science at the University of Washington " +
                    "in Seattle, is the one man in the one-man crew of AppsForMe. He loves the Windows Phone platform, and enjoys " +
                    "software engineering and UI design.",
                Margin = new Thickness(0, 20, 0, 0),
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
            
            AddNewTitleBlock(TipsPanel, "navigate up, not back");
            AddNewDescriptionBlock(TipsPanel, "Open the settings screen and enable \"" +
                "Override back key.\" When you tap the back key while viewing directory listings, yo" +
                "u'll always navigate to the parent directory.");

            AddNewTitleBlock(TipsPanel, "delete, move, rename, pin");
            AddNewDescriptionBlock(TipsPanel, "Navigate to the page of document and directory " + 
                "listings. Press and hold on an item until a context menu pops up.");

            AddNewTitleBlock(TipsPanel, "open to folders list");
            AddNewDescriptionBlock(TipsPanel, "Open the settings screen and enable \"Open to folders " + 
                "list.\" When you open Notepad, you'll skip the note editor screen and open to document" + 
                " listings instead.");

            AddNewTitleBlock(TipsPanel, "send as sms or email");
            AddNewDescriptionBlock(TipsPanel, "Use notes as email/sms draft templates. After saving a note, " + 
                "use the \"send as sms\" or \"send as email\" features. You can find them in the note editor.");

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
            emailTask.To = "appsforme@live.com";
            emailTask.Show();
        }

        #endregion


    }
}