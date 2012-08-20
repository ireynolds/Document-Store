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
        private TextBlock _title;
        private WatermarkedTextBox _searchTermBox;
        private IList<SearchResultListItem> _items;
        private Dictionary<string, List<SearchResultListItem>> _previousResults;
        private Searcher _searcher;
        private ListBox ContentBox;
        private string _lastPattern;
        private IList<Document> _universalScope;
        private TextBlock _emptyNoticeBlock;
        private bool _isShowingEmptyNotice { get { return LayoutRoot.Children.Contains(_emptyNoticeBlock); } }

        public Search()
        {
            InitializeComponent();
            _items = new List<SearchResultListItem>();
            _previousResults = new Dictionary<string, List<SearchResultListItem>>();
            _lastPattern = string.Empty;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _universalScope = FileUtils.GetAllDocuments(new Directory(PathBase.Root));
            _searcher = new Searcher(_universalScope);
            _searcher.SearchCompleted += new SearchCompletedEventHandler(SearchCompleted);

            if (LayoutRoot.Children.Count == 0)
                UpdateView();
            ContentBox.SelectedIndex = -1;
        }

        private void UpdateView()
        {
            StackPanel StaticUIPanel = new StackPanel();
            StaticUIPanel.Margin = new Thickness(12, 0, 12, 0);
            Grid.SetRow(StaticUIPanel, 0);
            LayoutRoot.Children.Add(StaticUIPanel);

            _title = new TextBlock()
            {
                Style = (Style)App.Current.Resources["PhoneTextNormalStyle"],
                Margin = new Thickness(12, 0, 0, 0),
                Text = "DOCUMENT SEARCH"
            };
            StaticUIPanel.Children.Add(_title);

            _searchTermBox = new WatermarkedTextBox("search your documents");
            _searchTermBox.InputScope = GetSearchInputScope();
            _searchTermBox.TextChanged += new TextChangedEventHandler(_searchTermBox_TextChanged);
            _searchTermBox.KeyUp += new KeyEventHandler(_searchTermBox_KeyUp);
            StaticUIPanel.Children.Add(_searchTermBox);

            ContentBox = new ListBox();
            ContentBox.Margin = new Thickness(12, 0, 12, 0);
            ContentBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            Grid.SetRow(ContentBox, 1);
            LayoutRoot.Children.Add(ContentBox);
        }

        private void _searchTermBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _searcher.SetNextSearchPattern(_searchTermBox.Text);
        }

        private void ContentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentBox.SelectedIndex == -1)
                return;
            (ContentBox.SelectedItem as SearchResultListItem).Source.Open(NavigationService);
        }

        private void _searchTermBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pattern = _searchTermBox.Text;
            if (!_searchTermBox.HasUserSetText || 
                StringUtils.EqualsIgnoreCase(_lastPattern, pattern))
                return;
            if (pattern.Length < 2)
            {
                ContentBox.Items.Clear();
                return;
            }

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
            for (int i = 0; i < results.Count; i++)
                uiResults.Add(new SearchResultListItem(results[i]));
            SetResultsPane(uiResults);
            if (!_previousResults.ContainsKey(pattern))
                _previousResults.Add(pattern, uiResults);
        }

        private void DisplayEmptyNotice()
        {
            if (_emptyNoticeBlock == null)
                _emptyNoticeBlock = CreateNoticeBlock("We couldn't find a match. Please try a different search term.");
            if (!_isShowingEmptyNotice)
                LayoutRoot.Children.Add(_emptyNoticeBlock);
            _emptyNoticeBlock.Visibility = Visibility.Visible;
        }

        private void RemoveEmptyNotice()
        {
            if (_isShowingEmptyNotice)
                _emptyNoticeBlock.Visibility = Visibility.Collapsed;
        }

        private TextBlock CreateNoticeBlock(string Text)
        {
            TextBlock tb = new TextBlock()
            {
                Text = Text,
                Foreground = new SolidColorBrush(Colors.Gray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(24, 12, 24, 0),
                FontSize = 24
            };
            Grid.SetRow(tb, 1);
            return tb;
        }

        private void SetResultsPane(List<SearchResultListItem> results)
        {
            ContentBox.Items.Clear();
            foreach (SearchResultListItem li in results)
                ContentBox.Items.Add(li);

            if (results.Count == 0)
                DisplayEmptyNotice();
            else
                RemoveEmptyNotice();
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