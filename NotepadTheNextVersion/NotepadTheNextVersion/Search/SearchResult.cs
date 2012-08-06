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
using NotepadTheNextVersion.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace NotepadTheNextVersion.ListItems
{
    public class SearchResult : IComparable<SearchResult>
    {
        private Document _document;
        private IList<Match> _titleMatches;
        private IList<Match> _textMatches;
        private string _pattern;
        private string _sourceText;
        private string _sourceTitle;

        public bool HasMatches
        {
            get
            {
                return HasTextMatches || HasTitleMatches;
            }
        }

        public bool HasTitleMatches
        {
            get
            {
                return _titleMatches != null && _titleMatches.Count != 0;
            }
        }

        public bool HasTextMatches
        {
            get
            {
                return _textMatches != null && _textMatches.Count != 0;
            }
        }

        public string Pattern
        {
            get
            {
                return _pattern;
            }
        }

        public Document Source
        {
            get
            {
                return _document;
            }
        }

        public string SourceText
        {
            get
            {
                return _sourceText;
            }
        }

        public string SourceTitle
        {
            get
            {
                return _sourceTitle;
            }
        }

        public IList<Match> TitleMatches
        {
            get
            {
                if (_titleMatches == null)
                    _titleMatches = new List<Match>();
                return _titleMatches;
            }
        }

        public IList<Match> TextMatches
        {
            get
            {
                if (_textMatches == null)
                    _textMatches = new List<Match>();
                return _textMatches;
            }
        }

        public SearchResult(Document document, string pattern)
        {
            _document = document;
            _pattern = pattern;
        }

        public int CompareTo(SearchResult other)
        {
            int a = other.TitleMatches.Count - this.TitleMatches.Count;
            if (a != 0)
                return a;

            int b = other.TextMatches.Count - this.TextMatches.Count;
            return b;
        }

        internal void SetTitleMatches(string sourceTitle, MatchCollection matches)
        {
            _sourceTitle = sourceTitle;
            _titleMatches = ExtractMatches(matches);
        }

        internal void SetTextMatches(string sourceText, MatchCollection matches)
        {
            _sourceText = sourceText;
            _textMatches = ExtractMatches(matches);
        }

        private IList<Match> ExtractMatches(MatchCollection matches)
        {
            IList<Match> matchesList = new List<Match>();
            foreach (Match m in matches)
                matchesList.Add(m);
            return matchesList;
        }
    }
}
