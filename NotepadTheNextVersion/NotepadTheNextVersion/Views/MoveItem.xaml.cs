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
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.Enumerations;
using Microsoft.Phone.Shell;
using NotepadTheNextVersion.Exceptions;

namespace NotepadTheNextVersion.ListItems
{ 
    // Given a list of items, moves each item to a common chosen directory. 
    //
    // If there is a name collision, the user is given the option to (a) skip 
    // it or (b) rename it. The list of items to be renamed is passed to the 
    // rename page.
    //
    // Parameters:
    //   IList<IActionable>     A list of items to move.
    public partial class MoveItem : PhoneApplicationPage
    {
        private IList<IActionable> _actionables;

        public MoveItem()
        {
            InitializeComponent();
            this.Loaded += PageLoaded;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void PageLoaded(object sender, EventArgs e)
        {
            this.Loaded -= PageLoaded;
            GetArguments();
            UpdateView();
            ContentBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
        }

        #region Private Helpers

        private void GetArguments()
        {
            var args = new List<IActionable>();
            foreach (var key in NavigationContext.QueryString.Keys)
                args.Add(Utils.CreateActionableFromPath(new PathStr(NavigationContext.QueryString[key])));
            _actionables = args;
        }

        private void UpdateView()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Queue<LDirectory> dirsQ = new Queue<LDirectory>();
                List<LDirectory> dirsL = new List<LDirectory>();
                dirsQ.Enqueue(new LDirectory(new PathStr(PathBase.Root)));
                dirsQ.Enqueue(new LDirectory(new PathStr(PathBase.Trash)));

                while (dirsQ.Count != 0)
                {
                    LDirectory dir = dirsQ.Dequeue();

                    if (IsInIgnoreList(dir, _actionables[0]))
                        continue;

                    dirsL.Add(dir);
                    foreach (string subDirName in isf.GetDirectoryNames(System.IO.Path.Combine(dir.Path.PathString, "*")))
                    {
                        dirsQ.Enqueue(new LDirectory(dir.Path.NavigateIn(subDirName, ItemType.Default)));
                    }
                }

                dirsL.Sort(new IActionableComparer());
                foreach (LDirectory d in dirsL)
                {
                    var item = createNewDirItem(d);
                    item.IsEnabled = !IsParent(d);
                    ContentBox.Items.Add(item);
                }
            }
        }

        private bool IsParent(LDirectory d)
        {
            var parentPath = d.Path.PathString;
            foreach (var item in _actionables)
            {
                if (item.Path.Parent.PathString.Equals(d.Path.PathString))
                    return true;
            }
            return false;
        }

        private MoveListItem createNewDirItem(LDirectory d)
        {
            return new MoveListItem(d);
        }

        private bool IsInIgnoreList(LDirectory dest, IActionable itemToMove)
        {
            bool b = false;
            foreach (IActionable a in _actionables)
                b = b || a.Path.Equals(dest.Path);

            return b || 
                   dest.Path.PathString.Equals("Shared") ||
                   dest.Path.IsInTrash;
        }

        private void TryMoveItem(IActionable a, LDirectory newParent)
        {
            var newLocation = Utils.CreateActionableFromPath(new PathStr(newParent.Path.NavigateIn(a.Name)));
            if (newLocation.Exists())
            {
                var type = a.GetType().Name.ToLower();
                MessageBoxResult r = MessageBox.Show(string.Format("There is already an {0} with the same name at the specified destination. Tap OK to overwrite the existing {0}, or Cancel to skip this item.", type), a.DisplayName, MessageBoxButton.OKCancel);
                if (r != MessageBoxResult.OK)
                    return;
                IActionable d = newLocation.Delete(true);
                if (d.Equals(newLocation))
                    return;
            }
            try
            {
                a.Move(newParent);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #endregion

        #region Event Handlers

        private void ContentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentBox.SelectedIndex == -1)
                return;
            if (!(ContentBox.SelectedItem as MoveListItem).IsEnabled)
                return;

            LDirectory newLoc = (ContentBox.SelectedItem as MoveListItem).DirectoryItem;
            ContentBox.SelectedIndex = -1;

            Queue<IActionable> items = new Queue<IActionable>(_actionables);
            while (items.Count != 0)
            {
                IActionable a = items.Dequeue();
                TryMoveItem(a, newLoc);
            }

            Utils.TryGoBack(NavigationService);
        }

        #endregion

    }
}