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
        /// <summary>
        /// Returns false if the name contains any invalid filename characters. The out 
        /// parameter contains a list of the bad chars.
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <param name="badCharsInName">List of invalid chars in name (if any)</param>
        /// <returns></returns>
        public static bool IsValidFileName(string name, out IList<string> badCharsInName)
        {
            badCharsInName = new List<string>();
            foreach (char ch in GetInvalidNameChars())
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
                if (!isf.FileExists(parent.Path.NavigateIn(name).PathString) &&
                    !isf.DirectoryExists(parent.Path.NavigateIn(name).PathString))
                    return name;
                
                int count = 1;
                while (isf.FileExists(parent.Path.NavigateIn(String.Format("{0} ({1})", name, count)).PathString) ||
                       isf.DirectoryExists(parent.Path.NavigateIn(String.Format("{0} ({1})", name, count)).PathString))
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
        /// <param name="path">Must contain the full filepath, including root.</param>
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
                    if (!isf.DirectoryExists(currPath))
                        isf.CreateDirectory(currPath);
                }
                string fileName = pathArray[pathArray.Length - 1];
                IsolatedStorageFileStream f = isf.CreateFile(System.IO.Path.Combine(currPath, fileName));
                f.Close();

                Path p = new Path(PathBase.Root);
                // Skip the first because "root" is already in the path.
                for (int i = 1; i < pathArray.Length; i++)
                    p = p.NavigateIn(pathArray[i]);
                return new Document(p);
            }
        }

        /// <summary>
        /// Returns an array containing all invalid path characters, as specified 
        /// by System.IO.Path.GetInvalidPathChars() unioned with { '/', '\\' }.
        /// </summary>
        /// <returns></returns>
        public static char[] GetInvalidNameChars()
        {
            // Get arrays of all invalid chars
            List<char> invalidPathChars = new List<char>(System.IO.Path.GetInvalidPathChars());
            char[] extraInvalidPathChars = new char[] { '/', '\\' };
            foreach (char c in extraInvalidPathChars)
                invalidPathChars.Add(c);
            return invalidPathChars.ToArray();
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
                        dirsQ.Enqueue(new Directory(dir.Path.NavigateIn(subDirName)));
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
                        docs.Add(new Document(d.Path.NavigateIn(s)));
                return docs;
            }
        }

        /// <summary>
        /// Removes the favorite at the oldPath and adds a favorite at the newPath
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public static void ReplaceFavorite(IActionable oldFave, IActionable newFave)
        {
            oldFave.IsFavorite = false;
            newFave.IsFavorite = true;
        }
    }
}
