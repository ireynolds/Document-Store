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

            GetArguments();
            UpdateView();

            ContentBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            NavigationService.RemoveBackEntry();
        }

        #region Private Helpers

        private void GetArguments()
        {
            IList<object> args = ParamUtils.GetArguments();

            _actionables = (IList<IActionable>)args.ElementAt(0);
        }

        private void UpdateView()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Queue<Directory> dirsQ = new Queue<Directory>();
                List<Directory> dirsL = new List<Directory>();
                dirsQ.Enqueue(new Directory(new Path(PathBase.Root)));
                dirsQ.Enqueue(new Directory(new Path(PathBase.Trash)));

                while (dirsQ.Count != 0)
                {
                    Directory dir = dirsQ.Dequeue();

                    if (IsInIgnoreList(dir, _actionables[0]))
                        continue;

                    dirsL.Add(dir);
                    foreach (string subDirName in isf.GetDirectoryNames(System.IO.Path.Combine(dir.Path.PathString, "*")))
                    {
                        dirsQ.Enqueue(new Directory(dir.Path.NavigateIn(subDirName, ItemType.Default)));
                    }
                }

                dirsL.Sort(new IActionableComparer());
                foreach (Directory d in dirsL)
                    ContentBox.Items.Add(createNewDirItem(d));
            }

            if (ContentBox.Items.Count < 6)
                ScrollViewer.SetVerticalScrollBarVisibility(ContentBox, ScrollBarVisibility.Disabled);
        }

        private StackPanel createNewDirItem(Directory d)
        {
            MoveListItem i = new MoveListItem(d);
            return i;
        }

        private bool IsInIgnoreList(Directory dest, IActionable itemToMove)
        {
            bool b = false;
            foreach (IActionable a in _actionables)
                b = b || a.Path.Equals(dest.Path);

            return b || 
                   dest.Path.PathString.Equals("Shared") ||
                   dest.Path.IsInTrash;
        }

        private void TryMoveItem(IActionable a, Directory newLoc)
        {
            try
            {
                a.Move(newLoc);
            }
            catch (ActionableException)
            {
                MessageBoxResult r = MessageBox.Show("There is already a document or directory with the same name at this " +
                    "location. Tap OK to overwrite this item, or Cancel to skip it.", a.DisplayName, MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.OK)
                {
                    (new Directory(newLoc.Path.NavigateIn(a.DisplayName, ItemType.Directory))).Delete();
                    TryMoveItem(a, newLoc);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void ContentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentBox.SelectedIndex == -1)
                return;
            
            Directory newLoc = (ContentBox.SelectedItem as MoveListItem).DirectoryItem;
            ContentBox.SelectedIndex = -1;

            Queue<IActionable> items = new Queue<IActionable>(_actionables);
            while (items.Count != 0)
            {
                IActionable a = items.Dequeue();
                TryMoveItem(a, newLoc);
            }

            NavigationService.GoBack();
        }

        #endregion

    }
}