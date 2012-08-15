using System;
using System.Collections.Generic;
using NotepadTheNextVersion.Exceptions;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.Utilities;

namespace NotepadTheNextVersion.Models
{
    /*
     * Represents the immutable location of a unique file or directory in
     * IsolatedStorage.
     * 
     * Unless specified otherwise, all methods in Path throw 
     * ArgumentNullExceptions if any argument(s) is (are) null.
     * 
     * @specfields
     *      Base path : Path // Either "root/" or "trash/". Parent directory is the
     *                          IsololatedStorage's root
     */
    public sealed class Path : IEquatable<Path>
    {

        // Representation Invariant:
        // 
        // path != null
        // path.count >= 1
        // for i s.t. 0 <= i < path.count, path[i] != null
        // for i s.t. 0 <= i < path.count, path[i] != string.Empty
        // path[0].Equals("root") || path[0].Equals("trash")

        /*
         * Stores the path hierarchy 
         */
        private readonly IList<string> pathList;

        private bool isInTrash;

        #region Accessors

        /*
         * Returns this Path's path string such that
         * DirectoryExists(Path)
         */
        private string _pathString;
        public string PathString
        {
            get
            {
                if (_pathString == null)
                    _pathString = Combine(pathList, true);
                return _pathString;
            }
        }

        public string DisplayPathString
        {
            get
            {
                string s = Combine(pathList, false);
                return s;
            }
        }

        /*
         * Gets the Path referencing this Path's parent.
         */
        public Path Parent
        {
            get
            {
                if (pathList.Count < 2) return null;

                IList<string> lst = new List<string>();
                for (int i = 0; i < this.pathList.Count - 1; i++)
                    lst.Add(this.pathList[i]);
                bool trash = PathIsInTrash(lst);
                return new Path(lst, trash);
            }
        }

        /*
         * Gets this Path's depth from its base
         * path.
         */
        public int Depth
        {
            get
            {
                return pathList.Count - 1;
            }
        }

        /*
         * Gets this location's name.
         */
        public string Name
        {
            get
            {
                return pathList[pathList.Count - 1];
            }
        }

        public string DisplayName
        {
            get
            {
                return Name.Substring(0, Name.Length - 4);
            }
        }

        public bool IsInTrash
        {
            get 
            { 
                return isInTrash; 
            }
        }

        #endregion


        #region Constructors

        private Path()
        {
            pathList = new List<string>();
        }

        public static Path CreatePathFromString(string s)
        {
            string[] parts = s.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
            Path p = null;
            if (parts[0].Equals("trash-dir"))
                p = new Path(PathBase.Trash);
            else
                p = new Path(PathBase.Root);
            for (int i = 1; i < parts.Length; i++)
                p = p.NavigateIn(parts[i], ItemType.Default);
            return p;
        }

        public Path(Path p)
            : this()
        {
            foreach (string s in p.pathList)
                this.pathList.Add(s);
            this.isInTrash = p.IsInTrash;
        }

        private Path(IList<string> path, bool isTrash)
            : this()
        {
            this.pathList = path;
            this.isInTrash = PathIsInTrash(path);
        }

        public Path(PathBase pathBase)
            : this()
        {
            if (pathBase == PathBase.Root)
            {
                pathList.Add(SettingUtils.GetSetting<string>(Setting.RootDirectoryName));
                this.isInTrash = false;
            }
            else if (pathBase == PathBase.Trash)
            {
                pathList.Add("trash-dir");
                this.isInTrash = false;
            }
        }
        



        #endregion

        #region Public Methods

        /*
         * Returns true if these Paths reference the same directory
         */
        public bool Equals(Path other)
        {
            return PathString.Equals(other.PathString);
        }

        /*
         * Returns a new Path referencing a directory at this:name. 
         * 
         * @requires    : name is a valid URI string
         * @param name  : the path segment to append
         * @throws LocationNotFoundException : if the new path's location
         *                                     doesn't exist
         */
        public Path NavigateIn(string name, ItemType type)
        {
            if (name == null || name.Equals(string.Empty))
                throw new ArgumentException();
            //checkRep();

            Path p = new Path(this);
            if (type == ItemType.Directory)
                p.pathList.Add(name + "-dir");
            else if (type == ItemType.Document)
                p.pathList.Add(name + "-doc");
            else
                p.pathList.Add(name);
            p.isInTrash = PathIsInTrash(p.pathList);

            //checkRep();
            return p;
        }

        /*
         * Returns a string representation of this Path.
         */
        public override string ToString()
        {
            return PathString;
        }

        public Path SwapRoot()
        {
            Path p = new Path(this);
            p.pathList[0] = SettingUtils.GetSetting<string>(Setting.RootDirectoryName);
            return p;
        }

        #endregion

        #region Private Methods

        //
        // Checks this class's representation invariant, and throws an 
        // InvalidOperationException if any invariant isn't fulfilled.
        // 
        private void checkRep()
        {
            if (pathList == null)
                throw new InvalidStateException("path is null");
            if (pathList.Count < 1)
                throw new InvalidStateException("path.count < 1");
            if (pathList.Contains(null))
                throw new InvalidStateException("path.contains(null)");
            if (pathList.Contains(string.Empty))
                throw new InvalidStateException("path.contains(string.empty)");
            if (!pathList[0].Equals(SettingUtils.GetSetting<string>(Setting.RootDirectoryName)) && !pathList[0].Equals("trash-dir"))
                throw new InvalidStateException("root is neither root nor trash");
        }

        #endregion

        #region Static Methods

        //
        // Converts the given list of strings into a valid URI string.
        // 
        public static string Combine(IList<string> pathToCombine, bool IncludeTag)
        {
            string partialPath = string.Empty;

            for (int i = 0; i < pathToCombine.Count; i++)
            {
                string tmp = pathToCombine[i];
                string nextElement = (IncludeTag) ? tmp : tmp.Substring(0, tmp.Length - 4);
                partialPath = System.IO.Path.Combine(partialPath, nextElement);
            }

            return partialPath;
        }

        private static bool PathIsInTrash(IList<string> path)
        {
            return path[0].Equals("trash-dir") &&
                   path.Count > 1;
        }

        #endregion
    }
}
