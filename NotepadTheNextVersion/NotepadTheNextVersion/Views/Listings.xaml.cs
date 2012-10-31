// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using Microsoft.Phone.Controls;
using NotepadTheNextVersion.Models;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Exceptions;
using NotepadTheNextVersion.AppBars;
using System.Windows.Threading;
using System.Collections;
using System.Linq;

namespace NotepadTheNextVersion.ListItems
{
    public partial class Listings : PhoneApplicationPage
    {
        private Directory _currBeforeTrash;
        private Directory _curr;
        private PageMode _pageMode;
        private IList<IListingsListItem> _items;
        private IList<IListingsListItem> _faves;
        private bool _isShowingEmptyNotice;
        private bool _isShowingLoadingNotice;
        private TimedItemAnimator _animationTimer;
        private ICollection<SelectionChangedEventHandler> _handlers;
        private bool _isNewInstance;

        private StackPanel _pathPanel;
        private Pivot _masterPivot;
        private PivotItem _allPivot;
        public PivotItem _favesPivot;
        private ListBox _allBox;
        private ListBox _favesBox;
        private TextBlock _loadingNotice;
        private TextBlock _emptyNotice;
        private Grid _allGrid;
        private Grid _favesGrid;
        private ScrollViewer _allScrollViewer;
        private ScrollViewer _favesScrollViewer;

        private Grid _currentGrid
        {
            get
            {
                return (_masterPivot.SelectedItem as PivotItem).Content as Grid;
            }
        }

        private ScrollViewer _currentViewer
        {
            get
            {
                return (ScrollViewer)_currentGrid.Children[0];
            }
        }

        public ListBox CurrentBox
        {
            get
            {
                if (_masterPivot.SelectedItem == _allPivot || _masterPivot.SelectedItem == null)
                    return _allBox;
                else if (_masterPivot.SelectedItem == _favesPivot)
                    return _favesBox;
                else
                    throw new Exception("Unrecognized PivotItem");
            }
            set
            {
                _masterPivot.SelectedItem = value;
            }
        }

        public Directory CurrentDirectory
        {
            get
            {
                return new Directory(_curr.Path);
            }
        }

        #region Storyboard constants (in millis)

