using System;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Navigation;
using NotepadTheNextVersion.Utilities;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Exceptions;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Windows;

namespace NotepadTheNextVersion.Models
{
    public class Directory : IActionable
    {
        private Path _path;


        private bool _isTemp;

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
            get { return Name; }
        }

        public bool IsPinned
        {
            get { throw new NotImplementedException(); }
        }

        public Path Path
        {
            get { return new Path(_path); }
        }

        public Directory(Path p)
        {
            _path = p;
        }

        public Directory(Directory parent, string name)
        {
            _path = parent.Path.NavigateIn(name);
        }

        public Directory(PathBase Base)
            : this(new Path(Base)) { }

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

        // takes the parent of the new location
        public IActionable Move(Directory newLocation) 
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Directory newLoc = new Directory(newLocation.Path.NavigateIn(Name));
                if (isf.DirectoryExists(newLoc.Path.PathString))
                    throw new ActionableException(this);

                isf.MoveDirectory(Path.PathString, newLoc.Path.PathString);
                return newLoc;
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
                Path newLoc = Path.Parent.NavigateIn(newDirectoryName);
                if (isf.DirectoryExists(newLoc.PathString))
                    throw new ActionableException(this);

                isf.MoveDirectory(Path.PathString, newLoc.PathString);
                isf.DeleteFile(this.Path.PathString);
                return new Directory(newLoc);
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
                Directory trash = new Directory(new Path(PathBase.Trash));
                Directory newLoc = new Directory(trash.Path.NavigateIn(Name));
                if (newLoc.Exists())
                    newLoc.Delete();

                this.Move(trash);
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
            return new Directory(Path.SwapRoot());
        }

        #region Private Helpers

        private static void DeleteRecursive(Path dir, IsolatedStorageFile isf)
        {
            // Delete every subdirectory's contents recursively
            foreach (string subDir in isf.GetDirectoryNames(dir.PathString + "/*"))
                DeleteRecursive(dir.NavigateIn(subDir), isf);
            // Delete every file inside
            foreach (string file in isf.GetFileNames(dir.PathString + "/*"))
                isf.DeleteFile(System.IO.Path.Combine(dir.PathString, file));
            
            isf.DeleteDirectory(dir.PathString);
        }

        #endregion
    }
}
