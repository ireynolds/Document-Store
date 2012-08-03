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
using NotepadTheNextVersion.Search;
using System.ComponentModel;
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.StaticClasses;
using NotepadTheNextVersion.Enumerations;

namespace NotepadTheNextVersion.Views
{
    public partial class Search : PhoneApplicationPage
    {
        private TextBox _searchTermBox;
        private IList<SearchResultListItem2> _items;
        private Dictionary<string, IList<SearchResultListItem2>> _previousResults;
        private Searcher2 _searcher;
        private ListBox ContentBox;
        private string _lastPattern;
        private IList<Document> _universalScope;

        public Search()
        {
            InitializeComponent();
            _items = new List<SearchResultListItem2>();
            _previousResults = new Dictionary<string, IList<SearchResultListItem2>>();
            this.Loaded += new RoutedEventHandler((object sender, RoutedEventArgs e) => _searchTermBox.Focus());
            _lastPattern = string.Empty;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _universalScope = Utils.GetAllDocuments(PathBase.Root);
            _searcher = new Searcher2(_universalScope);
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

            _searchTermBox = new TextBox();
            _searchTermBox.KeyDown += new KeyEventHandler(_searchTermBox_KeyDown);
            _searchTermBox.TextChanged += new TextChangedEventHandler(_searchTermBox_TextChanged);
            StaticUIPanel.Children.Add(_searchTermBox);

            ContentBox = new ListBox();
            ContentBox.Margin = new Thickness(12, 0, 12, 0);
            Grid.SetRow(ContentBox, 1);
            LayoutRoot.Children.Add(ContentBox);
        }

        void _searchTermBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pattern = _searchTermBox.Text;
            if (StringUtils.EqualsIgnoreCase(_lastPattern, pattern))
                return;

            if (pattern.StartsWithIgnoreCase(_lastPattern))
                if (ContentBox.Items.Count != 0)
                    _searcher.SetScope(ExtractScope(ContentBox.Items));
            else
                _searcher.SetScope(_universalScope);

            _lastPattern = _searchTermBox.Text;

            _searcher.AddToAsyncSearchStack(pattern);
        }

        private void _searchTermBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && !StringUtils.IsNullOrWhitespace(_searchTermBox.Text))
            //{
            //    _searcher.RunSearchAsync(_searchTermBox.Text);
            //}
        }

        private void SearchCompleted(string pattern, List<SearchResult2> results)
        {
            ContentBox.Items.Clear();
            results.Sort();
            foreach (SearchResult2 result in results)
                ContentBox.Items.Add(new SearchResultListItem2(result));
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
            foreach (SearchResultListItem2 item in items)
                docs.Add(item.Source);
            return docs;
        }
    }
}