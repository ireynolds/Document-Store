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

namespace NotepadTheNextVersion.ListItems
{
    public partial class Listings : PhoneApplicationPage
    {
        private Directory _currBeforeTrash;
        private Directory _curr;
        private ListingsMode _pageMode;
        private IList<IListingsListItem> _items;
        private IList<IListingsListItem> _favs;
        private bool _isUpdatingItems;
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

        private ListBox _currentBox
        {
            get
            {
                if (_masterPivot.SelectedItem == _allPivot)
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

        #region Storyboard constants (in millis)

        private const int SLIDE_X_OUT_DURATION = 150;
        private const int SLIDE_X_IN_DURATION = 150;
        private const int SLIDE_Y_OUT_DURATION = 200;
        private const int SLIDE_Y_IN_DURATION = 200;
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
            Root.RenderTransform = new CompositeTransform();

            InitializeApplicationBar();
            InitializePageUI();
            SetPageMode(ListingsMode.View);

            Root.Opacity = 0;
        }

        void Listings_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateItems(() => GetNavigatedToStoryboard(_curr).Begin());
            UpdateView(() => GetNavigatedToStoryboard(_curr).Begin());
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
            if (_pageMode == ListingsMode.Trash)
            {
                SetPageMode(ListingsMode.View);
                Navigate(_currBeforeTrash,
                         GetNavigatedFromStoryboard(),
                         GetNavigatedToStoryboard(_currBeforeTrash));
                e.Cancel = true;
            }
            else if (_pageMode == ListingsMode.Edit)
            {
                SetPageMode(ListingsMode.View);
                e.Cancel = true;
            }
            else if (_pageMode == ListingsMode.View)
            {
                Path parent = _curr.Path.Parent;
                if (parent != null)
                {
                    Navigate(new Directory(parent),
                        GetOutStoryboardForBackwardNavigation(),
                        GetInStoryboardFromBackwardNavigation());
                    e.Cancel = true;
                }
            }
            else if (_curr.Equals(new Directory(PathBase.Root)))
            {
                throw new ApplicationMustExitException();
            }
            else
                throw new Exception("Unknown page mode (should be \"edit\" or \"view\")");

            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Root.Opacity = 0;
        }

        #region Event Handlers

        private void ContentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isShowingEmptyNotice || _isShowingLoadingNotice)
                return;

            if (_pageMode == ListingsMode.Trash)
            {
                SyncSelectedItemsWithCheckboxes(e);
                if (_allBox.SelectedItems.Count == 0)
                    SetAppBarButtonsEnabled(false);
                else
                    SetAppBarButtonsEnabled(true);
            }
            else if (_pageMode == ListingsMode.Edit)
            {
                SyncSelectedItemsWithCheckboxes(e);
                if (_allBox.SelectedItems.Count == 0)
                    SetPageMode(ListingsMode.View);
                else if (_allBox.SelectedItems.Count == 1)
                    EnableSingleSelectionAppBarItems();
                else if (_allBox.SelectedItems.Count > 1)
                    EnableMultipleSelectionAppBarItems();
            }
            else if (_pageMode == ListingsMode.View)
            {
                if (_allBox.SelectedIndex == -1)
                    return;

                IListingsListItem li = (IListingsListItem)_allBox.SelectedItem;
                _allBox.SelectedIndex = -1;
                if (li.GetType() == typeof(DocumentListItem))
                {
                    Storyboard sb = GetOutStoryboardForForwardNavigation(li);
                    sb.Completed += new EventHandler(
                        (object a, EventArgs b) => li.ActionableItem.Open(NavigationService));
                    sb.Begin();
                }
                else
                {
                    Navigate((Directory)li.ActionableItem,
                        GetOutStoryboardForForwardNavigation(li),
                        GetInStoryboardFromForwardNavigation(li.ActionableItem.DisplayName));
                }
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

        private Storyboard GetInStoryboardFromForwardNavigation(string destinationDisplayName)
        {
            // Slide in from right
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, _allBox);

            s.Children.Add(AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE));
            s.Children.Add(AnimationUtils.FadeIn(FADE_IN_DURATION));

            TextBlock append = CreatePathPanelBlock(destinationDisplayName);
            append.Opacity = 0;
            _pathPanel.Children.Add(append);
            
