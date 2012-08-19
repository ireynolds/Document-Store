﻿using System;
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
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.Utilities;
using Microsoft.Phone.Shell;
using NotepadTheNextVersion.Enumerations;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

namespace NotepadTheNextVersion.ListItems
{
    public partial class DocumentEditor : PhoneApplicationPage
    {
        private Document _doc;
        private TextBox DocTextBox;
        private TextBlock DocTitleBlock;
        private bool _shouldRemoveBackEntry;
        private SolidColorBrush _background;
        private SolidColorBrush _foreground;

        public DocumentEditor()
        {
            InitializeComponent();
        }

        #region Private Helpers

        private InputScope GetTextInputScope()
        {
            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();
            name.NameValue = InputScopeNameValue.Text;
            scope.Names.Add(name);
            return scope;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (_doc.IsTemp && _shouldRemoveBackEntry)
            {
                _doc.Delete(true);
                NavigationService.RemoveBackEntry();
            }
            else
            {
                _doc.Text = DocTextBox.Text;
                _doc.Save();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (_doc == null)
                GetArgs();
            UpdateView();
            DocScrollViewer.Opacity = 0;
            UpdateColors();

            DocScrollViewer.RenderTransform = new CompositeTransform();
            Storyboard s = new Storyboard();
            s.Children.Add(AnimationUtils.FadeIn(100));
            s.Children.Add(AnimationUtils.TranslateY(350, 0, 200));
            Storyboard.SetTarget(s, DocScrollViewer);
            s.Begin();
        }

        private void GetArgs()
        {
            string s;
            if (NavigationContext.QueryString.TryGetValue("param", out s))
            {
                PathStr p = new PathStr(s);
                _doc = new Document(p);
            }
            else
            {
                IList<object> args = ParamUtils.GetArguments();
                _doc = (Document)args[0];
            }
        }

        private void UpdateView()
        {
            if (ApplicationBar == null)
                InitializeAppBar();

            DocScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            DocScrollViewer.VerticalAlignment = VerticalAlignment.Top;
            DocScrollViewer.Height = 800;

            StackPanel DocStackPanel = new StackPanel();
            DocScrollViewer.Content = DocStackPanel;

            this.DocTitleBlock = new TextBlock();
            if (SettingUtils.GetSetting<bool>(Setting.DisplayNoteTitle))
            {
                DocTitleBlock.Text = _doc.DisplayName.ToUpper();
                DocTitleBlock.FontSize = (double)App.Current.Resources["PhoneFontSizeMedium"];
                DocTitleBlock.FontFamily = new FontFamily("Segoe WP Semibold");
                DocTitleBlock.Foreground = new SolidColorBrush(Colors.Gray);
                DocTitleBlock.Margin = new Thickness(12, 12, 0, 10);
                DocTitleBlock.RenderTransform = new CompositeTransform();
                DocTitleBlock.Tap += delegate(object sender, System.Windows.Input.GestureEventArgs e)
                {
                    _doc.NavToRename(NavigationService);
                };
                DocStackPanel.Children.Add(DocTitleBlock);
            }

            this.DocTextBox = new TextBox();
            DocTextBox.BorderThickness = new Thickness(0);
            DocTextBox.AcceptsReturn = true;
            DocTextBox.MinHeight = 700;
            DocTextBox.Text = _doc.Text;
            DocTextBox.SizeChanged += new SizeChangedEventHandler(DocTextBox_SizeChanged);
            DocTextBox.TextWrapping = TextWrapping.Wrap;
            DocTextBox.Margin = new Thickness(-5, 0, -5, 0);
            DocTextBox.InputScope = GetTextInputScope();
            DocTextBox.TextChanged += new TextChangedEventHandler(DocTextBox_TextChanged);
            DocTextBox.GotFocus += new RoutedEventHandler((object sender, RoutedEventArgs e) => { DocTextBox.Background = _background; });
            DocTextBox.LostFocus +=new RoutedEventHandler((object sender, RoutedEventArgs e) => { DocTextBox.Background = _background; });
            
            DocStackPanel.Children.Add(DocTextBox);

            UpdateColors();
        }

        private void UpdateColors()
        {
            if (SettingUtils.GetSetting<ThemeColor>(Setting.NoteEditorThemeColor) != ThemeColor.phone)
            {
                _background = SettingUtils.GetUserSetBackgroundBrush();
                _foreground = SettingUtils.GetUserSetForegroundBrush();

                LayoutRoot.Background = _background;
                DocTitleBlock.Foreground = new SolidColorBrush(Colors.Gray);
                DocTextBox.Background = _background;
                DocTextBox.Foreground = _foreground;
                DocTextBox.CaretBrush = CopyBrush(_foreground);
            }
        }

        private SolidColorBrush CopyBrush(SolidColorBrush brush)
        {
            return new SolidColorBrush(new Color() 
            { 
                A = brush.Color.A,
                R = brush.Color.R, 
                G = brush.Color.G, 
                B = brush.Color.B 
            });
        }

        private void InitializeAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Buttons.Add(Utils.CreateIconButton("folders", App.FolderIconSmall, new EventHandler(FoldersIconButton_Click)));
            ApplicationBar.MenuItems.Add(Utils.CreateMenuItem("settings", new EventHandler(SettingsMenuItem_Click)));
            ApplicationBar.MenuItems.Add(Utils.CreateMenuItem("send as...", new EventHandler(SendAsMenuItem_Click)));
        }

        #endregion

        #region Event Handlers

        private void DocTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _doc.IsTemp = false;
        }

        private void SendAsMenuItem_Click(object sender, EventArgs e)
        {
            _doc.Save();
            ParamUtils.SetArguments(_doc);
            NavigationService.Navigate(App.SendAs);
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(App.Settings);
        }

        private void FoldersIconButton_Click(object sender, EventArgs e)
        {
            _shouldRemoveBackEntry = true;
            ParamUtils.SetArguments(new Directory(_doc.Path.Parent));
            NavigationService.Navigate(App.Listings);
        }

        void DocTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DocTextBox.DesiredSize.Height > 700)
            {
                DocScrollViewer.ScrollToVerticalOffset(DocTextBox.DesiredSize.Height);
            }
        }

        #endregion
    }
}