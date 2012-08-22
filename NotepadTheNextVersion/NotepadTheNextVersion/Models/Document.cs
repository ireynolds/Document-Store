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

namespace NotepadTheNextVersion.Models
{
    public class Document : IActionable, IComparable<Document>
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
                if (value && !IsFavorite)
                    favs.Add(this.Path.PathString);
                else if (!value && IsFavorite)
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

        public Document(PathStr p)
        {
            if (!FileUtils.IsDoc(p.PathString))
                throw new Exception();
            _path = p;
        }

        public Document(Directory parent, string name)
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

        public IActionable Move(Directory newParent)
        {
            var newLocation = new Document(newParent.Path.NavigateIn(Name));
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
                newLocation.IsFavorite = true;
            return newLocation;
        }

        public void NavToRename(NavigationService NavigationService)
        {
            NavigationService.Navigate(App.RenameItem.AddArg(this).AddArg("istemp", IsTemp.ToString()));
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
            var newDoc = new Document(newLocation);
            if (IsFavorite)
                FileUtils.ReplaceFavorite(this, newDoc);
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
                Directory trash = new Directory(new PathStr(PathBase.Trash));
                Document newLoc = new Document(trash.Path.NavigateIn(Name, ItemType.Default));
                if (newLoc.Exists())
                    newLoc.Delete();

                try
                {
                    this.Move(trash);
                    if (this.IsPinned)
                        this.TogglePin();
                }
                catch
                {
                    return null;
                }
                this.IsFavorite = false;
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
            if (other.GetType() == typeof(Directory))
                return 1;
            else
                return this.Name.CompareTo(other.Name);
        }

        public int CompareTo(Document other)
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
            Document d = new Document(Path.UpdateRoot());
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
