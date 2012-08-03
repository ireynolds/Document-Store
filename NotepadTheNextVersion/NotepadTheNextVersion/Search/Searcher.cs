using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;

namespace NotepadTheNextVersion.Search
{
    public class Searcher
    {
        private Collection<SearchableFile> allFiles;
        private Dictionary<string, KeyValuePair<Collection<SearchableFile>, Collection<SearchResultFile>>> lastSearchResults; // searchTerm => (searchable files, search results)

        private const int SURROUNDING_CHARS = 20;

        public Searcher(string rootDirectory)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                allFiles = getFiles(System.IO.Path.Combine(rootDirectory, "*"), myIsolatedStorage, new ObservableCollection<SearchableFile>());
            }
            lastSearchResults = new Dictionary<string, KeyValuePair<Collection<SearchableFile>, Collection<SearchResultFile>>>();
        }

        private Collection<SearchableFile> getFiles(string pattern, IsolatedStorageFile storeFile, Collection<SearchableFile> list)
        {
            string root = System.IO.Path.GetDirectoryName(pattern);

            foreach (string fileName in storeFile.GetFileNames(pattern))
            {
                string filePath = System.IO.Path.Combine(root, fileName);
                string fileText = string.Empty;
                using (StreamReader myReader = new StreamReader(new IsolatedStorageFileStream(filePath, FileMode.Open, storeFile)))
                {
                    fileText = myReader.ReadToEnd();
                }
                list.Add(new SearchableFile(filePath, fileName, root, storeFile.GetLastWriteTime(filePath), fileText));
            }

            foreach (string dirName in storeFile.GetDirectoryNames(pattern))
            {
                pattern = System.IO.Path.Combine(root, dirName);
                list = getFiles(pattern + "/*", storeFile, list);
            }

            return list;
        }

        public IList<SearchResultFile> search(string searchTerm)
        {
            IList<SearchResultFile> results;

            // find any exact match
            // find a good match
            string LongestStartsWith = FindLongestStartsWith(searchTerm);
            if (lastSearchResults.ContainsKey(searchTerm))
            {
                results = new ObservableCollection<SearchResultFile>();
                KeyValuePair<Collection<SearchableFile>, Collection<SearchResultFile>> pastSearch;
                lastSearchResults.TryGetValue(searchTerm, out pastSearch);
                foreach (SearchResultFile r in pastSearch.Value)
                {
                    results.Add(r);
                }
            }
            else if (!string.IsNullOrEmpty(LongestStartsWith)) // best match
            {
                KeyValuePair<Collection<SearchableFile>, Collection<SearchResultFile>> kvp;
                lastSearchResults.TryGetValue(LongestStartsWith, out kvp);
                InitializeAsPreviousResult(searchTerm);
                results = extendedSearch(searchTerm, LongestStartsWith, kvp.Key, kvp.Value);
            }
            else // no match
            {
                InitializeAsPreviousResult(searchTerm);
                results = search(searchTerm, allFiles);
            }

            return results;
        }

        private void InitializeAsPreviousResult(string searchTerm)
        {
            if (!lastSearchResults.ContainsKey(searchTerm))
                lastSearchResults.Add(searchTerm, new KeyValuePair<Collection<SearchableFile>, Collection<SearchResultFile>>(
                    new Collection<SearchableFile>(), new Collection<SearchResultFile>()));
        }

        private string FindLongestStartsWith(string searchTerm)
        {
            List<LengthString> prevTerms = new List<LengthString>();
            foreach (string prevTerm in lastSearchResults.Keys)
                prevTerms.Add(new LengthString(prevTerm));

            // longest strings before short ones
            prevTerms.Sort();

            // first (and, since sorted, longest) string which satisfies the given condition.
            LengthString ls = prevTerms.FirstOrDefault(s => searchTerm.StartsWith(s.Value));

            if (ls == null)
                return null;
            else
                return ls.Value;
        }

        private ObservableCollection<SearchResultFile> extendedSearch(string searchTerm, string oldSearchTerm, Collection<SearchableFile> searchable, Collection<SearchResultFile> preResults)
        {
            if (searchable.Count != preResults.Count)
                return search(searchTerm, allFiles); // default - search everything

            for (int i = 0; i < searchable.Count; i++)
            {
                SearchableFile sf = searchable.ElementAt(i);
                SearchResultFile sr = preResults.ElementAt(i);
                SearchResultFile postResult = new SearchResultFile(sf);

                foreach (KeyValuePair<int, string[]> hit in sr.hits)
                {
                    int index = hit.Key;

                    // filename hit
                    if (index == -1 && sf.FileName.ToLower().Contains(searchTerm.ToLower()))
                    {
                        int index2 = sf.FileName.ToLower().IndexOf(searchTerm.ToLower());
                        string[] surroundingText = new string[3];
                        surroundingText[0] = sf.FileName.Substring(0, index2);
                        surroundingText[1] = sf.FileName.Substring(index2, searchTerm.Length);
                        surroundingText[2] = sf.FileName.Substring(index2 + searchTerm.Length);
                        postResult.AddHit(-1, surroundingText);
                    }

                    int end = Math.Min(searchTerm.Length, sf.FileText.Length - index);
                    if (index > -1 && sf.FileText.Substring(index, end).ToLower().Equals(searchTerm.ToLower()))
                    {
                        string[] surroundingText = new string[3];
                        surroundingText[0] = hit.Value[0];
                        surroundingText[1] = hit.Value[1] + hit.Value[2].Substring(0, searchTerm.Length - oldSearchTerm.Length);
                        surroundingText[2] = hit.Value[2].Substring(searchTerm.Length - oldSearchTerm.Length);
                        postResult.AddHit(index, surroundingText);
                    }
                }
                if (postResult.GetNumHits() > 0)
                {
                    lastSearchResults[searchTerm].Key.Add(sf);
                    lastSearchResults[searchTerm].Value.Add(postResult);
                }
            }
            return new ObservableCollection<SearchResultFile>(lastSearchResults[searchTerm].Value);
        }

        private ObservableCollection<SearchResultFile> search(string searchTerm, Collection<SearchableFile> filesToSearch)
        {
            searchTerm = searchTerm.ToLower();
            List<SearchResultFile> searchResults = new List<SearchResultFile>();
            foreach (SearchableFile file in filesToSearch)
            {
                SearchResultFile result = null;

                // The filename match will be the first "hit"
                int index = file.FileName.ToLower().IndexOf(searchTerm);
                if (index != -1)
                {
                    result = new SearchResultFile(file);

                    int start = index + searchTerm.Length;
                    string[] surroundingText = new string[3];
                    surroundingText[0] = file.FileName.Substring(0, index);
                    surroundingText[1] = file.FileName.Substring(index, searchTerm.Length);
                    surroundingText[2] = file.FileName.Substring(start, file.FileName.Length - start);

                    result.AddHit(-1, surroundingText); // -1 is a default value. Not meant for display.
                }

                if ((bool)IsolatedStorageSettings.ApplicationSettings["SearchFileTextSetting"])
                {
                    index = file.FileText.IndexOf(searchTerm);
                    while (index != -1)
                    {
                        if (result == null)
                            result = new SearchResultFile(file);

                        string[] surroundingText = GetSurroundingText(result, index, file.FileText, searchTerm);
                        result.AddHit(index, surroundingText);

                        index = file.FileText.IndexOf(searchTerm, index + 1);
                    }
                }

                if (result != null)
                {
                    searchResults.Add(result);

                    lastSearchResults[searchTerm].Key.Add(file);
                    lastSearchResults[searchTerm].Value.Add(result);
                }
            }
            searchResults.Sort();
            return new ObservableCollection<SearchResultFile>(searchResults);
        }

        private string[] GetSurroundingText(SearchResultFile result, int index, string FileText, string searchTerm)
        {
            int start = Math.Max(0, index - SURROUNDING_CHARS);
            int length = Math.Min(SURROUNDING_CHARS, FileText.Length - index - searchTerm.Length);

            string[] surroundingText = new string[3];
            surroundingText[0] = FileText.Substring(start, index - start);
            surroundingText[1] = FileText.Substring(index, searchTerm.Length);
            surroundingText[2] = FileText.Substring(index + searchTerm.Length, length);

            return surroundingText;
        }
    }
}
