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
    public static class Utils
    {
        /**
         * Returns true if the given name is a valid filename (not filepath). On exit, the parameter
         * badCharsInName will contain each invalid char in the given name (or none if the given name
         * is valid).
         */
        public static bool IsValidFileName(string name, out IList<string> badCharsInName)
        {
            badCharsInName = new List<string>();
            foreach (char ch in GetInvalidPathChars())
            {
                if (name.Contains("" + ch))
                    badCharsInName.Add("" + ch);
            }
            return badCharsInName.Count == 0;
        }

        public static string GetNumberedName(string name, Directory parent)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                if (!isf.FileExists(parent.Path.NavigateIn(name + ".txt").PathString) &&
                    !isf.DirectoryExists(parent.Path.NavigateIn(name).PathString))
                    return name;
                
                int count = 1;
                while (isf.FileExists(parent.Path.NavigateIn(String.Format("{0} ({1}).txt", name, count)).PathString) ||
                       isf.DirectoryExists(parent.Path.NavigateIn(String.Format("{0} ({1})", name, count)).PathString))
                {
                    count++;
                }
                return String.Format("{0} ({1})", name, count);
            }
        }

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

        public static IActionable createActionableFromPath(Path location)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(location.PathString))
                    return new Document(location);
                else if (isf.DirectoryExists(location.PathString))
                    return new Directory(location);
                else
                    throw new Exception("No such document or directory exists.");
            }
        }

        /**
         * Returns an array containing all invalid path characters, as specified
         * by System.IO.Path.GetInvalidPathChars() and:
         *     
         *     * '/'
         *     * '\\'
         */
        public static char[] GetInvalidPathChars()
        {
            // Get arrays of all invalid chars
            List<char> invalidPathChars = new List<char>(System.IO.Path.GetInvalidPathChars());
            char[] extraInvalidPathChars = new char[] { '/', '\\' };
            foreach (char c in extraInvalidPathChars)
                invalidPathChars.Add(c);

            return invalidPathChars.ToArray();
        }

        // Returns a name which is unique in the current IsolatedStorage scope.
        // CreateDirectory(..) and CreateFile(.. + ".txt") are guaranteed to 
        // create new items successfully. 
        public static string GetUniqueName(Directory parent, IsolatedStorageFile isf)
        {
            string name = string.Empty;
            do
            {
                name = Guid.NewGuid().ToString();
            } while (isf.FileExists(parent.Path.NavigateIn(name).PathString + ".txt") ||
                     isf.DirectoryExists(parent.Path.NavigateIn(name).PathString));
            return name;
        }

        // Sets the arguments to prepare for navigation
        public static void SetArguments(params object[] list)
        {
            if ((App.Current as App).Arguments != null)
                throw new InvalidOperationException("Arguments already non-null");

            (App.Current as App).Arguments = list;
        }

        // Retrieves the arguments from the current navigation
        public static IList<object> GetArguments()
        {
            if ((App.Current as App).Arguments == null)
                throw new InvalidOperationException("Arguments null");

            IList<object> Arguments = (App.Current as App).Arguments;
            (App.Current as App).Arguments = null;
            return Arguments;
        }

        // Creates a new icon button with the given parameters
        public static ApplicationBarIconButton createIconButton(string text, string imagePath, EventHandler e)
        {
            ApplicationBarIconButton b = new ApplicationBarIconButton();
            b.IconUri = new Uri(imagePath, UriKind.Relative);
            b.Text = text;
            b.Click += e;
            return b;
        }

        // Creates a new menu item with the given parameters
        public static ApplicationBarMenuItem createMenuItem(string text, EventHandler e)
        {
            ApplicationBarMenuItem b = new ApplicationBarMenuItem();
            b.Text = text;
            b.Click += e;
            return b;
        }

        // adds " ($i)" to the end of the given name to make it a unique name
        public static string GetUniqueDirectoryName(Path parent, string originalName)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                int i = 1;
                string newName = originalName;
                if (isf.DirectoryExists(parent.NavigateIn(newName).PathString))
                {
                    do
                    {
                        newName = originalName + " (" + i + ")";
                    }
                    while (isf.DirectoryExists(parent.NavigateIn(newName).PathString));
                }
                return newName;
            }
        }

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
                    {
                        dirsQ.Enqueue(new Directory(dir.Path.NavigateIn(subDirName)));
                    }
                }

                return dirsL;
            }
        }

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
    }
}
