using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.Utilities;
using System.Collections.Generic;
using System.IO;

namespace NotepadTheNextVersion.Models
{
    public struct PathStr : IEquatable<PathStr>
    {
        private static readonly string[] separators = { "\\", "/" };

        private string _path;

        public string PathString
        {
            get
            {
                return _path;
            }
        }

        public string DisplayPathString
        {
            get
            {
                return Combine(_path.Split(separators, StringSplitOptions.RemoveEmptyEntries), false);
            }
        }

        public PathStr Parent
        {
            get
            {
                return new PathStr(Path.GetDirectoryName(_path));
            }
        }

        public int Depth
        {
            get
            {
                return _path.Split(separators, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            }
        }

        public string Name
        {
            get
            {
                return Path.GetFileName(_path);
            }
        }

        public string DisplayName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_path);
            }
        }

        public bool IsInTrash
        {
            get
            {
                return _path.StartsWith("trash");
            }
        }

        public PathStr(string path)
        {
            _path = path;
        }

        public PathStr(PathStr p)
        {
            _path = p._path;
        }

        public PathStr(PathBase root)
        {
            switch (root)
            {
                case PathBase.Root:
                    _path = SettingUtils.GetSetting<string>(Setting.RootDirectoryName);
                    break;
                case PathBase.Trash:
                    _path = "trash.dir";
                    break;
                default:
                    throw new Exception();
            }
        }

        public PathStr NavigateIn(string name, ItemType type = ItemType.Default)
        {
            if (!Path.HasExtension(name))
                switch (type)
                {
                    case ItemType.Directory:
                        name += ".dir";
                        break;
                    case ItemType.Document:
                        name += ".doc";
                        break;
                }

            return new PathStr(Path.Combine(_path, name));
        }


        public bool Equals(PathStr other)
        {
            return _path.Equals(other._path);
        }

        public override string ToString()
        {
            return DisplayPathString;
        }

        public PathStr UpdateRoot()
        {
            if (IsInTrash)
                return this;

            var newRoot = SettingUtils.GetSetting<string>(Setting.RootDirectoryName);
            var index = _path.IndexOf("\\");

            if (index == -1)
                return new PathStr(newRoot);
            else
            {
                var a = _path.Substring(index + 1);
                return new PathStr(Path.Combine(newRoot, a));
            }
        }

        public static string Combine(IEnumerable<string> Crumbs, bool IncludeExtension)
        {
            var path = string.Empty;
            foreach (var crumb in Crumbs)
            {
                var next = (IncludeExtension) ? Path.GetFileName(crumb) : Path.GetFileNameWithoutExtension(crumb);
                path = Path.Combine(path, next);
            }
            return path;
        }
    }
}
