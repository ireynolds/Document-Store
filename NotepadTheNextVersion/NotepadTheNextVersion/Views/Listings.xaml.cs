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

namespace NotepadTheNextVersion.ListItems
{
    public partial class Listings : PhoneApplicationPage
    {
        private Directory _currBeforeTrash;
        private Directory _curr;
        private PageMode _pageMode;
        private IList<IListingsListItem> _items;
        private IList<IListingsListItem> _faves;
        private bool _isShowingEmptyNotice { get { return _currentGrid.Children.Contains(_emptyNotice); } }
        private bool _isShowingLoadingNotice { get { return _currentGrid.Children.Contains(_loadingNotice); } }

        private StackPanel _pathPanel;
        private Pivot _masterPivot;
        private PivotItem _allPivot;
        private PivotItem _favesPivot;
        private ListBox _allBox;
        private ListBox _favesBox;
        private TextBlock _loadingNotice;
        private TextBlock _emptyNotice;
        private Grid _allGrid;
        private Grid _favesGrid;

        private Grid _currentGrid
        {
            get
            {
                return (Grid)((PivotItem)_masterPivot.SelectedItem).Content;
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
        private const int SLIDE_Y_IN_DURATION = 300;
        private const int FADE_IN_DURATION = 100;
        private const int FADE_OUT_DURATION = 100;
        private const int SWOOP_DURATION = 250;
        private static readonly ExponentialEase SLIDE_X_IN_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_X_OUT_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_Y_IN_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 };
        private static readonly ExponentialEase SLIDE_Y_OUT_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };

        #endregion

        public Listings()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Listings_Loaded);
            _items = new List<IListingsListItem>();
            _faves = new List<IListingsListItem>();
            Root.RenderTransform = new CompositeTransform();

            InitializeApplicationBar();
            InitializePageUI();
            SetPageMode(PageMode.View);

