using System.ComponentModel;
using System.Collections.Generic;
using NotepadTheNextVersion.Models;
using System;
using System.Text.RegularExpressions;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Enumerations;

namespace NotepadTheNextVersion.ListItems
{
    public delegate void SearchCompletedEventHandler(string pattern, List<SearchResult> results);
    public class Searcher
    {
        private BackgroundWorker _worker;
        private IList<Document> _scope;
        private static Regex WhitespaceRgx = new Regex("[\r\n\t]+");
        private string _next;
        private Dictionary<string, Triple> _pastResults;

        public SearchCompletedEventHandler SearchCompleted;

        public bool IsSearching
        {
            get
            {
                return _worker.IsBusy;
            }
        }

        public Searcher(IList<Document> DocumentsToSearch)
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(Search);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ReturnResults);
            _scope = Copy(DocumentsToSearch);
            _pastResults = new Dictionary<string, Triple>();
        }

        public void SetNextSearchPattern(string pattern)
        {
            _next = pattern;

            if (!IsSearching)
            {
                _worker.RunWorkerAsync(_next);
                _next = null;
            }
        }

        private void SetScope(IList<Document> scope)
        {
            _scope = scope;
        }

        private void Search(object sender, DoWorkEventArgs e)
        {
            string pattern = (string)e.Argument;
            string lastPattern;
            List<SearchResult> results = new List<SearchResult>();
            List<Document> resultScope = new List<Document>();
            
            if (pattern.Equals(string.Empty))
            {
                e.Result = SetResultArgs(pattern, results);
                return;
            }
            else if (_pastResults.ContainsKey(pattern))
            {
                e.Result = SetResultArgs(pattern, _pastResults[pattern].SearchResults);
                return;
            }
            else if (TryGetLastPattern(pattern, out lastPattern))
            {
                SetScope(_pastResults[lastPattern].ResultScope);
            }
            
            // Search
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Document doc in _scope)
            {
                SearchResult result = new SearchResult(doc, pattern);

                string title = doc.Name;
                MatchCollection titleMatches = rgx.Matches(title);
                result.SetTitleMatches(title, titleMatches);

                if ((bool)SettingUtils.GetSetting(Setting.SearchFileText))
                {
                    string text = WhitespaceRgx.Replace(doc.Text, " ");
                    MatchCollection textMatches = rgx.Matches(text);
                    result.SetTextMatches(text, textMatches);
                }

                if (result.HasMatches)
                {
                    results.Add(result);
                    resultScope.Add(doc);
                }
            }

            Triple t = new Triple(resultScope, results);
            _pastResults.Add(pattern, t);
            e.Result = SetResultArgs(pattern, results);
        }

        private object[] SetResultArgs(string pattern, List<SearchResult> results)
        {
            return new object[] { pattern, results };
        }

        private bool TryGetLastPattern(string currentPattern, out string lastPattern)
        {
            lastPattern = string.Empty;
            for (int i = currentPattern.Length - 1; i >= 0; i--)
            {
                string possiblePattern = currentPattern.Substring(0, i);
                if (_pastResults.ContainsKey(possiblePattern))
                {
                    lastPattern = possiblePattern;
                    break;
                }
            }
            return !lastPattern.Equals(string.Empty);
        }

        private void ReturnResults(object sender, RunWorkerCompletedEventArgs e)
        {
            string pattern = (string)((object[])e.Result)[0];
            List<SearchResult> results = (List<SearchResult>)((object[])e.Result)[1];
            if (_next != null)
            {
                _worker.RunWorkerAsync(_next);
                _next = null;
            }
            SearchCompleted(pattern, results);
        }

        private List<Document> Copy(IList<Document> docs)
        {
            List<Document> copy = new List<Document>();
            foreach (Document d in docs)
                copy.Add(d);
            return copy;
        }
    }
}
