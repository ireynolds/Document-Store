using System.ComponentModel;
using System.Collections.Generic;
using NotepadTheNextVersion.Models;
using System;
using System.Text.RegularExpressions;

namespace NotepadTheNextVersion.Search
{
    public delegate void SearchCompletedEventHandler(string pattern, List<SearchResult2> results);
    public class Searcher2
    {
        private BackgroundWorker _worker;
        private IList<Document> _scope;
        private static Regex WhitespaceRgx = new Regex("[\r\n\t]+");
        private bool _isSearching;
        private Stack<string> _searchStack;

        public SearchCompletedEventHandler SearchCompleted;

        public bool IsSearching
        {
            get
            {
                return _isSearching;
            }
        }

        public Searcher2(IList<Document> DocumentsToSearch)
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(Search);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ReturnResults);
            _scope = Copy(DocumentsToSearch);
            _searchStack = new Stack<string>();
        }

        public void AddToAsyncSearchStack(string pattern)
        {
            _searchStack.Push(pattern);

            if (!IsSearching)
                _worker.RunWorkerAsync(_searchStack.Pop());
        }

        public void SetScope(IList<Document> scope)
        {
            if (!_isSearching)
                _scope = scope;
        }

        private void Search(object sender, DoWorkEventArgs e)
        {
            _isSearching = true;
            string pattern = (string)e.Argument;
            List<SearchResult2> results = new List<SearchResult2>();
            
            // Search
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Document doc in _scope)
            {
                SearchResult2 result = new SearchResult2(doc, pattern);
                
                string title = doc.DisplayName;
                MatchCollection titleMatches = rgx.Matches(title);
                result.SetTitleMatches(title, titleMatches);

                string text = WhitespaceRgx.Replace(doc.Text, " ");
                MatchCollection textMatches = rgx.Matches(text);
                result.SetTextMatches(text, textMatches);

                if (result.HasMatches)
                    results.Add(result);
            }

            object[] args = new object[] { pattern, results };
            e.Result = args;
        }

        private void ReturnResults(object sender, RunWorkerCompletedEventArgs e)
        {
            string pattern = (string)((object[])e.Result)[0];
            List<SearchResult2> results = (List<SearchResult2>)((object[])e.Result)[1];
            _isSearching = false;
            if (_searchStack.Count != 0)
                _worker.RunWorkerAsync(_searchStack.Pop());
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
