// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using System.IO.IsolatedStorage;
using System.IO;
using NotepadTheNextVersion.Utilities;
using System.Windows.Navigation;
using NotepadTheNextVersion.Exceptions;
using NotepadTheNextVersion.Enumerations;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls;

namespace NotepadTheNextVersion.Models
{
    public class LDocument : IActionable, IComparable<LDocument>
    {

        // The path of this
        private readonly PathStr _path;

        // Caches the text of this
        private string _text;

        private bool _isTemp;

        public bool IsFavorite
        {
            get
            {
                var favs = SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList);
                return favs.Contains(this.Path.PathString);
            }
            set
            {
                var favs = SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList);
                if (value == IsFavorite)
                    return;
                
                if (value)
                    favs.Add(this.Path.PathString);
                else
                    favs.Remove(this.Path.PathString);
                App.AppSettings.Save();
            }
        }

        private bool isTrash
        {
            get
            {
                return _path.IsInTrash;
            }
        }

        public bool IsTemp
        {
            get
            {
                return _isTemp;
            }
            set
            {
                _isTemp = value;
            }
        }

        public string Name
        {
            get { return _path.Name; }
        }

        public string DisplayName
        {
            get { return _path.DisplayName; }
        }

        public bool IsPinned
        {
            get
            {
                return Utils.GetTile(Path.PathString) != null;
            }
        }

        public PathStr Path
        {
            get { return new PathStr(_path); }
        }

        public string Text
        {
            get
            {
                // Lazily initialize text
                if (_text == null)
                    _text = GetText();
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public LDocument(PathStr p)
        {
            if (!FileUtils.IsDoc(p.PathString))
                throw new Exception();
            _path = p;
        }

        public LDocument(LDirectory parent, string name)
        {
            if (!FileUtils.IsDoc(name))
                throw new Exception();
            _path = parent.Path.NavigateIn(name, ItemType.Document);
        }

        public void Open(NavigationService NavigationService)
        {
            NavigationService.Navigate(App.DocumentEditor.AddArg(this).AddArg("istemp", IsTemp.ToString()));
        }

        public void NavToMove(NavigationService NavigationService)
        {
            NavigationService.Navigate(App.MoveItem.AddArg(this));
        }

        public IActionable Move(LDirectory newParent)
        {
            var newLocation = new LDocument(newParent.Path.NavigateIn(Name));
            if (FileUtils.DocumentExists(newLocation.Path.PathString))
            {
                MessageBox.Show("A document with the specified name already exists.", "An error occurred", MessageBoxButton.OK);
                return null;
            }

            try
            {
                FileUtils.MoveDocument(Path.PathString, newLocation.Path.PathString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Notepad could not move the document. There may be an existing document at the specified destination.", "An error occurred", MessageBoxButton.OK);
                return null;
            }
            if (IsFavorite)
            {
                this.IsFavorite = false;
                if (!newLocation.isTrash)
                    newLocation.IsFavorite = true;
            }
            if (IsPinned)
                TogglePin();
            return newLocation;
        }

        public void NavToRename(NavigationService NavigationService, PhoneApplicationPage page)
        {
            NavigationService.Navigate(App.RenameItem.AddArg(this)
                                                     .AddArg("istemp", IsTemp.ToString())
                                                     .AddArg("prevpage", page.NavigationService.CurrentSource.OriginalString));
        }

        public IActionable Rename(string newFileName)
        {
            PathStr newLocation = Path.Parent.NavigateIn(newFileName, ItemType.Document);
            if (FileUtils.DocumentExists(newLocation.PathString))
            {
                MessageBox.Show("A document with the specified name already exists.", "An error occurred", MessageBoxButton.OK);
                throw new IsolatedStorageException();
            }

            try
            {
                FileUtils.MoveDocument(Path.PathString, newLocation.PathString);
            }
            catch (IsolatedStorageException ex)
            {
                MessageBox.Show("There may be illegal characters in the specified name.\n\nIf applicable, remove any special characters or punctuation in the name.", "An error occurred", MessageBoxButton.OK);
                throw;
            }
            var newDoc = new LDocument(newLocation);
            if (IsFavorite)
                FileUtils.ReplaceFavorite(this, newDoc);
            if (IsPinned)
                TogglePin();
            return newDoc;
        }

        public IActionable Delete(bool permanently = false)
        {
            if (isTrash || permanently)
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isf.DeleteFile(Path.PathString);
                }
                if (this.IsPinned)
                    this.TogglePin();
                return null;
            }
            else // if (!isTrash)
            {
                LDirectory trash = new LDirectory(new PathStr(PathBase.Trash));
                LDocument newLoc = new LDocument(trash.Path.NavigateIn(Name, ItemType.Default));
                if (newLoc.Exists())
                    newLoc.Delete();

                try
                {
                    this.Move(trash);
                }
                catch
                {
                    return null;
                }
                this.IsFavorite = false;
                if (IsPinned)
                    TogglePin();
                return newLoc;
            }
        }

        public void TogglePin()
        {
            // Import System.Linq to use "extension" methods
            var currTile = Utils.GetTile(Path.PathString);

            if (currTile == null)
            {
                StandardTileData data = new StandardTileData();
                data.Title = this.DisplayName;
                data.BackContent = Text;
                data.BackgroundImage = new Uri(App.DocumentTile, UriKind.Relative);
                ShellTile.Create(App.DocumentEditor.AddArg(this).AddArg("istemp", IsTemp.ToString()), data);
            }
            else
            {
                currTile.Delete();
            }
        }

        public void Save()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream s = isf.OpenFile(Path.PathString, FileMode.Truncate);
                using (StreamWriter sw = new StreamWriter(s))
                    sw.Write(_text);
                s.Close();
            }
        }
        
        public bool Exists()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.FileExists(Path.PathString);
            }
        }

        public int CompareTo(IActionable other)
        {
            if (other.GetType() == typeof(LDirectory))
                return 1;
            else
                return this.Name.CompareTo(other.Name);
        }

        public int CompareTo(LDocument other)
        {
            return CompareTo((IActionable)other);
        }

        #region Private Helpers

        private string GetText()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream s = isf.OpenFile(Path.PathString, FileMode.Open);
                String text = string.Empty;
                using (StreamReader sr = new StreamReader(s))
                {
                    text = sr.ReadToEnd();
                }
                s.Close();
                return text;
            }
        }

        public IActionable SwapRoot()
        {
            LDocument d = new LDocument(Path.UpdateRoot());
            if (this.IsFavorite)
            {
                this.IsFavorite = false;
                d.IsFavorite = true;
            }
            return d;
        }

        #endregion
    }
}