            DoubleAnimation pathSlide = AnimationUtils.TranslateX(500, 0, SLIDE_X_IN_DURATION, SLIDE_X_IN_EASE);
            Storyboard.SetTarget(pathSlide, append);
            s.Children.Add(pathSlide);
            DoubleAnimation pathFade = AnimationUtils.FadeIn(1);
            Storyboard.SetTarget(pathFade, append);
            s.Children.Add(pathFade);

            return s;
        }

        private Storyboard GetInStoryboardFromBackwardNavigation()
        {
            // Slide in from left
            Storyboard s = new Storyboard();
            Storyboard.SetTarget(s, _allBox);

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
            Storyboard.SetTarget(slideBoxRight, _allBox);
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
            foreach (IListingsListItem item in _allBox.Items) // ContentBox items
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
            
            s.Children.Add(AnimationUtils.FadeIn(SLIDE_Y_IN_DURATION));
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
            foreach (string crumb in path.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                TextBlock element = CreatePathPanelBlock(crumb);
                _pathPanel.Children.Add(element);
            }
        }

        private TextBlock CreatePathPanelBlock(string element)
        {
            return new TextBlock()
            {
                Text = "\\" + element.ToUpper(),
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

        private void Navigate(Directory newDirectory, Storyboard outAnimation, Storyboard inAnimation)
        {
            outAnimation.Begin();

            _curr = newDirectory;
            Action BeginInAnimation = () =>
            {
                inAnimation.Begin();
            };
            Action WorkToDo = () =>
            {
                UpdateItems(BeginInAnimation);
            };
            outAnimation.Completed += new EventHandler((object sender, EventArgs e) =>
            {
                UpdateView(BeginInAnimation);
            });
            Action<object, RunWorkerCompletedEventArgs> RunWorkerCompletedEvent = (object sender, RunWorkerCompletedEventArgs e) =>
            {
                if (outAnimation.GetCurrentState() != ClockState.Active)
                {
                    UpdateView(BeginInAnimation);
                    outAnimation.Stop();
                }
            };

            CreateNewBackgroundWorker(WorkToDo, RunWorkerCompletedEvent)
                .RunWorkerAsync();
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
        private void UpdateItems(Action OnCompleted)
        {
            _isUpdatingItems = true;
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

            _isUpdatingItems = false;
            if (_isShowingLoadingNotice && OnCompleted != null)
                UpdateView(OnCompleted);

            RemoveNotice(Notice.Empty);
            RemoveNotice(Notice.Loading);
        }

        private void UpdateView(Action OnCompleted)
        {
            _allBox.Items.Clear();

            if (_isUpdatingItems)
                ShowNotice(Notice.Loading);
            else if (_items.Count == 0)
                ShowNotice(Notice.Empty);
            else
                foreach (IListingsListItem li in _items)
                    _allBox.Items.Add(li);

            DisableOrEnableScrolling();
            if (_pageMode == ListingsMode.Edit)
                SetPageMode(ListingsMode.View);

            if (OnCompleted != null)
                OnCompleted();
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

        private void DisableOrEnableScrolling()
        {
            _allBox.UpdateLayout();
            if (_allBox.Items.Count < 6)
                ScrollViewer.SetVerticalScrollBarVisibility(_allBox, ScrollBarVisibility.Disabled);
            else
                ScrollViewer.SetVerticalScrollBarVisibility(_allBox, ScrollBarVisibility.Visible);
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
            _allBox.VerticalAlignment = VerticalAlignment.Top;
            _allBox.MinHeight = 625;
            _allBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _allBox.RenderTransform = new CompositeTransform();
            _allGrid.Children.Add(_allBox);

            _favesPivot = new PivotItem();
            _favesPivot.Header = "favorites";
            _masterPivot.Items.Add(_favesPivot);

            _favesGrid = new Grid();
            _favesPivot.Content = _favesGrid;

            _favesBox = new ListBox();
            _favesBox.Margin = new Thickness(6, 0, 12, 0);
            _favesBox.MinHeight = 625;
            _favesBox.VerticalAlignment = VerticalAlignment.Top;
            _favesBox.SelectionChanged += new SelectionChangedEventHandler(ContentBox_SelectionChanged);
            _favesBox.MinHeight = 625;
            _favesBox.RenderTransform = new CompositeTransform();
            _favesGrid.Children.Add(_favesBox);
        }

        private void SetPageMode(ListingsMode type)
        {
            _pageMode = type;
            if (type == ListingsMode.View)
                InitializeViewMode();
            else if (type == ListingsMode.Edit)
                InitializeEditMode();
            else if (type == ListingsMode.Trash)
                InitializeTrashMode();
        }

        private IList<ApplicationBarIconButton> TrashListButtons;
        private IList<ApplicationBarMenuItem> TrashListItems;

        private void InitializeTrashMode()
        {
            if (TrashListButtons == null)
            {
                TrashListButtons = new List<ApplicationBarIconButton>();
                TrashListButtons.Add(ViewUtils.createIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in _allBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    BeginDeleteAnimations(deletedItems);
                }));
                TrashListButtons.Add(ViewUtils.createIconButton("restore", App.UndeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in _allBox.SelectedItems)
                        args.Add(li.ActionableItem);

                    ParamUtils.SetArguments(args);
                    NavigationService.Navigate(App.MoveItem);
                }));
            }

            if (TrashListItems == null)
            {
                TrashListItems = new List<ApplicationBarMenuItem>();

                TrashListItems.Add(ViewUtils.createMenuItem("empty trash", (object sender, EventArgs e) =>
                {
                    if (MessageBoxResult.Cancel == MessageBox.Show("This will delete all documents in trash permanently. Do you want to continue?", "Warning", MessageBoxButton.OKCancel))
                        return;

                    foreach (IListingsListItem i in _allBox.SelectedItems)
                        i.ActionableItem.Delete();
                    _allBox.Items.Clear();
                }));
            }

            _allBox.SelectedIndex = -1;
            _allBox.SelectionMode = SelectionMode.Multiple;

            ApplicationBar.Buttons.Clear();
            foreach (ApplicationBarIconButton b in TrashListButtons)
                ApplicationBar.Buttons.Add(b);

            ApplicationBar.MenuItems.Clear();
            foreach (ApplicationBarMenuItem i in TrashListItems)
                ApplicationBar.MenuItems.Add(i);

            SetAppBarButtonsEnabled(false);

            _currBeforeTrash = _curr;
            Directory trash = new Directory(PathBase.Trash);
            Storyboard sb = GetNavigatedToStoryboard(trash);
            sb.Completed += new EventHandler((object sender, EventArgs e) =>
            {
                foreach (IListingsListItem item in _allBox.Items)
                    item.IsSelectable = true;
            });
            Navigate(trash,
                     GetNavigatedFromStoryboard(),
                     sb);
        }

        private IList<ApplicationBarIconButton> ViewListButtons;
        private IList<ApplicationBarMenuItem> ViewListItems;

        // Returns a list of icon buttons for the application bar's "view" setting
        private void InitializeViewMode()
        {
            // Lazy initialization
            if (ViewListButtons == null) // && ViewListItems == null
            {
                ViewListButtons = new List<ApplicationBarIconButton>();
                ViewListButtons.Add(ViewUtils.createIconButton("new", App.AddIcon, (object Sender, EventArgs e) =>
                {
                    ParamUtils.SetArguments(_curr);
                    NavigationService.Navigate(App.AddNewItem);
                }));
                ViewListButtons.Add(ViewUtils.createIconButton("select", App.SelectIcon, (object sender, EventArgs e) => { SetPageMode(ListingsMode.Edit); }));

                ViewListItems = new List<ApplicationBarMenuItem>();
                ViewListItems.Add(ViewUtils.createMenuItem("search", (object sender, EventArgs e) => { NavigationService.Navigate(App.Search); }));
                ViewListItems.Add(ViewUtils.createMenuItem("settings", (object sender, EventArgs e) => { NavigationService.Navigate(App.Settings); }));
                ViewListItems.Add(ViewUtils.createMenuItem("trash", (object sender, EventArgs e) => { SetPageMode(ListingsMode.Trash); }));
                ViewListItems.Add(ViewUtils.createMenuItem("import+export", (object sender, EventArgs e) => { NavigationService.Navigate(App.ExportAll); }));
                ViewListItems.Add(ViewUtils.createMenuItem("about+tips", (object sender, EventArgs e) => { NavigationService.Navigate(App.AboutAndTips); }));
            }

            _allBox.SelectedIndex = -1;
            _allBox.SelectionMode = SelectionMode.Single;

            ApplicationBar.Buttons.Clear();
            foreach (ApplicationBarIconButton b in ViewListButtons)
                ApplicationBar.Buttons.Add(b);

            ApplicationBar.MenuItems.Clear();
            foreach (ApplicationBarMenuItem i in ViewListItems)
                ApplicationBar.MenuItems.Add(i);

            foreach (IListingsListItem item in _allBox.Items)
                item.IsSelectable = false;
        }

        private IList<ApplicationBarIconButton> EditListButtons;
        private IList<ApplicationBarMenuItem> EditListItems;

        // Returns a list of icon buttons for the application bar's "edit" setting
        private void InitializeEditMode()
        {
            // Lazy initialization
            if (EditListButtons == null) // && EditListItems == null
            {
                EditListButtons = new List<ApplicationBarIconButton>();
                EditListButtons.Add(ViewUtils.createIconButton("delete", App.DeleteIcon, (object sender, EventArgs e) =>
                {
                    IList<IListingsListItem> deletedItems = new List<IListingsListItem>();
                    foreach (IListingsListItem li in _allBox.SelectedItems)
                    {
                        li.ActionableItem.Delete();
                        deletedItems.Add(li);
                    }
                    BeginDeleteAnimations(deletedItems);
                    SetPageMode(ListingsMode.View);
                }));
                EditListButtons.Add(ViewUtils.createIconButton("pin", App.PinIcon, (object sender, EventArgs e) =>
                {
                    IActionable a = (_allBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.TogglePin();
                }));

                EditListItems = new List<ApplicationBarMenuItem>();
                EditListItems.Add(ViewUtils.createMenuItem("move", (object sender, EventArgs e) =>
                {
                    IList<IActionable> args = new List<IActionable>();
                    foreach (IListingsListItem li in _allBox.SelectedItems)
                        args.Add(li.ActionableItem);

                    ParamUtils.SetArguments(args);
                    NavigationService.Navigate(App.MoveItem);
                }));
                EditListItems.Add(ViewUtils.createMenuItem("rename", (object sender, EventArgs e) =>
                {
                    IActionable a = (_allBox.SelectedItem as IListingsListItem).ActionableItem;
                    a.NavToRename(NavigationService);
                }));
            }

            _allBox.SelectedIndex = -1;
            _allBox.SelectionMode = SelectionMode.Multiple;

            ApplicationBar.Buttons.Clear();
            foreach (ApplicationBarIconButton b in EditListButtons)
                ApplicationBar.Buttons.Add(b);
            
            ApplicationBar.MenuItems.Clear();
            foreach (ApplicationBarMenuItem i in EditListItems)
                ApplicationBar.MenuItems.Add(i);

            SetAppBarEnabled(false);

            foreach (IListingsListItem item in _allBox.Items)
                item.IsSelectable = true;
        }

        private void BeginDeleteAnimations(IList<IListingsListItem> deletedItems)
        {
            IListingsListItem lastDeletedItem = deletedItems[deletedItems.Count - 1];
            IList<IListingsListItem> previousItems = new List<IListingsListItem>();
            int i = 0;
            while (i < _allBox.Items.Count)
            {
                IListingsListItem item = (IListingsListItem)_allBox.Items[i];
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
            while (i < _allBox.Items.Count)
            {
                DoubleAnimation d = AnimationUtils.TranslateY(height, 0, 110, new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 });
                Storyboard.SetTarget(d, (UIElement)_allBox.Items[i]);
                s.Children.Add(d);
                ((UIElement)_allBox.Items[i]).RenderTransform = new CompositeTransform();
                i++;
            }

            foreach (IListingsListItem item in deletedItems)
                _allBox.Items.Remove(item);

            s.Begin();
            s.Completed += (object sender, EventArgs e) =>
            {
                if (_allBox.Items.Count == 0)
                    ShowNotice(Notice.Empty);
            };
        }

        private void SetAppBarItemsEnabled(bool enabled)
        {
            foreach (ApplicationBarMenuItem i in ApplicationBar.MenuItems)
                i.IsEnabled = enabled;
        }

        private void SetAppBarEnabled(bool enabled)
        {
            SetAppBarButtonsEnabled(enabled);
            SetAppBarItemsEnabled(enabled);
        }

        private void SetAppBarButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
                b.IsEnabled = enabled;
        }

        private void EnableSingleSelectionAppBarItems()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;

            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = true;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).IsEnabled = true;
        }

        private void EnableMultipleSelectionAppBarItems()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;

            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = true;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).IsEnabled = false;
        }

        #endregion 
    }
}