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

namespace NotepadTheNextVersion.Models
{
    public class Document : IActionable
    {

        // The path of this
        private readonly Path _path;

        // Caches the text of this
        private string _text;

        private bool _isTemp;

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
            get { return Name.Substring(0, Name.Length - 4); }
        }

        public bool IsPinned
        {
            get { throw new NotImplementedException(); }
        }

        public Path Path
        {
            get { return new Path(_path); }
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

        public Document(Path p)
        {
            String s = p.PathString;

            _path = p;
        }

        public Document(Directory parent, string name)
        {
            _path = parent.Path.NavigateIn(name);
        }

        public void Open(NavigationService NavigationService)
        {
            Utils.SetArguments(this, IsTemp);
            NavigationService.Navigate(App.DocumentEditor);
        }

        public void NavToMove(NavigationService NavigationService)
        {
            Utils.SetArguments(this);
            NavigationService.Navigate(App.MoveItem);
        }

        // Takes the parent of the new location.
        public IActionable Move(Directory newParent)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Path newLoc = newParent.Path.NavigateIn(Name);
                if (isf.FileExists(newLoc.PathString))
                    throw new ActionableException(this);
                    
                isf.MoveFile(_path.PathString, newLoc.PathString);
                return new Document(newLoc);
            }
        }

        public void NavToRename(NavigationService NavigationService)
        {
            Utils.SetArguments(this);
            NavigationService.Navigate(App.RenameItem);
        }

        public IActionable Rename(string newFileName)
        {
            if (!newFileName.EndsWith(".txt"))
                newFileName += ".txt";

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Path newLoc = Path.Parent.NavigateIn(newFileName);
                if (isf.FileExists(newLoc.PathString))
                    throw new ActionableException(this);

                isf.MoveFile(Path.PathString, newLoc.PathString);
                isf.DeleteFile(this.Path.PathString);
                return new Document(newLoc);
            }
        }

        public IActionable Delete()
        {
            if (isTrash)
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isf.DeleteFile(Path.PathString);
                }
                return null;
            }
            else // if (!isTrash)
            {
                Directory trash = new Directory(new Path(PathBase.Trash));
                Document newLoc = new Document(trash.Path.NavigateIn(Name));
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
                data.BackContent = Text;
                data.BackgroundImage = new Uri(App.DirectoryTile, UriKind.Relative);
                ShellTile.Create(App.Listings + "?param=" + Uri.EscapeUriString(Path.PathString), data);
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
                if (!Exists())
                    throw new FileNotFoundException("File does not exist");

                IsolatedStorageFileStream s = isf.OpenFile(Path.PathString, FileMode.Truncate);
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.Write(_text);
                }
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
            return new Document(Path.SwapRoot());
        }

        #endregion
    }
}
