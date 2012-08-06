﻿using System;
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
using NotepadTheNextVersion.ListItems;

namespace NotepadTheNextVersion.Models
{
    public struct Triple
    {
        public List<Document> ResultScope;
        public List<SearchResult> SearchResults;

        public Triple(List<Document> ResultScope, List<SearchResult> SearchResults)
        {
            this.ResultScope = ResultScope;
            this.SearchResults = SearchResults;
        }
    }
}
