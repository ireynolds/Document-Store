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
        private SolidColorBrush _background;
        private SolidColorBrush _foreground;
        private bool _isNewInstance;

        public DocumentEditor()
        {
            InitializeComponent();
            _isNewInstance = true;
            UpdateView();
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
            if (_doc.IsTemp)
            {
                try
                {
                    _doc.Delete(true);
                }
                catch (Exception ex)
                {
                    // if you can't delete the document, that's probably because it already doesn't exist. 
                }
                NavigationService.RemoveBackEntry();
            }
            else
            {
                _doc.Text = DocTextBox.Text;
                TrySave();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (App.WasTombstoned || _isNewInstance)
            {
                _isNewInstance = false;
                if (_doc == null)
                    GetArgs();
                DocTitleBlock.Text = _doc.DisplayName.ToUpper();
                try
                {
                    DocTextBox.Text = _doc.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The specified file could not be opened.", "An error occurred", MessageBoxButton.OK);
                    throw;
                }
            }
            if (App.WasTombstoned)
                _doc.IsTemp = false;
            if (!_doc.Exists())
            {
                _doc.IsTemp = true;
                NavigationService.Navigate(App.Listings.AddArg(new Directory(PathBase.Root)));
                return;
            }
            UpdateColors();
        }

        private void GetArgs()
        {
            _doc = (Document)Utils.CreateActionableFromPath(new PathStr(NavigationContext.QueryString["param"]));
            if (NavigationContext.QueryString.ContainsKey("istemp"))
                _doc.IsTemp = bool.Parse(NavigationContext.QueryString["istemp"]);
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
                DocTitleBlock.FontSize = (double)App.Current.Resources["PhoneFontSizeMedium"];
                DocTitleBlock.FontFamily = new FontFamily("Segoe WP Semibold");
                DocTitleBlock.Foreground = new SolidColorBrush(Colors.Gray);
                DocTitleBlock.Margin = new Thickness(12, 12, 0, 10);
                DocTitleBlock.RenderTransform = new CompositeTransform();
                DocTitleBlock.Tap += delegate(object sender, System.Windows.Input.GestureEventArgs e)
                {
                    _doc.IsTemp = false;
                    _doc.NavToRename(NavigationService, this);
                };
                DocStackPanel.Children.Add(DocTitleBlock);
            }

            this.DocTextBox = new TextBox();
            DocTextBox.BorderThickness = new Thickness(0);
            DocTextBox.AcceptsReturn = true;
            DocTextBox.MinHeight = 700;
            DocTextBox.SizeChanged += new SizeChangedEventHandler(DocTextBox_SizeChanged);
            DocTextBox.TextWrapping = TextWrapping.Wrap;
            DocTextBox.Margin = new Thickness(-5, 0, -5, 0);
            DocTextBox.InputScope = GetTextInputScope();
            DocTextBox.TextChanged += new TextChangedEventHandler(DocTextBox_TextChanged);
            DocTextBox.GotFocus += new RoutedEventHandler((object sender, RoutedEventArgs e) => { DocTextBox.Background = _background; });
            DocTextBox.LostFocus += new RoutedEventHandler((object sender, RoutedEventArgs e) => { DocTextBox.Background = _background; });

            DocStackPanel.Children.Add(DocTextBox);
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

            ApplicationBar.Buttons.Add(Utils.CreateIconButton("save", App.SaveIcon, new EventHandler(Save_IconButton)));
            ApplicationBar.Buttons.Add(Utils.CreateIconButton("folders", App.FolderIconSmall, new EventHandler(FoldersIconButton_Click)));
            ApplicationBar.MenuItems.Add(Utils.CreateMenuItem("settings", new EventHandler(SettingsMenuItem_Click)));
            ApplicationBar.MenuItems.Add(Utils.CreateMenuItem("send as...", new EventHandler(SendAsMenuItem_Click)));
        }

        private void Save_IconButton(object sender, EventArgs e)
        {
            if (TrySave())
                DisplaySuccessNotification();
        }

        private void DisplaySuccessNotification()
        {
            var c = new Canvas();
            c.Background = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            c.Height = 72;
            c.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            c.Opacity = 0;
            c.RenderTransform = new CompositeTransform();
            c.Children.Add(new TextBlock()
            {
                Text = "Document saved successfully",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 23,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(10, 20, 0, 0)
            });
            LayoutRoot.Children.Add(c);

            var sbc = new Storyboard();
            EventHandler completed = (s, e) =>
            {
                sbc.BeginTime = TimeSpan.FromMilliseconds(1000);
                sbc.Children.Add(AnimationUtils.TranslateX(0, 500, 200, new ExponentialEase() { EasingMode = EasingMode.EaseInOut, Exponent = 2 }, c));
                sbc.Children.Add(AnimationUtils.FadeOut(150, c));
                sbc.Begin();
            };
            c.Tap += (s, e) =>
            {
                sbc.Seek(TimeSpan.FromMilliseconds(1000));
            };

            var sb = new Storyboard();
            sb.Children.Add(AnimationUtils.TranslateX(-500, 0, 200, new ExponentialEase() { EasingMode = EasingMode.EaseInOut, Exponent = 2 }, c));
            sb.Children.Add(AnimationUtils.FadeIn(150, c));
            sb.Completed += completed;
            sb.Begin();
        }

        #endregion

        #region Event Handlers

        private void DocTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _doc.IsTemp = false;
        }

        private bool TrySave()
        {
            try
            {
                _doc.IsTemp = false;
                _doc.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("This file could not be saved.", "An error occurred", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void SendAsMenuItem_Click(object sender, EventArgs e)
        {
            if (TrySave())
                NavigationService.Navigate(App.SendAs.AddArg(_doc));
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (TrySave())
                NavigationService.Navigate(App.Settings);
        }

        private void FoldersIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(App.Listings.AddArg(new Directory(new PathStr(_doc.Path.Parent.PathString))));
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