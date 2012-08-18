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
using Microsoft.Phone.Shell;
using NotepadTheNextVersion.Models;
using System.IO.IsolatedStorage;
using System.IO;
using NotepadTheNextVersion.Utilities;

namespace NotepadTheNextVersion.ListItems
{
    // This page accepts a list of items to rename, and then renames each item individually.
    public partial class RenameItem : PhoneApplicationPage
    {
        public IActionable _actionable;
        public WatermarkedTextBox NewNameBox;

        public RenameItem()
        {
            InitializeComponent();

            GetArgs();
            UpdateView();

            NewNameBox.GotFocus += new RoutedEventHandler(NewNameBox_GotFocus);
            this.Loaded += new RoutedEventHandler(RenameItem_Loaded);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            Cancel();
            e.Cancel = true;
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
        }

        #region Event Handlers

        void RenameItem_Loaded(object sender, RoutedEventArgs e)
        {
            NewNameBox.Focus();
        }

        void NewNameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewNameBox.SelectAll();
        }

        private void Cancel()
        {
            if (_actionable.IsTemp)
                _actionable.Delete();
            NavigationService.GoBack();
        }

        private void IconButton_Okay_Click(object sender, EventArgs e)
        {
            var newName = NewNameBox.Text.Trim();
            IList<string> badChars = new List<string>();

            if (newName.Equals(_actionable.DisplayName))
            {
                NavigateOnSuccess(_actionable);
                return;
            }
            else if (!FileUtils.IsValidFileName(newName, out badChars))
            {
                MessageBox.Show("You used the following invalid characters: " + badChars.ToString(), "Invalid characters", MessageBoxButton.OK);
                return;
            }
            else if (!FileUtils.IsUniqueFileName(newName, _actionable.Name, _actionable.Path.Parent.PathString))
            {
                MessageBox.Show("An item with the same name already exists in that location.\n\nNote that names are case-insensitive.", "Invalid name", MessageBoxButton.OK);
                return;
            }
            else if (newName.Equals("."))
            {
                MessageBox.Show("The name \".\" is an invalid file name.", "Invalid name", MessageBoxButton.OK);
                return;
            }
            else if (newName.StartsWith("."))
            {
                var r = MessageBox.Show("Items whose names start with '.' are hidden. You can disable this feature in settings.", "Hidden file", MessageBoxButton.OKCancel);
                newName = (r == MessageBoxResult.OK) ? newName : newName.Substring(1);
            }

            try
            {
                _actionable = _actionable.Rename(newName);
                NavigateOnSuccess(_actionable);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void NavigateOnSuccess(IActionable act)
        {
            var prevPage = NavigationService.BackStack.ElementAt(NavigationService.BackStack.Count(delegate { return true; }) - 1);
            act.IsTemp = false;
            if (prevPage.Source.Equals(App.Listings))
            {
                NavigationService.GoBack();
            }
            else if (prevPage.Source.Equals(App.DocumentEditor))
            {
                act.Open(NavigationService);
                NavigationService.RemoveBackEntry();
            }
            else if (prevPage.Source.Equals(App.AddNewItem))
            {
                act.Open(NavigationService);
            }
            else
                NavigationService.GoBack();
        }

        private void IconButton_Cancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void NewNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IconButton_Okay_Click(null, null);
            }
        }

        #endregion

        #region Private Helpers

        private void AlertUserBadChars(IList<string> badCharsInName)
        {
            MessageBox.Show("The following characters are invalid for use in names: " + badCharsInName.ToString(),
                "Invalid characters", MessageBoxButton.OK);
        }

        private void AlertUserDuplicateName()
        {
            MessageBox.Show("An item with the same name already exists in that location.\n\nNote that names are case-insensitive.", "Invalid name", MessageBoxButton.OK);
        }

        private void AlertUserDotFile()
        {
            MessageBox.Show("Items whose names start with '.' are hidden. You can disable this feature in settings.", "Dot file", MessageBoxButton.OKCancel);
        }

        private void UpdateView()
        {
            if (ApplicationBar == null)
                CreateAppBar();

            TextBlock tb = new TextBlock();
            tb.Text = "Specify a new name.";
            tb.Margin = new Thickness(12, 0, 0, 0);
            ContentPanel.Children.Add(tb);

            NewNameBox = new WatermarkedTextBox("specify a new name");
            NewNameBox.SetText(_actionable.DisplayName);
            NewNameBox.KeyDown += new KeyEventHandler(NewNameBox_KeyDown);
            ContentPanel.Children.Add(NewNameBox);

            if (_actionable.IsTemp)
            {
                ApplicationTitle.Text = "NEW";
                PageTitle.Text = "new " + _actionable.GetType().Name.ToString().ToLower();
            }
            else
            {
                ApplicationTitle.Text = _actionable.DisplayName.ToUpper();
            }
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.IsVisible = true;

            ApplicationBar.Buttons.Add(Utils.CreateIconButton("okay", App.CheckIcon, new EventHandler(IconButton_Okay_Click)));
            ApplicationBar.Buttons.Add(Utils.CreateIconButton("cancel", App.CancelIcon, new EventHandler(IconButton_Cancel_Click)));
        }

        private void GetArgs()
        {
            IList<object> args = ParamUtils.GetArguments();

            _actionable = (IActionable)args[0];
        }

        private bool IsUniqueFileName(string name)
        {
            if (_actionable is Document)
            {
                Models.PathStr newPath = _actionable.Path.Parent.NavigateIn(name, Enumerations.ItemType.Document);
                return !FileUtils.DocumentExists(newPath.PathString);
            }
            else
            {
                Models.PathStr newPath = _actionable.Path.Parent.NavigateIn(name, Enumerations.ItemType.Directory);
                return !FileUtils.DirectoryExists(newPath.PathString);
            }
        }


        #endregion
    }
}