        private const int SLIDE_X_OUT_DURATION = 150;
        private const int SLIDE_X_IN_DURATION = 150;
        private const int SLIDE_Y_OUT_DURATION = 200;
        private const int SLIDE_Y_IN_DURATION = 200;
        private const int FADE_IN_DURATION = 100;
        private const int FADE_OUT_DURATION = 100;
        private const int SWOOP_DURATION = 200;
        private const int TIMER_DURATION = 60;
        private double _timer_duration;
        private const double DECAY_CONSTANT = 1;
        private static readonly ExponentialEase ITEM_SLIDE_IN_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 };
        private static readonly ExponentialEase ITEM_SLIDE_OUT_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_X_IN_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_X_OUT_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_Y_IN_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_Y_OUT_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };

        #endregion

        public Listings()
        {
            InitializeComponent();
            _items = new List<IListingsListItem>();
            _faves = new List<IListingsListItem>();
            _handlers = new Collection<SelectionChangedEventHandler>();
            Root.RenderTransform = new CompositeTransform();
            Root.Opacity = 0;
            _isNewInstance = true;
            InitializePageUI();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (App.WasTombstoned || _isNewInstance)
            {
                _isNewInstance = false;
                if (_curr == null)
                    GetArgs();
                if (!_curr.Exists())
                {
                    MessageBox.Show("The selected directory could not be found.", "An error occurred", MessageBoxButton.OK);
                    NavigationService.Navigate(App.Listings.AddArg(new Directory(PathBase.Root)));
                }
                _curr = (Directory)_curr.SwapRoot();
            }
            NavigateTo(_curr);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            RemoveNotice(Notice.Empty);
            RemoveNotice(Notice.Loading);
            Root.Opacity = 0;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_animationTimer != null)
                _animationTimer.Stop();
            if (_pageMode == PageMode.Trash)
            {
                SetPageMode(PageMode.View);
                NavigateOut(_currBeforeTrash);
                e.Cancel = true;
            }
            else if (_pageMode == PageMode.Edit)
            {
                SetPageMode(PageMode.View);
                e.Cancel = true;
            }
            else if (_pageMode == PageMode.Favorites)
            {
                _masterPivot.SelectedItem = _allPivot;
                e.Cancel = true;
            }
            else if (_pageMode == PageMode.View)
            {
                PathStr parent = _curr.Path.Parent;
                if (!parent.PathString.Equals(string.Empty))
                {
                    NavigateBack(new Directory(parent));
                    e.Cancel = true;
                }
                else if (_curr.Path.PathString.Equals(new Directory(PathBase.Root).Path.PathString))
                {
                    e.Cancel = true;
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            if (!App.AppSettings.Contains("HasExitedBefore"))
                            {
                                App.AppSettings.Add("HasExitedBefore", true);
                                App.AppSettings.Save();
                                MessageBox.Show("When you press the back key from the root directory, the application will exit.", "Exiting Notepad", MessageBoxButton.OK);
                            }
                            throw new ApplicationMustExitException();
                        });
                }
            }
            else
                throw new Exception("Unrecognized PageMode.");

            base.OnBackKeyPress(e);
        }

        #region Event Handlers

        private void ContentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageMode == PageMode.Trash)
            {
                SyncSelectedItemsWithCheckboxes(e);
            }
            else if (_pageMode == PageMode.Edit)
            {
                SyncSelectedItemsWithCheckboxes(e);
            }
            else if (_pageMode == PageMode.Favorites)
            {
                if (CurrentBox.SelectedIndex == -1)
                    return;
                _masterPivot.SelectedItem = _allPivot;
                var dest = _favesBox.SelectedItem as IListingsListItem;
                if (!dest.ActionableItem.Path.Equals(_curr.Path))
                    Open(dest);
            }
            else if (_pageMode == PageMode.View)
            {
                if (CurrentBox.SelectedIndex == -1)
                    return;
                Open((IListingsListItem)CurrentBox.SelectedItem);
            }
        }

        private void Open(IListingsListItem li)
        {
            CurrentBox.SelectedIndex = -1;
            if (li.GetType() == typeof(DocumentListItem))
            {
                Storyboard sb = GetOutForwardPageSB(li);
                sb.Completed += new EventHandler(
                    (object a, EventArgs b) => li.ActionableItem.Open(NavigationService));
                sb.Begin();
            }
            else
            {
                NavigateIn(li);
            }
        }

        private void _masterPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_masterPivot.SelectedItem == _allPivot)
            {
                SetPageMode(PageMode.View);
            }
            else if (_masterPivot.SelectedItem == _favesPivot)
            {
                UpdateFavesView();
                SetPageMode(PageMode.Favorites);
            }
        }

        #endregion 

        #region Private Helpers

        private void SyncSelectedItemsWithCheckboxes(SelectionChangedEventArgs e)
        {
            foreach (IListingsListItem item in e.AddedItems)
                item.IsChecked = true;
            foreach (IListingsListItem item in e.RemovedItems)
                item.IsChecked = false;
        }

        #region Storyboard Generators

        private Storyboard GetInForwardItemSB(IListingsListItem item)
        {
            Storyboard s = new Storyboard();
            s.Children.Add(AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE, item));
            return s;
        }

        private Storyboard GetInForwardPageSB(Directory destination)
        {
            Storyboard s = new Storyboard();
            TextBlock append = InitPathPanelFromPath(destination.Path.DisplayPathString);
            _pathPanel.Children.Remove(append);
            append.Opacity = 0;
            _pathPanel.Children.Add(append);

            s.Children.Add(AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE, append));
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION, append));

            return s;
        }

        private Storyboard GetInBackwardPageSB()
        {
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, CurrentBox);
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION));
            return s;
        }

        private Storyboard GetInBackwardItemSB(IListingsListItem item)
        {
            Storyboard s = new Storyboard();
            s.Children.Add(AnimationUtils.TranslateX(-500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE, item));
            return s;
        }

        private Storyboard GetOutBackwardPageSB()
        {
            Storyboard s = new Storyboard();
            s.Completed += new EventHandler((object sender, EventArgs e) => _pathPanel.Children.RemoveAt(_pathPanel.Children.Count - 1));
            s.Children.Add(AnimationUtils.TranslateX(0, 500, SLIDE_X_OUT_DURATION, SLIDE_X_OUT_EASE, _pathPanel.Children[_pathPanel.Children.Count - 1]));
            return s;
        }

        private Storyboard GetOutBackwardItemSB(IListingsListItem item)
        {
            Storyboard s = new Storyboard();
            s.Children.Add(AnimationUtils.TranslateX(0, 500, SLIDE_X_OUT_DURATION, SLIDE_X_OUT_EASE, item));
            return s;
        }

        private Storyboard GetOutForwardPageSB(IListingsListItem selectedItem)
        {
            // Swoop selectedItem, fade out
            Storyboard s = new Storyboard();

            // Swoop
            var swoop = AnimationUtils.SwoopSelected(SWOOP_DURATION, selectedItem.GetAnimatedItemReference());
            swoop.Completed += delegate(object sender, EventArgs e)
            {
                selectedItem.Opacity = 0;
            };
            s.Children.Add(swoop);

            // Fade out
            Storyboard fade = new Storyboard();
            foreach (IListingsListItem item in CurrentBox.Items) // ContentBox items
                if (item != selectedItem)
                    fade.Children.Add(AnimationUtils.FadeOut(FADE_OUT_DURATION, item));
            foreach (UIElement item in selectedItem.GetNotAnimatedItemsReference()) // Elements of selectedItem
                fade.Children.Add(AnimationUtils.FadeOut(FADE_OUT_DURATION, item));
            s.Children.Add(fade);

            return s;
        }

        private Storyboard GetNavToSB(Directory openingDirectory)
        {
            Storyboard s = new Storyboard();
            
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION, Root));
            s.Children.Add(AnimationUtils.TranslateY(350, 0, SLIDE_Y_IN_DURATION, SLIDE_Y_IN_EASE, Root));

            InitPathPanelFromPath(openingDirectory.Path.DisplayPathString);

            return s;
        }

        private Storyboard GetNavFromSB()
        {
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, Root);

            s.Children.Add(AnimationUtils.FadeOut(SLIDE_Y_OUT_DURATION));
            s.Children.Add(AnimationUtils.TranslateY(0, 350, SLIDE_Y_OUT_DURATION, SLIDE_Y_OUT_EASE));

            return s;
        }

        private TextBlock InitPathPanelFromPath(string path)
        {
            _pathPanel.Children.Clear();
            string prefix = string.Empty;
            TextBlock last = null;
            foreach (string crumb in path.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                last = CreatePathPanelBlock(prefix + crumb);
                _pathPanel.Children.Add(last);
                prefix = "\\";
            }
            return last;
        }

        private TextBlock CreatePathPanelBlock(string element)
        {
            return new TextBlock()
            {
                Text = element.ToUpper(),
                FontSize = (double)App.Current.Resources["PhoneFontSizeMedium"],
                FontFamily = new FontFamily("Segoe WP Semibold"),
                Margin = new Thickness(0, 0, 0, 0),
                RenderTransform = new CompositeTransform()
            };
        }

        #endregion

        private int GetIndexFromOffset(IList<IListingsListItem> Items)
        {
            double offset = _currentViewer.VerticalOffset;
            int i = 0;
            double height = 0;
            while (i < Items.Count && height < offset)
            {
                IListingsListItem item = Items[i] as IListingsListItem;
                height += item.DesiredSize.Height + item.Margin.Top + item.Margin.Bottom;
                i++;
            }
            return Math.Max(i - 1, 0);
        }

        private void NavigateIn(IListingsListItem selectedItem)
        {
            if (_animationTimer != null)
                _animationTimer.Stop();

            Directory destination = selectedItem.ActionableItem as Directory;
            Storyboard sb = GetOutForwardPageSB(selectedItem);
            sb.Completed += delegate(object sender, EventArgs e)
            {
                CurrentBox.Items.Clear();
                _curr = destination;
                UpdateItems(null);

                int ct = 0;
                _currentViewer.ScrollToVerticalOffset(0);
                GetInForwardPageSB(destination).Begin();
                _animationTimer = new TimedItemAnimator(_items);
                _animationTimer.ForEachItem += delegate(IListingsListItem item)
                {
                    if (ct < 8)
                        GetInForwardItemSB(item).Begin();
                    CurrentBox.Items.Add(item);
                    ct++;
                    _animationTimer.Interval = TimeSpan.FromMilliseconds(1);
                };
                _animationTimer.Completed += delegate(IList<IListingsListItem> itemsNotAdded)
                {
                    foreach (var item in itemsNotAdded)
                        CurrentBox.Items.Add(item);
                };
                _animationTimer.Start();
            };
            sb.Begin();
        }

        private void NavigateBack(Directory destination)
        {
            if (_animationTimer != null)
                _animationTimer.Stop();

            int index = GetAnimationStartIndex();
            var items = new List<IListingsListItem>();
            for (int i = index; i < Math.Min(index + 8, CurrentBox.Items.Count); i++)
                items.Add(CurrentBox.Items[i] as IListingsListItem);

            Storyboard sb = GetOutBackwardPageSB();
            _animationTimer = new TimedItemAnimator(items);
            _animationTimer.ForEachItem += delegate(IListingsListItem item)
            {
                GetOutBackwardItemSB(item).Begin();
            };
            _animationTimer.Completed += delegate(IList<IListingsListItem> itemsNotAdded)
            {
                CurrentBox.Items.Clear();
                _curr = destination;
                UpdateItems(null);

                _currentViewer.ScrollToVerticalOffset(0);
                GetInBackwardPageSB().Begin();
                _animationTimer = new TimedItemAnimator(_items);
                _animationTimer.ForEachItem += delegate(IListingsListItem item)
                {
                    GetInBackwardItemSB(item).Begin();
                    CurrentBox.Items.Add(item);
                };
                _animationTimer.Completed += delegate(IList<IListingsListItem> itemsNotAdded2)
                {
                    foreach (var item in itemsNotAdded2)
                        CurrentBox.Items.Add(item);
                };
                _animationTimer.Start();
            };
            sb.Begin();
            _animationTimer.Start();
        }

        private void NavigateTo(Directory curr)
        {
            if (_animationTimer != null)
                _animationTimer.Stop();

            CurrentBox.Items.Clear();
            Storyboard sb = GetNavToSB(curr);
            sb.Completed += delegate(object sender, EventArgs e)
            {
                _curr = curr;
                UpdateItems(null);

                _currentViewer.ScrollToVerticalOffset(0);
                _animationTimer = new TimedItemAnimator(_items);
                _animationTimer.ForEachItem += delegate(IListingsListItem item)
                {
                    GetInForwardItemSB(item).Begin();
                    CurrentBox.Items.Add(item);
                };
                _animationTimer.Completed += delegate(IList<IListingsListItem> itemsNotAdded)
                {
                    foreach (var item in itemsNotAdded)
                        CurrentBox.Items.Add(item);
                };
                _animationTimer.Start();
            };
            sb.Begin();
        }

        private void NavigateOut(Uri destination)
        {
            NavigationService.Navigate(destination);
        }

        private void NavigateOut(Directory destination)
        {
            Storyboard sb = GetNavFromSB();
            sb.Completed += delegate(object sender, EventArgs e)
            {
                CurrentBox.Items.Clear();
                _curr = destination;
                UpdateItems(null);

                _currentViewer.ScrollToVerticalOffset(0);
                GetNavToSB(destination).Begin();
                foreach (var item in _items)
                {
                    CurrentBox.Items.Add(item);
                }
            };
            sb.Begin();
        }

        private int GetAnimationStartIndex()
        {
            var currPosition = _currentViewer.VerticalOffset;
            var index = -1;
            var height = 0.0;
            while (height < currPosition)
            {
                index++;
                height += (CurrentBox.Items[index] as IListingsListItem).DesiredSize.Height;
            }
            return Math.Max(index, 0);
        }

        private static IList<T> ToList<T>(ItemCollection items)
        {
            IList<T> lst = new List<T>();
            foreach (T item in items)
                lst.Add(item);
            return lst;
        }

        private BackgroundWorker CreateNewBackgroundWorker(Action workToDo, Action Completed)
        {
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (object sender, DoWorkEventArgs e) => { this.Dispatcher.BeginInvoke(workToDo); };
            b.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => { Completed(); };
            return b;
        }

        // Reads information from the incoming UriString to fill class fields
        private void GetArgs()
        {
            string s;
            if (NavigationContext.QueryString.TryGetValue("param", out s))
            {
                PathStr p = new PathStr(s);
                _curr = new Directory(p);
            }
            else
            {
                _curr = (Directory)Utils.CreateActionableFromPath(new PathStr(PathBase.Root));
            }
        }

        // Changes the currently-viewed folder and updates the view
        private void UpdateItems(EventHandler Completed)
        {
            try
            {
                IList<IListingsListItem> Items = new List<IListingsListItem>();

                // Re-fill ContentBox
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    List<string> dirs = new List<string>();
                    try
                    {
                        foreach (string dir in isf.GetDirectoryNames(_curr.Path.PathString + "/*"))
                            if (!dir.StartsWith(".") || SettingUtils.GetSetting<bool>(Setting.ShowHiddenItems))
                                dirs.Add(dir);
                    }
                    catch (System.IO.DirectoryNotFoundException ex)
                    {
                        MessageBox.Show("The specified directory could not be found.", "An error occurred", MessageBoxButton.OK);
                        throw;
                    }
                    dirs.Sort();

                    // Add directories
                    foreach (string dir in dirs)
                    {
                        Directory d = new Directory(_curr.Path.NavigateIn(dir, ItemType.Default));
                        IListingsListItem li = IListingsListItem.CreateListItem(d);
                        Items.Add(li);
                    }

                    List<string> docs = new List<string>();
                    foreach (string doc in isf.GetFileNames(_curr.Path.PathString + "/*"))
                        if (!doc.StartsWith(".") || SettingUtils.GetSetting<bool>(Setting.ShowHiddenItems))
                            docs.Add(doc);
                    docs.Sort();

                    // Add documents
                    foreach (string doc in docs)
                    {
                        Document d = new Document(_curr.Path.NavigateIn(doc, ItemType.Default));
                        IListingsListItem li = IListingsListItem.CreateListItem(d);
                        Items.Add(li);
                    }
                }

                if (_pageMode == PageMode.Edit)
                    SetPageMode(PageMode.View);

                _items = Items;

                Directory trash = new Directory(PathBase.Trash);
                if (_curr.Path.Equals(trash.Path))
                    foreach (var item in _items)
                        item.IsSelectable = true;

                if (_items.Count == 0)
                    ShowNotice(Notice.Empty);
                else
                    RemoveNotice(Notice.Empty);

                if (Completed != null)
                    Completed(null, null);
            }
            catch (Exception ex)
            {
                (new Directory(PathBase.Root)).Open(NavigationService);
            }
        }

        private void UpdateFavesView()
        {
            foreach (string s in SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList))
            {
                var p = new PathStr(s);
                IActionable a = Utils.CreateActionableFromPath(p);
                IListingsListItem item = IListingsListItem.CreateListItem(a);
                _faves.Add(item);
            }

            CurrentBox.Items.Clear();
            foreach (IListingsListItem i in _faves)
                CurrentBox.Items.Add(i);
            _faves.Clear();
        }

        private void ShowNotice(Notice notice)
        {
            if (notice == Notice.Empty)
            {
                RemoveNotice(Notice.Empty);
                _emptyNotice = CreateNoticeBlock(string.Empty);
                _isShowingEmptyNotice = true;
                if (_curr.Path.IsInTrash)
                    _emptyNotice.Text = "Trash is currently empty.";
                else
                    _emptyNotice.Text = notice.GetText();
                _currentGrid.Children.Add(_emptyNotice);
            }
            else if (notice == Notice.Loading)
            {
                RemoveNotice(Notice.Loading);
                _loadingNotice = CreateNoticeBlock(notice.GetText());
                _isShowingLoadingNotice = true;
                _currentGrid.Children.Add(_loadingNotice);
            }
            else
                throw new Exception("Unknown enum type");
        }

        private void RemoveNotice(Notice notice)
        {
            switch (notice)
            {
                case Notice.Empty:
                    _allGrid.Children.Remove(_emptyNotice);
                    _isShowingEmptyNotice = false;
                    break;
                case Notice.Loading:
                    _allGrid.Children.Remove(_loadingNotice);
                    _isShowingLoadingNotice = false;
                    break;
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }

        private TextBlock CreateNoticeBlock(string Text)
        {
            TextBlock tb = new TextBlock()
            {
                Text = Text,
                Foreground = new SolidColorBrush(Colors.Gray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(12, 0, 12, 0),
                FontSize = 24
            };
            return tb;
        }

        private void InitializeApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;
        }

        private void InitializePageUI()
        {
            InitializeApplicationBar();

            _masterPivot = new Pivot();
            _masterPivot.RenderTransform = new CompositeTransform();
            _masterPivot.SelectionChanged += new SelectionChangedEventHandler(_masterPivot_SelectionChanged);
            Grid.SetRow(_masterPivot, 1);
            Root.Children.Add(_masterPivot);

            _pathPanel = new StackPanel();
            Grid.SetRow(_pathPanel, 0);
            _pathPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            _masterPivot.Title = _pathPanel;

            _allPivot = new PivotItem();
            _allPivot.Header = "all";
            _masterPivot.Items.Add(_allPivot);

            _allGrid = new Grid();
            _allPivot.Content = _allGrid;

            _allScrollViewer = new ScrollViewer();
            _allGrid.Children.Add(_allScrollViewer);

            _allBox = new ListBox();
            _allBox.Margin = new Thickness(6, 0, 12, 0);
            _allBox.MinHeight = 500;
            _allBox.VerticalAlignment = VerticalAlignment.Top;
            _allBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _allBox.RenderTransform = new CompositeTransform();
            _allBox.Template = (ControlTemplate)Root.Resources["ItemTemplate"];
            _allScrollViewer.Content = _allBox;
            _masterPivot.SelectedItem = _allPivot; // also sets pagemode

            if (SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList).Count > 0)
            {
                InitializeFavesPivotItem();
            }
        }

        public void InitializeFavesPivotItem()
        {
            _favesPivot = new PivotItem();
            _favesPivot.Header = "favorites";
            _masterPivot.Items.Add(_favesPivot);

            _favesGrid = new Grid();
            _favesPivot.Content = _favesGrid;

            _favesScrollViewer = new ScrollViewer();
            _favesGrid.Children.Add(_favesScrollViewer);

            _favesBox = new ListBox();
            _favesBox.Margin = new Thickness(6, 0, 12, 0);
            _favesBox.MinHeight = 500;
            _favesBox.VerticalAlignment = VerticalAlignment.Top;
            _favesBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _favesBox.RenderTransform = new CompositeTransform();
            _favesBox.Template = (ControlTemplate)Root.Resources["ItemTemplate"];
            Grid.SetRow(_favesBox, 0);
            _favesGrid.Children.Add(_favesBox);
        }

        public void SetPageMode(PageMode type)
        {
            if (type == _pageMode)
                return;
            ResetHandlers(CurrentBox);
            if (type == PageMode.View)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Single;
                ApplicationBar = (new Listings.ViewAppBar(this)).AppBar;
            }
            else if (type == PageMode.Edit)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Multiple;
                ApplicationBar = (new Listings.EditAppBar(this)).AppBar;
            }
            else if (type == PageMode.Trash)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Multiple;
                _currBeforeTrash = _curr;
                ApplicationBar = (new Listings.TrashAppBar(this)).AppBar;
                NavigateOut(new Directory(PathBase.Trash));
            }
            else if (type == PageMode.Favorites)
            {
                CurrentBox.SelectedItem = -1;
                CurrentBox.SelectionMode = SelectionMode.Single;
                ApplicationBar = (new Listings.FavoritesAppBar(this)).AppBar;
            }
            _pageMode = type;
        }

        private void BeginDeleteAnimations(IList<IListingsListItem> deletedItems)
        {
            if (deletedItems.Count == CurrentBox.Items.Count)
            {
                Storyboard s2 = new Storyboard();
                foreach (IListingsListItem item in CurrentBox.Items)
                    s2.Children.Add(AnimationUtils.FadeOut(200, item));
                s2.Begin();
                s2.Completed += (sender, e) =>
                {
                    foreach (var item in deletedItems)
                        CurrentBox.Items.Remove(item);
                };
                ShowNotice(Notice.Empty);
                return;
            }

            var lastDeletedItem = deletedItems[deletedItems.Count - 1];
            var previousItems = new List<IListingsListItem>();
            int i = 0;
            while (i < CurrentBox.Items.Count)
            {
                var item = (IListingsListItem)CurrentBox.Items[i];
                i++;
                if (item != deletedItems[0])
                    previousItems.Add(item);
                if (item == lastDeletedItem)
                    break;
            }

            double height = 0;
            foreach (IListingsListItem item in deletedItems)
            {
                height += item.DesiredSize.Height;
            }

            var s = new Storyboard();
            while (i < CurrentBox.Items.Count)
            {
                var d = AnimationUtils.TranslateY(height, 0, 110, new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 });
                Storyboard.SetTarget(d, (UIElement)CurrentBox.Items[i]);
                s.Children.Add(d);
                ((UIElement)CurrentBox.Items[i]).RenderTransform = new CompositeTransform();
                i++;
            }

            s.Completed += (object sender, EventArgs e) =>
            {
                if (CurrentBox.Items.Count == 0)
                    ShowNotice(Notice.Empty);
            };
            if (_pageMode != PageMode.Trash)
                SetPageMode(PageMode.View);
            if (_pageMode == PageMode.Trash)
            {
                foreach (IListingsListItem item in deletedItems)
                    CurrentBox.Items.Remove(item);
            }
            deletedItems[0].IsSelectableAnimationCompleted += (s2, e2) =>
            {
                foreach (IListingsListItem item in deletedItems)
                    CurrentBox.Items.Remove(item);
                s.Begin();
            };
        }

        private void AddHandler(ListBox box, SelectionChangedEventHandler handler)
        {
            _handlers.Add(handler);
            box.SelectionChanged += handler;
        }

        private void ResetHandlers(ListBox box)
        {
            foreach (var handler in _handlers)
                box.SelectionChanged -= handler;
        }

        #endregion 

        #region AppBar Classes

        private class ViewAppBar : ApplicationBarSetup
        {
            private static ApplicationBarIconButton NewButton;
            private static ApplicationBarIconButton SelectButton;
            private static ApplicationBarIconButton SearchButton;
            private static ApplicationBarMenuItem SettingsItem;
            private static ApplicationBarMenuItem TrashItem;
            private static ApplicationBarMenuItem ImportExportItem;
            private static ApplicationBarMenuItem AboutTipsItem;

            public ViewAppBar(Listings Page)
                : base(Page)
            {
                NewButton = Utils.CreateIconButton("new", App.AddIcon, (object Sender, EventArgs e) =>
                {
                    var c = App.AddNewItem.AddArg(Page._curr);
                    Page.NavigationService.Navigate(c);
                });
                SelectButton = Utils.CreateIconButton("select", App.SelectIcon, (object sender, EventArgs e) => { Invoke(delegate { Page.SetPageMode(PageMode.Edit); }); });
                SearchButton = Utils.CreateIconButton("search", App.SearchIcon, (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.Search); });

                SettingsItem = Utils.CreateMenuItem("settings", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.Settings); });
                TrashItem = Utils.CreateMenuItem("trash", (object sender, EventArgs e) => { Page.SetPageMode(PageMode.Trash); });
                ImportExportItem = Utils.CreateMenuItem("import+export", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.ExportAll); });
                AboutTipsItem = Utils.CreateMenuItem("about+tips", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.AboutAndTips); });

                foreach (IListingsListItem item in Page.CurrentBox.Items)
                    item.IsSelectable = false;

                _buttons = new ButtonList() { NewButton, SelectButton, SearchButton };
                _menuItems = new ItemList() { SettingsItem, TrashItem, ImportExportItem, AboutTipsItem };
                ApplicationBarSetup.SetElements(_appBar, _buttons, _menuItems);
                _appBar.Mode = ApplicationBarMode.Default;
                _appBar.IsMenuEnabled = true;
            }
        }

        private class FavoritesAppBar : ApplicationBarSetup
        {
            public FavoritesAppBar(Listings Page)
                :base(Page)
            {
                _buttons = new ButtonList();
                _menuItems = new ItemList();

                _appBar.Mode = ApplicationBarMode.Minimized;
                _appBar.IsMenuEnabled = false;
            }
        }

        private class EditAppBar : ApplicationBarSetup
        {
            private static ApplicationBarIconButton DeleteButton;
            private static ApplicationBarIconButton FaveButton;
            private static ApplicationBarIconButton UnfaveButton;
            private static ApplicationBarMenuItem RenameItem;
            private static ApplicationBarMenuItem MoveItem;
            private static ApplicationBarMenuItem PinItem;

            private Listings Page;

            private bool _isShowingFaveButton
            {
                get
                {
                    return _appBar.Buttons.Contains(FaveButton);
                }
            }

            public EditAppBar(Listings Page)
                :base(Page)
            {
                this.Page = Page;
                DeleteButton = Utils.CreateIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    Page.BeginDeleteAnimations(deletedItems);
                    if (SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList).Count == 0)
                        Page._masterPivot.Items.Remove(Page._favesPivot);
                });
                FaveButton = Utils.CreateIconButton("add", App.FaveIcon, (object sender, EventArgs e) =>
                {
                    (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem.IsFavorite = true;
                    Page.SetPageMode(PageMode.View);
                    if (Page._favesPivot == null)
                        Page.InitializeFavesPivotItem();
                    else if (!Page._masterPivot.Items.Contains(Page._favesPivot))
                        Page._masterPivot.Items.Add(Page._favesPivot);
                });
                UnfaveButton = Utils.CreateIconButton("remove", App.UnfaveIcon, (object sender, EventArgs e) =>
                {
                    (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem.IsFavorite = false;
                    Page.SetPageMode(PageMode.View);
                    if (SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList).Count == 0)
                        Page._masterPivot.Items.Remove(Page._favesPivot);
                });
                RenameItem = (Utils.CreateMenuItem("rename", (object sender, EventArgs e) =>
                {
                    IActionable a = (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.NavToRename(Page.NavigationService, Page);
                }));
                MoveItem = Utils.CreateMenuItem("move", (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                        args.Add(li.ActionableItem);
                    Page.NavigationService.Navigate(App.MoveItem.AddArgs(args));
                });
                PinItem = Utils.CreateMenuItem("pin", (object sender, EventArgs e) =>
                {
                    IActionable a = (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.TogglePin();
                });

                _buttons = new ButtonList() { DeleteButton, FaveButton };
                _menuItems = new ItemList() { RenameItem, MoveItem, PinItem };
                ApplicationBarSetup.SetElements(_appBar, _buttons, _menuItems);
                ApplicationBarSetup.SetAllEnabled(_appBar, false);
                Page.AddHandler(Page.CurrentBox, new SelectionChangedEventHandler(SelectedItemsChanged));
                _appBar.Mode = ApplicationBarMode.Default;
                _appBar.IsMenuEnabled = true;

                foreach (IListingsListItem item in Page.CurrentBox.Items)
                    item.IsSelectable = true;
            }

            protected override void SelectedItemsChanged(object sender, SelectionChangedEventArgs e)
            {
                base.SelectedItemsChanged(sender, e);
                if (Page.CurrentBox.SelectedItems.Count == 0)
                    Page.SetPageMode(PageMode.View);
                else if (Page.CurrentBox.SelectedItems.Count == 1)
                {
                    IActionable selectedActionable = (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem;
                    SetAllEnabled(_appBar, true);
                    if (selectedActionable.IsFavorite && _appBar.Buttons[1] != UnfaveButton)
                        _appBar.Buttons[1] = UnfaveButton;
                    else if (!selectedActionable.IsFavorite && _appBar.Buttons[1] != FaveButton)
                        _appBar.Buttons[1] = FaveButton;
                    if (selectedActionable.IsPinned)
                        SetEnabledElements(false, new ButtonList(), new ItemList() { PinItem });
                }
                else // multiple selected
                {
                    SetEnabledElements(true, new ButtonList() { DeleteButton }, new ItemList() { MoveItem });
                    SetEnabledElements(false, new ButtonList() { FaveButton, UnfaveButton }, new ItemList() { PinItem, RenameItem });
                }
            }
        }

        private class TrashAppBar : ApplicationBarSetup
        {
            private ApplicationBarIconButton DeleteButton;
            private ApplicationBarIconButton RestoreButton;
            private ApplicationBarMenuItem EmptyItem;

            private Listings Page;

            public TrashAppBar(Listings Page)
                :base(Page)
            {
                this.Page = Page;
                DeleteButton = Utils.CreateIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    if (!ConfirmDelete())
                        return;

                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    Page.BeginDeleteAnimations(deletedItems);
                });
                RestoreButton = Utils.CreateIconButton("restore", App.UndeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                        args.Add(li.ActionableItem);
                    Page.NavigationService.Navigate(App.MoveItem.AddArgs(args));
                });
                EmptyItem = Utils.CreateMenuItem("empty trash", (object sender, EventArgs e) =>
                {
                    if (MessageBoxResult.OK != MessageBox.Show("This will delete all documents in trash permanently.", "Are you sure?", MessageBoxButton.OKCancel))
                        return;

                    foreach (IListingsListItem i in Page.CurrentBox.SelectedItems)
                        i.ActionableItem.Delete();
                    Page.CurrentBox.Items.Clear();
                });

                _buttons = new ButtonList() { DeleteButton, RestoreButton };
                _menuItems = new ItemList() { EmptyItem };
                SetElements(_appBar, _buttons, _menuItems);
                SetAllEnabled(_appBar, false);
                EmptyItem.IsEnabled = true;
                Page.AddHandler(Page.CurrentBox, new SelectionChangedEventHandler(this.SelectedItemChanged));
                _appBar.Mode = ApplicationBarMode.Default;
                _appBar.IsMenuEnabled = true;

                Directory trash = new Directory(PathBase.Trash);
                Storyboard sb = Page.GetNavToSB(trash);
                sb.Completed += new EventHandler((object sender, EventArgs e) =>
                {
                    foreach (IListingsListItem item in Page.CurrentBox.Items)
                        item.IsSelectable = true;
                });
            }

            protected void SelectedItemChanged(object sender, SelectionChangedEventArgs e)
            {
                int ct = Page.CurrentBox.SelectedItems.Count;
                if (ct == 0)
                    SetEnabledElements(false, new ButtonList() { DeleteButton, RestoreButton }, new ItemList());
                else
                    SetAllEnabled(_appBar, true);
            }

            private bool ConfirmDelete()
            {
                var r = MessageBox.Show("The selected items will be deleted permanently.", "Are you sure?", MessageBoxButton.OKCancel);
                return r == MessageBoxResult.OK;
            }
        }

        #endregion
    }
}