            Root.Opacity = 0;
        }

        void Listings_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateTo(_curr);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_curr == null)
                GetArgs();
            _curr = (Directory)_curr.SwapRoot();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_pageMode == PageMode.Trash)
            {
                SetPageMode(PageMode.View);
                NavigateFrom(_currBeforeTrash);
                e.Cancel = true;
            }
            else if (_pageMode == PageMode.Edit)
            {
                SetPageMode(PageMode.View);
                e.Cancel = true;
            }
            else if (_pageMode == PageMode.View)
            {
                Path parent = _curr.Path.Parent;
                if (parent != null)
                {
                    NavigateBack(new Directory(parent));
                    e.Cancel = true;
                }
            }
            else if (_pageMode == PageMode.Favorites) 
            {
                _masterPivot.SelectedItem = _allPivot;
                e.Cancel = true;
            }
            else if (_curr.Equals(new Directory(PathBase.Root)))
            {
                throw new ApplicationMustExitException();
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
                Open((IListingsListItem)_favesBox.SelectedItem);
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
                Storyboard sb = GetOutStoryboardForForwardNavigation(li);
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

        private Storyboard GetInStoryboardFromForwardNavigation(Directory destination)
        {
            // Slide in from right
            Storyboard s = new Storyboard();
            TextBlock append = CreatePathPanelBlock("\\" + destination.Name);
            append.Opacity = 0;
            _pathPanel.Children.Add(append);

            DoubleAnimation boxSlide = AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE);
            Storyboard.SetTarget(boxSlide, CurrentBox);
            s.Children.Add(boxSlide); 

            DoubleAnimation rootFade = AnimationUtils.FadeIn(FADE_IN_DURATION);
            Storyboard.SetTarget(rootFade, Root);
            s.Children.Add(rootFade);
            
            DoubleAnimation pathSlide = AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE);
            Storyboard.SetTarget(pathSlide, append);
            s.Children.Add(pathSlide);

            DoubleAnimation pathFade = AnimationUtils.FadeIn(FADE_IN_DURATION);
            Storyboard.SetTarget(pathFade, append);
            s.Children.Add(pathFade);

            return s;
        }

        private Storyboard GetInStoryboardFromBackwardNavigation()
        {
            // Slide in from left
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, CurrentBox);

            s.Children.Add(AnimationUtils.TranslateX(-500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE));
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION));

            return s;
        }

        private Storyboard GetOutStoryboardForBackwardNavigation()
        {
            // Slide out to right
            Storyboard s = new Storyboard();
            s.Completed += new EventHandler((object sender, EventArgs e) => _pathPanel.Children.RemoveAt(_pathPanel.Children.Count - 1));

            DoubleAnimation slideBoxRight = AnimationUtils.TranslateX(0, 500, SLIDE_X_OUT_DURATION, SLIDE_X_OUT_EASE);
            Storyboard.SetTarget(slideBoxRight, CurrentBox);
            s.Children.Add(slideBoxRight);

            DoubleAnimation slidePathRight = AnimationUtils.TranslateX(0, 500, SLIDE_X_OUT_DURATION, SLIDE_X_OUT_EASE);
            Storyboard.SetTarget(slidePathRight, _pathPanel.Children[_pathPanel.Children.Count - 1]);
            s.Children.Add(slidePathRight);

            return s;
        }

        private Storyboard GetOutStoryboardForForwardNavigation(IListingsListItem selectedItem)
        {
            // Swoop selectedItem, fade out
            Storyboard s = new Storyboard();

            // Swoop
            Storyboard swoop = new Storyboard();
            Storyboard.SetTarget(swoop, selectedItem.GetAnimatedItemReference());

            swoop.Children.Add(AnimationUtils.TranslateY(0, 80, SWOOP_DURATION, new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 }));
            swoop.Children.Add(AnimationUtils.TranslateX(0, 350, SWOOP_DURATION, new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 4 }));
            s.Children.Add(swoop);

            // Fade out
            Storyboard fade = new Storyboard();
            foreach (IListingsListItem item in CurrentBox.Items) // ContentBox items
                if (item != selectedItem)
                    fade.Children.Add(ApplyFadeOutAnimation(item, FADE_OUT_DURATION));
            foreach (UIElement item in selectedItem.GetNotAnimatedItemsReference()) // Elements of selectedItem
                fade.Children.Add(ApplyFadeOutAnimation(item, FADE_OUT_DURATION));
            s.Children.Add(fade);

            return s;
        }

        private Storyboard GetNavigatedToStoryboard(Directory openingDirectory)
        {
            // Carousel/corkscrew items in (including PageTitle)
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, Root);
            
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION));
            s.Children.Add(AnimationUtils.TranslateY(350, 0, SLIDE_Y_IN_DURATION, SLIDE_Y_IN_EASE));

            InitPathPanelFromPath(openingDirectory.Path.PathString);

            return s;
        }

        private Storyboard GetNavigatedFromStoryboard()
        {
            // Carousel/corkscrew items in (including PageTitle)
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, Root);

            s.Children.Add(AnimationUtils.FadeOut(SLIDE_Y_OUT_DURATION));
            s.Children.Add(AnimationUtils.TranslateY(0, 350, SLIDE_Y_OUT_DURATION, SLIDE_Y_OUT_EASE));

            return s;
        }

        private void InitPathPanelFromPath(string path)
        {
            _pathPanel.Children.Clear();
            string prefix = string.Empty;
            foreach (string crumb in path.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                TextBlock element = CreatePathPanelBlock(prefix + crumb);
                _pathPanel.Children.Add(element);
                prefix = "\\";
            }
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

        private DoubleAnimation ApplyFadeOutAnimation(UIElement item, int millis)
        {
            DoubleAnimation d = AnimationUtils.FadeOut(millis);
            Storyboard.SetTarget(d, item);
            return d;
        }

        #endregion

        private bool isHereSecondTime;
        public EventHandler GetNavCompleteEventHandler(Action StartAnimation)
        {
            return (object sender, EventArgs e) =>
            {
                string s = (sender == null) ? "null" : sender.GetType().ToString();
                if (!isHereSecondTime)
                    isHereSecondTime = true;
                else
                {
                    UpdateView();
                    StartAnimation();
                }
            };
        }

        private void NavigateIn(IListingsListItem selectedItem)
        {
            isHereSecondTime = false;
            Directory destination = (Directory)selectedItem.ActionableItem;
            Storyboard outAnim = GetOutStoryboardForForwardNavigation(selectedItem);
            EventHandler WorkCompleted = GetNavCompleteEventHandler(delegate { GetInStoryboardFromForwardNavigation(destination).Begin(); });

            _curr = destination;
            outAnim.Completed += WorkCompleted;
            outAnim.Begin();
            UpdateItems(WorkCompleted);
        }

        private void NavigateBack(Directory destination)
        {
            isHereSecondTime = false;
            Storyboard outAnim = GetOutStoryboardForBackwardNavigation();
            EventHandler WorkCompleted = GetNavCompleteEventHandler(delegate { GetInStoryboardFromBackwardNavigation().Begin(); });

            _curr = destination;
            outAnim.Completed += WorkCompleted;
            outAnim.Begin();
            UpdateItems(WorkCompleted);
        }

        private void NavigateTo(Directory curr)
        {
            isHereSecondTime = false;
            EventHandler WorkCompleted = delegate
            {
                UpdateView();
                GetNavigatedToStoryboard(curr).Begin();
            };

            UpdateItems(WorkCompleted);
        }

        private void NavigateFrom(Uri destination)
        {
            NavigationService.Navigate(destination);
        }

        private void NavigateFrom(Directory destination)
        {
            isHereSecondTime = false;
            Storyboard outAnim = GetNavigatedFromStoryboard();
            EventHandler WorkCompleted = GetNavCompleteEventHandler(delegate { GetNavigatedToStoryboard(destination).Begin(); });

            _curr = destination;
            outAnim.Completed += WorkCompleted;
            outAnim.Begin();
            UpdateItems(WorkCompleted);
        }

        private BackgroundWorker CreateNewBackgroundWorker(Action workToDo, Action<object, RunWorkerCompletedEventArgs> OnCompleted)
        {
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (object sender, DoWorkEventArgs e) => { this.Dispatcher.BeginInvoke(workToDo); };
            b.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnCompleted);
            return b;
        }

        // Reads information from the incoming UriString to fill class fields
        private void GetArgs()
        {
            string s;
            if (NavigationContext.QueryString.TryGetValue("param", out s))
            {
                Path p = Path.CreatePathFromString(s);
                _curr = new Directory(p);
            }
            else
            {
                IList<object> args = ParamUtils.GetArguments();
                _curr = (Directory)args[0];
            }
        }

        // Changes the currently-viewed folder and updates the view
        private void UpdateItems(EventHandler Completed)
        {
            _items.Clear();

            // Re-fill ContentBox
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> dirs = new List<string>();
                foreach (string dir in isf.GetDirectoryNames(_curr.Path.PathString + "/*"))
                    if (!dir.StartsWith("."))
                        dirs.Add(dir);
                dirs.Sort();

                // Add directories
                foreach (string dir in dirs)
                {
                    Directory d = new Directory(_curr.Path.NavigateIn(dir));
                    IListingsListItem li = IListingsListItem.CreateListItem(d);
                    _items.Add(li);
                }

                List<string> docs = new List<string>();
                foreach (string doc in isf.GetFileNames(_curr.Path.PathString + "/*"))
                    if (!doc.StartsWith("."))
                        docs.Add(doc);
                docs.Sort();
                
                // Add documents
                foreach (string doc in docs)
                {
                    Document d = new Document(_curr.Path.NavigateIn(doc));
                    IListingsListItem li = IListingsListItem.CreateListItem(d);
                    _items.Add(li);
                }
            }

            if (Completed != null)
                Completed(null, null);

            RemoveNotice(Notice.Empty);
            RemoveNotice(Notice.Loading);
        }

        private void UpdateView()
        {
            CurrentBox.Items.Clear();

            foreach (IListingsListItem li in _items)
                CurrentBox.Items.Add(li);

            if (_items.Count == 0)
                ShowNotice(Notice.Empty);

            if (_pageMode == PageMode.Edit)
                SetPageMode(PageMode.View);
        }

        private void UpdateFavesView()
        {
            List<IActionable> temp = new List<IActionable>();
            foreach (Directory d in FileUtils.GetAllDirectories(PathBase.Root))
                if (d.IsFavorite) { temp.Add(d); }
            temp.Sort();
            foreach (Directory d in temp)
                _faves.Add(IListingsListItem.CreateListItem(d));
            temp.Clear();

            foreach (Document d in FileUtils.GetAllDocuments(PathBase.Root))
                if (d.IsFavorite) { temp.Add(d); }
            temp.Sort();
            foreach (Document d in temp)
                _faves.Add(IListingsListItem.CreateListItem(d));

            //List<IActionable> lst = new List<IActionable>();
            //foreach (string p in (IEnumerable<string>)IsolatedStorageSettings.ApplicationSettings[App.FavoritesKey])
            //{
            //    lst.Add(Path.CreatePathFromString(s));
            //}

            CurrentBox.Items.Clear();
            foreach (IListingsListItem i in _faves)
                CurrentBox.Items.Add(i);
            _faves.Clear();
        }

        private void ShowNotice(Notice notice)
        {
            if (notice == Notice.Empty)
            {
                if (_emptyNotice == null)
                {
                    _emptyNotice = CreateNoticeBlock(notice.GetText());
                }
                _currentGrid.Children.Add(_emptyNotice);
            }
            else if (notice == Notice.Loading)
            {
                if (_loadingNotice == null)
                {
                    _loadingNotice = CreateNoticeBlock(notice.GetText());
                }
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
                    _currentGrid.Children.Remove(_emptyNotice);
                    break;
                case Notice.Loading:
                    _currentGrid.Children.Remove(_loadingNotice);
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

            _allBox = new ListBox();
            _allBox.Margin = new Thickness(6, 0, 12, 0);
            _allBox.MinHeight = 500;
            _allBox.VerticalAlignment = VerticalAlignment.Top;
            _allBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _allBox.RenderTransform = new CompositeTransform();
            Grid.SetRow(_allBox, 0);
            _allGrid.Children.Add(_allBox);

            _favesPivot = new PivotItem();
            _favesPivot.Header = "favorites";
            _masterPivot.Items.Add(_favesPivot);

            _favesGrid = new Grid();
            _favesPivot.Content = _favesGrid;

            _favesBox = new ListBox();
            _favesBox.Margin = new Thickness(6, 0, 12, 0);
            _favesBox.MinHeight = 500;
            _favesBox.VerticalAlignment = VerticalAlignment.Top;
            _favesBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _favesBox.RenderTransform = new CompositeTransform();
            Grid.SetRow(_favesBox, 0);
            _favesGrid.Children.Add(_favesBox);
        }

        public void SetPageMode(PageMode type)
        {
            if (type == PageMode.View && _pageMode != PageMode.View)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Single;
                ApplicationBar = (new Listings.ViewAppBar(this)).AppBar;
            }
            else if (type == PageMode.Edit && _pageMode != PageMode.Edit)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Multiple;
                ApplicationBar = (new Listings.EditAppBar(this)).AppBar;
            }
            else if (type == PageMode.Trash && _pageMode != PageMode.Trash)
            {
                CurrentBox.SelectedIndex = -1;
                CurrentBox.SelectionMode = SelectionMode.Multiple;
                _currBeforeTrash = _curr;
                ApplicationBar = (new Listings.TrashAppBar(this)).AppBar;
                NavigateFrom(new Directory(PathBase.Trash));
            }
            else if (type == PageMode.Favorites && _pageMode != PageMode.Favorites)
            {
                CurrentBox.SelectedItem = -1;
                CurrentBox.SelectionMode = SelectionMode.Single;
                ApplicationBar = (new Listings.FavoritesAppBar(this)).AppBar;
            }
            _pageMode = type;
        }

        private void BeginDeleteAnimations(IList<IListingsListItem> deletedItems)
        {
            IListingsListItem lastDeletedItem = deletedItems[deletedItems.Count - 1];
            IList<IListingsListItem> previousItems = new List<IListingsListItem>();
            int i = 0;
            while (i < CurrentBox.Items.Count)
            {
                IListingsListItem item = (IListingsListItem)CurrentBox.Items[i];
                i++;
                if (item != deletedItems[0])
                    previousItems.Add(item);
                if (item == lastDeletedItem)
                    break;
            }

            double height = 0;
            foreach (IListingsListItem item in deletedItems)
            {
                // 25 is the bottom margin on a ListingsListItem. Not sure why, but
                // item.Margin.Bottom returns 0.0
                height += item.DesiredSize.Height;
            }

            Storyboard s = new Storyboard();
            while (i < CurrentBox.Items.Count)
            {
                DoubleAnimation d = AnimationUtils.TranslateY(height, 0, 110, new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 });
                Storyboard.SetTarget(d, (UIElement)CurrentBox.Items[i]);
                s.Children.Add(d);
                ((UIElement)CurrentBox.Items[i]).RenderTransform = new CompositeTransform();
                i++;
            }

            foreach (IListingsListItem item in deletedItems)
                CurrentBox.Items.Remove(item);

            s.Begin();
            s.Completed += (object sender, EventArgs e) =>
            {
                if (CurrentBox.Items.Count == 0)
                    ShowNotice(Notice.Empty);
            };
        }

        #endregion 

        #region AppBar Classes

        private class ViewAppBar : ApplicationBarSetup
        {
            private static ApplicationBarIconButton NewButton;
            private static ApplicationBarIconButton SelectButton;
            private static ApplicationBarMenuItem SearchItem;
            private static ApplicationBarMenuItem SettingsItem;
            private static ApplicationBarMenuItem TrashItem;
            private static ApplicationBarMenuItem ImportExportItem;
            private static ApplicationBarMenuItem AboutTipsItem;

            public ViewAppBar(Listings Page)
            {
                NewButton = ViewUtils.CreateIconButton("new", App.AddIcon, (object Sender, EventArgs e) =>
                {
                    ParamUtils.SetArguments(Page._curr);
                    Page.NavigationService.Navigate(App.AddNewItem);
                });
                SelectButton = ViewUtils.CreateIconButton("select", App.SelectIcon, (object sender, EventArgs e) => { Page.SetPageMode(PageMode.Edit); });

                SearchItem = ViewUtils.CreateMenuItem("search", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.Search); });
                SettingsItem = ViewUtils.CreateMenuItem("settings", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.Settings); });
                TrashItem = ViewUtils.CreateMenuItem("trash", (object sender, EventArgs e) => { Page.SetPageMode(PageMode.Trash); });
                ImportExportItem = ViewUtils.CreateMenuItem("import+export", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.ExportAll); });
                AboutTipsItem = ViewUtils.CreateMenuItem("about+tips", (object sender, EventArgs e) => { Page.NavigationService.Navigate(App.AboutAndTips); });

                foreach (IListingsListItem item in Page.CurrentBox.Items)
                    item.IsSelectable = false;

                _buttons = new ButtonList() { NewButton, SelectButton };
                _menuItems = new ItemList() { SearchItem, SettingsItem, TrashItem, ImportExportItem, AboutTipsItem };
                ApplicationBarSetup.SetElements(_appBar, _buttons, _menuItems);
                _appBar.Mode = ApplicationBarMode.Default;
                _appBar.IsMenuEnabled = true;
            }
        }

        private class FavoritesAppBar : ApplicationBarSetup
        {
            public FavoritesAppBar(Listings Page)
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
            {
                this.Page = Page;
                DeleteButton = ViewUtils.CreateIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    Page.BeginDeleteAnimations(deletedItems);
                    Page.SetPageMode(PageMode.View);
                });
                FaveButton = ViewUtils.CreateIconButton("add favorite", App.FaveIcon, (object sender, EventArgs e) =>
                {
                    (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem.IsFavorite = true;
                    Page.SetPageMode(PageMode.View);
                });
                UnfaveButton = ViewUtils.CreateIconButton("remove favorite", App.UnfaveIcon, (object sender, EventArgs e) =>
                {
                    (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem.IsFavorite = false;
                    Page.SetPageMode(PageMode.View);
                });
                RenameItem = (ViewUtils.CreateMenuItem("rename", (object sender, EventArgs e) =>
                {
                    IActionable a = (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.NavToRename(Page.NavigationService);
                }));
                MoveItem = ViewUtils.CreateMenuItem("move", (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                        args.Add(li.ActionableItem);
                    ParamUtils.SetArguments(args);
                    Page.NavigationService.Navigate(App.MoveItem);
                });
                PinItem = ViewUtils.CreateMenuItem("pin", (object sender, EventArgs e) =>
                {
                    IActionable a = (Page.CurrentBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.TogglePin();
                });

                _buttons = new ButtonList() { DeleteButton, FaveButton };
                _menuItems = new ItemList() { RenameItem, MoveItem, PinItem };
                ApplicationBarSetup.SetElements(_appBar, _buttons, _menuItems);
                ApplicationBarSetup.SetAllEnabled(_appBar, false);
                Page.CurrentBox.SelectionChanged += new SelectionChangedEventHandler(SelectedItemsChanged);
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
                    _appBar.Buttons.Clear();
                    _appBar.Buttons.Add(DeleteButton);
                    if (selectedActionable.IsFavorite)
                        _appBar.Buttons.Add(UnfaveButton);
                    else if (!selectedActionable.IsFavorite)
                        _appBar.Buttons.Add(FaveButton);
                    SetAllEnabled(_appBar, true);
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
            {
                this.Page = Page;
                DeleteButton = ViewUtils.CreateIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    Page.BeginDeleteAnimations(deletedItems);
                });
                RestoreButton = ViewUtils.CreateIconButton("restore", App.UndeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in Page.CurrentBox.SelectedItems)
                        args.Add(li.ActionableItem);

                    ParamUtils.SetArguments(args);
                    Page.NavigationService.Navigate(App.MoveItem);
                });
                EmptyItem = ViewUtils.CreateMenuItem("empty trash", (object sender, EventArgs e) =>
                {
                    if (MessageBoxResult.Cancel == MessageBox.Show("This will delete all documents in trash permanently. Do you want to continue?", "Warning", MessageBoxButton.OKCancel))
                        return;

                    foreach (IListingsListItem i in Page.CurrentBox.SelectedItems)
                        i.ActionableItem.Delete();
                    Page.CurrentBox.Items.Clear();
                });

                _buttons = new ButtonList() { DeleteButton, RestoreButton };
                _menuItems = new ItemList() { EmptyItem };
                SetElements(_appBar, _buttons, _menuItems);
                SetAllEnabled(_appBar, false);
                Page.CurrentBox.SelectionChanged += new SelectionChangedEventHandler(SelectedItemsChanged);
                _appBar.Mode = ApplicationBarMode.Default;
                _appBar.IsMenuEnabled = true;

                Directory trash = new Directory(PathBase.Trash);
                Storyboard sb = Page.GetNavigatedToStoryboard(trash);
                sb.Completed += new EventHandler((object sender, EventArgs e) =>
                {
                    foreach (IListingsListItem item in Page.CurrentBox.Items)
                        item.IsSelectable = true;
                });
            }

            public void SelectedItemChanged(object sender, SelectionChangedEventArgs e)
            {
                int ct = Page.CurrentBox.SelectedItems.Count;
                if (ct == 0)
                    SetEnabledElements(false, new ButtonList() { DeleteButton, RestoreButton }, new ItemList());
                else
                    SetAllEnabled(_appBar, true);
            }
        }

        #endregion
    }
}