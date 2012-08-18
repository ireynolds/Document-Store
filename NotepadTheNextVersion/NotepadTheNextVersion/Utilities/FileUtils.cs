using System;
using System.Collections.Generic;
using NotepadTheNextVersion.ListItems;
using System.Windows;
using System.Windows.Controls;
using NotepadTheNextVersion.Models;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Media;

namespace NotepadTheNextVersion.Utilities
{
    public static class FileUtils
    {
        public const string DOCUMENT_EXTENSION = ".doc";
        public const string DIRECTORY_EXTENSION = ".dir";

        public static readonly char[] InvalidFileNameChars = { };

        /// <summary>
        /// Creates a document with the given path in IsolatedStorage.
        /// </summary>
        /// <param name="parentPath">Includes extensions.</param>
        /// <param name="name">No extension.</param>
        /// <returns></returns>
        private static Document CreateDocument(string parentPath, string name)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                name = name.Trim() + DOCUMENT_EXTENSION;
                var f = isf.CreateFile(System.IO.Path.Combine(parentPath, name));
                f.Close();
                return new Document(new PathStr(name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentPath">Includes extensions.</param>
        /// <param name="name">No extension.</param>
        /// <returns></returns>
        private static Directory CreateDirectory(string parentPath, string name)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                name = name.Trim() + DIRECTORY_EXTENSION;
                isf.CreateDirectory(System.IO.Path.Combine(parentPath, name));
                return new Directory(new PathStr(name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">No extension.</param>
        /// <returns></returns>
        public static bool DirectoryExists(string path)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                path = path.Trim() + DIRECTORY_EXTENSION;
                return isf.DirectoryExists(path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">No extension.</param>
        /// <returns></returns>
        public static bool DocumentExists(string path)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                path = path.Trim() + DOCUMENT_EXTENSION;
                return isf.FileExists(path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newName">No extension.</param>
        /// <param name="currentName">Includes extension.</param>
        /// <returns></returns>
        public static bool IsUniqueFileName(string newName, string currentName, string parentDirectory)
        {
            if (FileUtils.IsDoc(currentName))
            {
                return !DocumentExists(System.IO.Path.Combine(parentDirectory, newName));
            }
            else if (FileUtils.IsDir(currentName))
            {
                return !DirectoryExists(System.IO.Path.Combine(parentDirectory, newName));
            }
            else
                throw new Exception();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Includes extension.</param>
        /// <returns></returns>
        public static bool IsDoc(string path)
        {
            return System.IO.Path.GetExtension(path).Equals(DOCUMENT_EXTENSION);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Includes extension.</param>
        /// <returns></returns>
        public static bool IsDir(string path)
        {
            return System.IO.Path.GetExtension(path).Equals(DIRECTORY_EXTENSION);
        }

        /// <summary>
        /// Returns false if the name contains any invalid filename characters. The out 
        /// parameter contains a list of the bad chars.
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <param name="badCharsInName">List of invalid chars in name, or null if no errors</param>
        /// <returns></returns>
        public static bool IsValidFileName(string name, out IList<string> badCharsInName)
        {
            badCharsInName = new List<string>();
            foreach (char ch in InvalidFileNameChars)
            {
                if (name.Contains("" + ch))
                    badCharsInName.Add("" + ch);
            }
            return badCharsInName.Count == 0;
        }

        /// <summary>
        /// Returns a new filename, {n}, of the form "name ($i)" such that {n} is not an existing 
        /// directory in parent (parameter) and {n} is not a file in parent (parameter).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static string GetNumberedName(string name, Directory parent)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.FileExists(parent.Path.NavigateIn(name, ItemType.Document).PathString) &&
                    !isf.DirectoryExists(parent.Path.NavigateIn(name, ItemType.Directory).PathString))
                    return name;
                
                int count = 1;
                while (isf.FileExists(parent.Path.NavigateIn(String.Format("{0} ({1})", name, count), ItemType.Document).PathString) ||
                       isf.DirectoryExists(parent.Path.NavigateIn(String.Format("{0} ({1})", name, count), ItemType.Directory).PathString))
                {
                    count++;
                }
                return String.Format("{0} ({1})", name, count);
            }
        }

        /// <summary>
        /// Given a full filepath including root, creates a document at that location relative to root. That is, 
        /// if you pass in root/hello but the current root is home, it will create home.
        /// </summary>
        /// <param name="path">Must contain the full filepath, including root. Must include ".doc", ".dir".</param>
        /// <returns></returns>
        public static Document CreateFileFromString(string path)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] pathArray = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                string currPath = string.Empty;
                // Skip the last because it's the filename
                for (int i = 0; i < pathArray.Length - 1; i++)
                {
                    currPath = System.IO.Path.Combine(currPath, pathArray[i]);
                    if (!isf.DirectoryExists(currPath + ""))
                        isf.CreateDirectory(currPath + "");
                }
                string fileName = pathArray[pathArray.Length - 1];
                IsolatedStorageFileStream f = isf.CreateFile(System.IO.Path.Combine(currPath, fileName));
                f.Close();

                PathStr p = new PathStr(PathBase.Root);
                // Skip the first because "root" is already in the path.
                for (int i = 1; i < pathArray.Length; i++)
                    p = p.NavigateIn(pathArray[i], ItemType.Default);
                return new Document(p);
            }
        }

        /// <summary>
        /// Returns a list of all the directories (recursively) inside the given 
        /// pathbases. 
        /// </summary>
        /// <param name="bases"></param>
        /// <returns></returns>
        public static IList<Directory> GetAllDirectories(params PathBase[] bases)
        {
            if (bases.Length == 0)
                throw new ArgumentException("Zero arguments to GetAllDirectories");

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Queue<Directory> dirsQ = new Queue<Directory>();
                List<Directory> dirsL = new List<Directory>();
                foreach (PathBase b in bases)
                    dirsQ.Enqueue(new Directory(b));

                while (dirsQ.Count != 0)
                {
                    Directory dir = dirsQ.Dequeue();
                    dirsL.Add(dir);
                    foreach (string subDirName in isf.GetDirectoryNames(System.IO.Path.Combine(dir.Path.PathString, "*")))
                        dirsQ.Enqueue(new Directory(dir.Path.NavigateIn(subDirName, ItemType.Default)));
                }
                return dirsL;
            }
        }

        /// <summary>
        /// Returns a list of all the documents (recursively) inside the given pathbases.
        /// </summary>
        /// <param name="bases"></param>
        /// <returns></returns>
        public static IList<Document> GetAllDocuments(params PathBase[] bases)
        {
            if (bases.Length == 0)
                throw new ArgumentException("Zero arguments to GetAllDocuments");

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IList<Document> docs = new List<Document>();
                foreach (Directory d in GetAllDirectories(bases))
                    foreach (string s in isf.GetFileNames(d.Path.PathString + "\\*"))
                        docs.Add(new Document(d.Path.NavigateIn(s, ItemType.Default)));
                return docs;
            }
        }

        /// <summary>
        /// Removes the favorite at the Path and adds a favorite at the newPath
        /// </summary>
        /// <param name="Path2"></param>
        /// <param name="newPath"></param>
        public static void ReplaceFavorite(IActionable oldFave, IActionable newFave)
        {
            oldFave.IsFavorite = false;
            newFave.IsFavorite = true;
        }
    }
}
