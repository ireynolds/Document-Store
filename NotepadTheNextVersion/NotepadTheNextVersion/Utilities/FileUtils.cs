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
        /// 
        /// </summary>
        /// <param name="sourcePath">Includes extensions.</param>
        /// <param name="destinationPath">Includes extensions.</param>
        /// <returns></returns>
        public static void MoveDirectory(string sourcePath, string destinationPath)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    isf.MoveDirectory(sourcePath, destinationPath);
                }
                catch (IsolatedStorageException ex)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath">Includes extensions.</param>
        /// <param name="destinationPath">Includes extensions.</param>
        /// <returns></returns>
        public static void MoveDocument(string sourcePath, string destinationPath)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    isf.MoveFile(sourcePath, destinationPath);
                }
                catch (IsolatedStorageException ex)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a document with the given path in IsolatedStorage.
        /// </summary>
        /// <param name="parentPath">Includes extensions.</param>
        /// <param name="name">No extension.</param>
        /// <returns></returns>
        public static Document CreateDocument(string parentPath, string name)
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
        public static Directory CreateDirectory(string parentPath, string name)
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
        /// <param name="path">Include extensions.</param>
        /// <returns></returns>
        public static bool DirectoryExists(string path)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.DirectoryExists(path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Include extensions.</param>
        /// <returns></returns>
        public static bool DocumentExists(string path)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.FileExists(path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newName">No extension.</param>
        /// <param name="currentName">Includes extension.</param>
        /// <param name="parentDirectory">Includes extensions.</param>
        /// <returns></returns>
        public static bool IsUniqueFileName(string newName, string currentName, string parentDirectory)
        {
            if (FileUtils.IsDoc(currentName))
            {
                newName += DOCUMENT_EXTENSION;
                return !DocumentExists(System.IO.Path.Combine(parentDirectory, newName));
            }
            else if (FileUtils.IsDir(currentName))
            {
                newName += DIRECTORY_EXTENSION;
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
        /// 
        /// </summary>
        /// <param name="baseName">No extension.</param>
        /// <param name="parentPath">Includes extensions.</param>
        /// <returns>Includes extensions.</returns>
        public static string GetNumberedDocumentPath(string baseName, string parentPath)
        {
            var original = System.IO.Path.Combine(parentPath, baseName);
            var result = original;
            var ct = 1;
            while (DocumentExists(result + DOCUMENT_EXTENSION))
            {
                result = String.Format(original + " ({0})", ct);
                ct++;
            }
            return result + DOCUMENT_EXTENSION;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseName">No extension.</param>
        /// <param name="parentPath">Includes extensions.</param>
        /// <returns>Includes extensions.</returns>
        public static string GetNumberedDirectoryPath(string baseName, string parentPath)
        {
            var original = System.IO.Path.Combine(parentPath, baseName);
            var result = original;
            var ct = 1;
            while (DirectoryExists(result + DIRECTORY_EXTENSION))
            {
                result = String.Format(original + " ({0})", ct);
                ct++;
            }
            return result + DIRECTORY_EXTENSION;
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
        public static IList<Directory> GetAllDirectories(params Directory[] bases)
        {
            if (bases.Length == 0)
                throw new ArgumentException("Zero arguments to GetAllDirectories");

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Queue<Directory> dirsQ = new Queue<Directory>();
                List<Directory> dirsL = new List<Directory>();
                foreach (var b in bases)
                    dirsQ.Enqueue(b);

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
        public static IList<Document> GetAllDocuments(params Directory[] bases)
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
