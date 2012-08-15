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
        private bool _shouldRemoveBackEntry;
        private SolidColorBrush _background;
        private SolidColorBrush _foreground;

        public DocumentEditor()
        {
            InitializeComponent();

            GetArgs();
            UpdateView();
            DocScrollViewer.Opacity = 0;
            this.Loaded += new RoutedEventHandler(DocumentEditor_Loaded);
        }

        void DocumentEditor_Loaded(object sender, RoutedEventArgs e)
        {
            DocScrollViewer.RenderTransform = new CompositeTransform();
            Storyboard s = new Storyboard();
            s.Children.Add(AnimationUtils.FadeIn(100));
            s.Children.Add(AnimationUtils.TranslateY(350, 0, 200));
            Storyboard.SetTarget(s, DocScrollViewer);
            s.Begin();
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
                IActionable a = _doc.Delete(); // -> trash
                a.Delete(); // permanent
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
            UpdateColors();
        }

        private void GetArgs()
        {
            IList<object> args = ParamUtils.GetArguments();
            _doc = (Document)args[0];
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

            if ((bool)SettingUtils.GetSetting(Setting.DisplayNoteTitle))
            {
                this.DocTitleBlock = new TextBlock()
                {
                    Text = _doc.DisplayName.ToUpper(),
                    FontSize = (double)App.Current.Resources["PhoneFontSizeMedium"],
                    FontFamily = new FontFamily("Segoe WP Semibold"),
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(12, 12, 0, 10),
                    RenderTransform = new CompositeTransform()
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
            if ((ThemeColor)SettingUtils.GetSetting(Setting.NoteEditorThemeColor) != ThemeColor.phone)
            {
                _background = SettingUtils.GetUserSetBackgroundBrush();
                _foreground = SettingUtils.GetUserSetForegroundBrush();

                LayoutRoot.Background = _background;
                DocTitleBlock.Foreground = _foreground;
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
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

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