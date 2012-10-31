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
            this.Loaded += RenameItem_Loaded;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            Cancel();
            e.Cancel = true;
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetArgs();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
            NewNameBox.GotFocus += new RoutedEventHandler(NewNameBox_GotFocus);
        }

        #region Event Handlers

        void RenameItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= RenameItem_Loaded;
            UpdateView();
            NewNameBox.Focus();
        }

        void NewNameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewNameBox.SelectAll();
        }

        private void IconButton_Okay_Click(object sender, EventArgs e)
        {
            var newName = NewNameBox.Text.Trim();
            IList<string> badChars = new List<string>();

            if (newName.Equals(string.Empty))
            {
                return;
            }
            if (newName.Equals(_actionable.DisplayName))
            {
                NavigateOnSuccess(Utils.CreateActionableFromPath(_actionable.Path));
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
                var r = MessageBox.Show("Items whose names start with '.' are hidden. You can enable showing hidden files in the application's settings.", "Hidden file", MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.Cancel)
                    return;
            }

            try
            {
                var act = _actionable.Rename(newName);
                if (!act.Equals(_actionable))   
                    NavigateOnSuccess(act);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void NavigateOnSuccess(IActionable act)
        {
            var prevPage = GetPreviousPageUri();
            act.IsTemp = false;

            if (prevPage == null)
            {
                Utils.TryGoBack(NavigationService);
            }
            else if ((prevPage.StartsWith(App.AddNewItem.OriginalString) && _actionable is LDocument))
            {
                act.Open(NavigationService);
            }
            else if (prevPage.StartsWith(App.DocumentEditor.OriginalString))
            {
                NavigationService.RemoveBackEntry();
                act.Open(NavigationService);
            }
            else
            {
                Utils.TryGoBack(NavigationService);
            }
        }

        private string GetPreviousPageUri()
        {
            string s;
            NavigationContext.QueryString.TryGetValue("prevpage", out s);
            return s;
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

        private void Cancel()
        {
            var prevPage = GetPreviousPageUri();
            if (prevPage.StartsWith(App.AddNewItem.OriginalString))
                _actionable.Delete(true);
            Utils.TryGoBack(NavigationService);
        }

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
            _actionable = Utils.CreateActionableFromPath(new PathStr(NavigationContext.QueryString["param"]));
            if (NavigationContext.QueryString.ContainsKey("istemp"))
                _actionable.IsTemp = bool.Parse(NavigationContext.QueryString["istemp"]);
        }

        private bool IsUniqueFileName(string name)
        {
            if (_actionable is LDocument)
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