using System;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Navigation;
using NotepadTheNextVersion.Utilities;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Exceptions;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace NotepadTheNextVersion.Models
{
    public class Directory : IActionable, IComparable<Directory>
    {
        private PathStr _path;


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
                if (value && !IsFavorite)
                    favs.Add(this.Path.PathString);
                else if (!value && IsFavorite)
                    favs.Remove(this.Path.PathString);
                App.AppSettings.Save();
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

        private bool isTrash
        {
            get
            {
                return _path.IsInTrash;
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
            get { return ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(Uri.EscapeUriString(Path.PathString))) != null; }
        }

        public PathStr Path
        {
            get { return new PathStr(_path); }
        }

        public Directory(PathStr p)
        {
            _path = p;
        }

        public Directory(Directory parent, string name)
        {
            _path = parent.Path.NavigateIn(name, ItemType.Directory);
        }

        public Directory(PathBase Base)
            : this(new PathStr(Base)) { }

        public void Open(NavigationService NavigationService)
        {
            ParamUtils.SetArguments(this);
            NavigationService.Navigate(App.Listings);
        }

        public void NavToMove(NavigationService NavigationService)
        {
            ParamUtils.SetArguments(this);
            NavigationService.Navigate(App.MoveItem);
        }

        public IActionable Move(Directory newParent) 
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Directory newLocation = new Directory(newParent.Path.NavigateIn(Name, ItemType.Default));
                if (isf.DirectoryExists(newLocation.Path.PathString))
                    throw new ActionableException(this);

                isf.MoveDirectory(Path.PathString, newLocation.Path.PathString);
                if (this.IsFavorite)
                    newLocation.IsFavorite = true;
                return newLocation;
            }
        }  

        public void NavToRename(NavigationService NavigationService)
        {
            ParamUtils.SetArguments(this);
            NavigationService.Navigate(App.RenameItem);
        }

        public IActionable Rename(string newDirectoryName)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                PathStr newLoc = Path.Parent.NavigateIn(newDirectoryName, ItemType.Directory);
                if (isf.DirectoryExists(newLoc.PathString))
                    throw new ActionableException(this);

                isf.MoveDirectory(Path.PathString, newLoc.PathString);
                isf.DeleteFile(this.Path.PathString);
                Directory newDir = new Directory(newLoc);
                if (this.IsFavorite)
                    FileUtils.ReplaceFavorite(this, newDir);
                return newDir;
            }
        }

        public IActionable Delete()
        {
            if (isTrash)
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    DeleteRecursive(Path, isf);
                }
                return null;
            }
            else // if (!isTrash)
            {
                Directory trash = new Directory(new PathStr(PathBase.Trash));
                Directory newLoc = new Directory(trash.Path.NavigateIn(Name, ItemType.Default));
                if (newLoc.Exists())
                    newLoc.Delete();

                this.Move(trash);
                this.IsFavorite = false;
                return newLoc;
            }
        }

        public void TogglePin()
        {
            // Import System.Linq to use "extension" methods
            ShellTile currTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(Uri.EscapeUriString(Path.PathString)));

            if (currTile == null)
            {
                StandardTileData data = new StandardTileData();
                data.Title = this.DisplayName;
                data.BackgroundImage = new Uri(App.DirectoryTile, UriKind.Relative);
                Uri myUri = App.Listings + "?param=" + Uri.EscapeUriString(Path.PathString); // App.Listings already has ?id= attached in order to create a unique string
                ShellTile.Create(myUri, data);
            }
            else
            {
                currTile.Delete();
            }
        }

        public bool Exists()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.DirectoryExists(Path.PathString);
            }
        }

        public IActionable SwapRoot()
        {
            Directory d = new Directory(Path.UpdateRoot());
            if (this.IsFavorite)
            {
                this.IsFavorite = false;
                d.IsFavorite = true;
            }
            return d;
        }

        public int CompareTo(IActionable other)
        {
            if (other.GetType() == typeof(Document))
                return -1;
            else
                return this.Name.CompareTo(other.Name);
        }

        public int CompareTo(Directory other)
        {
            return CompareTo((IActionable)other);
        }

        #region Private Helpers

        private static void DeleteRecursive(PathStr dir, IsolatedStorageFile isf)
        {
            // Delete every subdirectory's contents recursively
            foreach (string subDir in isf.GetDirectoryNames(dir.PathString + "/*"))
                DeleteRecursive(dir.NavigateIn(subDir, ItemType.Default), isf);
            // Delete every file inside
            foreach (string file in isf.GetFileNames(dir.PathString + "/*"))
                isf.DeleteFile(System.IO.Path.Combine(dir.PathString, file));
            
            isf.DeleteDirectory(dir.PathString);
        }

        #endregion
    }
}
