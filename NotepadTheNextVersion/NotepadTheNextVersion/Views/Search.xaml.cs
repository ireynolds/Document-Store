using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using NotepadTheNextVersion.ListItems;
using System.ComponentModel;
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Enumerations;

namespace NotepadTheNextVersion.ListItems
{
    public partial class Search : PhoneApplicationPage
    {
        private static readonly int RESULTS_TO_DISPLAY = 10;

        private WatermarkedTextBox _searchTermBox;
        private IList<SearchResultListItem> _items;
        private Dictionary<string, List<SearchResultListItem>> _previousResults;
        private Searcher _searcher;
        private ListBox ContentBox;
        private string _lastPattern;
        private IList<Document> _universalScope;

        public Search()
        {
            InitializeComponent();
            _items = new List<SearchResultListItem>();
            _previousResults = new Dictionary<string, List<SearchResultListItem>>();
            this.Loaded += new RoutedEventHandler((object sender, RoutedEventArgs e) => _searchTermBox.Focus());
            _lastPattern = string.Empty;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _universalScope = Utils.GetAllDocuments(PathBase.Root);
            _searcher = new Searcher(_universalScope);
            _searcher.SearchCompleted += new SearchCompletedEventHandler(SearchCompleted);

            if (LayoutRoot.Children.Count == 0)
                UpdateView();
        }

        private void UpdateView()
        {
            StackPanel StaticUIPanel = new StackPanel();
            StaticUIPanel.Margin = new Thickness(12, 0, 12, 0);
            Grid.SetRow(StaticUIPanel, 0);
            LayoutRoot.Children.Add(StaticUIPanel);

            TextBlock PageTitle = new TextBlock()
            {
                Style = (Style)App.Current.Resources["PhoneTextNormalStyle"],
                Margin = new Thickness(12, 0, 0, 0),
                Text = "DOCUMENT SEARCH"
            };
            StaticUIPanel.Children.Add(PageTitle);

            _searchTermBox = new WatermarkedTextBox("search your documents");
            _searchTermBox.InputScope = GetSearchInputScope();
            _searchTermBox.TextChanged += new TextChangedEventHandler(_searchTermBox_TextChanged);
            StaticUIPanel.Children.Add(_searchTermBox);

            ContentBox = new ListBox();
            ContentBox.Margin = new Thickness(12, 0, 12, 0);
            Grid.SetRow(ContentBox, 1);
            LayoutRoot.Children.Add(ContentBox);
        }

        void _searchTermBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_searchTermBox.HasUserSetText)
                return;

            string pattern = _searchTermBox.Text;
            if (StringUtils.EqualsIgnoreCase(_lastPattern, pattern))
                return;
            _lastPattern = pattern;

            if (_previousResults.ContainsKey(pattern))
                SetResultsPane(_previousResults[pattern]);
            else
                _searcher.SetNextSearchPattern(pattern);
        }

        private void SearchCompleted(string pattern, List<SearchResult> results)
        {
            results.Sort();
            List<SearchResultListItem> uiResults = new List<SearchResultListItem>();
            for (int i = 0; i < Math.Min(RESULTS_TO_DISPLAY, results.Count); i++)
                uiResults.Add(new SearchResultListItem(results[i]));
            SetResultsPane(uiResults);
            _previousResults.Add(pattern, uiResults);
        }

        private void SetResultsPane(List<SearchResultListItem> uiResults)
        {
            ContentBox.Items.Clear();
            foreach (SearchResultListItem li in uiResults)
                ContentBox.Items.Add(li);
        }

        private InputScope GetSearchInputScope()
        {
            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();
            name.NameValue = InputScopeNameValue.Search;
            scope.Names.Add(name);
            return scope;
        }

        private IList<Document> ExtractScope(ItemCollection items)
        {
            IList<Document> docs = new List<Document>();
            foreach (SearchResultListItem item in items)
                docs.Add(item.Source);
            return docs;
        }
    }
}