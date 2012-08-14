using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using System.Collections.Generic;
using System;
using NotepadTheNextVersion.Models;
using System.Windows.Media.Animation;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Enumerations;
using System.IO;
using System.Collections.ObjectModel;

namespace NotepadTheNextVersion
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        // Holds parameters during navigations.
        public IList<object> Arguments;

        public static readonly MyUri Listings = new MyUri("/Views/Listings.xaml", UriKind.Relative);
        public static readonly MyUri RenameItem = new MyUri("/Views/RenameItem.xaml", UriKind.Relative);
        public static readonly MyUri MoveItem = new MyUri("/Views/MoveItem.xaml", UriKind.Relative);
        public static readonly MyUri DocumentEditor = new MyUri("/Views/DocumentEditor.xaml", UriKind.Relative);
        public static readonly MyUri AddNewItem = new MyUri("/Views/AddNewItem.xaml", UriKind.Relative);
        public static readonly MyUri Search = new MyUri("/Views/Search.xaml", UriKind.Relative);
        public static readonly MyUri Settings = new MyUri("/Views/Settings.xaml", UriKind.Relative);
        public static readonly MyUri AboutAndTips = new MyUri("/Views/AboutAndTips.xaml", UriKind.Relative);
        public static readonly MyUri ExportAll = new MyUri("/Views/ExportAll.xaml", UriKind.Relative);
        public static readonly MyUri SendAs = new MyUri("/Views/SendAs.xaml", UriKind.Relative);

        public const string AddIcon = "/Images/appbar.add.rest.png";
        public const string BackIcon = "/Images/appbar.back.rest.png";
        public const string CancelIcon = "/Images/appbar.cancel.rest.png";
        public const string CheckIcon = "/Images/appbar.check.rest.png";
        public const string DeleteIcon = "/Images/appbar.delete.rest.png";
        public const string SearchIcon = "/Images/appbar.feature.search.rest.png";
        public const string SettingsIcon = "/Images/appbar.feature.settings.rest.png";
        public const string FolderIconSmall = "/Images/appbar.folder.rest.png";
        public const string SaveIcon = "/Images/appbar.save.rest.png";
        public const string FolderIconLargeBlack = "/Images/folder.black.png";
        public const string FolderIconLargeWhite = "/Images/folder.white.png";
        public const string UndeleteIcon = "/Images/appbar.undelete.rest3.png";
        public const string PinIcon = "/Images/pushpin.png";
        public const string SelectIcon = "/Images/appbar.list.check.png";
        public const string FaveIcon = "/Images/appbar.favs.addto.rest.png";
        public const string UnfaveIcon = "/Images/appbar.star.minus.png";

        public const string DocumentTile = "Application_DocumentTile.png";
        public const string DirectoryTile = "Application_DirectoryTile.png";

        public const string FavoritesKey = "Favorites";

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            string rootName = (string)SettingUtils.GetSetting(Setting.RootDirectoryName);
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.DirectoryExists(rootName))
                    isf.CreateDirectory(rootName);
                if (!isf.DirectoryExists("trash-dir"))
                    isf.CreateDirectory("trash-dir");
            }

            if (!IsolatedStorageSettings.ApplicationSettings.Contains(App.FavoritesKey))
                IsolatedStorageSettings.ApplicationSettings[App.FavoritesKey] = new Collection<string>();

            //// Add test data
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
            //    IsolatedStorageFileStream s1 = isf.CreateFile(rootName + "/new1-doc");
            //    s1.Close();
            //    IsolatedStorageFileStream s2 = isf.CreateFile(rootName + "/new2-doc");
            //    s2.Close();
            //    isf.CreateDirectory(rootName + "/Dir1-dir");
            //    isf.CreateDirectory(rootName + "/Dir2-dir");
            //    isf.CreateDirectory(rootName + "/Dir1-dir/SubDir1-dir");
            //    isf.CreateDirectory(rootName + "/Dir2-dir/SubDir2-dir");
                WriteFiles(new string[] { "FileDir1-dir", "FileDir2-dir" });
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.Message, "An error occurred", MessageBoxButton.OK);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        private void WriteFiles(string[] directories)
        {
            string testText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque rutrum tristique semper. " +
                    "Vestibulum pulvinar, nibh ut vehicula laoreet, justo sem fermentum enim, quis feugiat magna mauris " +
                    "sit amet sapien. Pellentesque vitae laoreet velit. Pellentesque sit amet commodo purus. Duis sit amet " +
                    "elit arcu, vitae tincidunt ligula. Curabitur aliquam semper vehicula. Class aptent taciti sociosqu ad " +
                    "litora torquent per conubia nostra, per inceptos himenaeos. Curabitur interdum gravida odio vestibulum " +
                    "rhoncus. Etiam vel tempor urna. Nam eu turpis vel elit convallis mollis. Curabitur non nulla ut massa " +
                    "dapibus tempus ac ut nisi. Integer risus risus, placerat et placerat ut, blandit sit amet justo. Cras " +
                    "ut augue nibh, vel sagittis sem.\r\rMorbi vestibulum tristique vulputate. Integer ultricies bibendum " +
                    "nulla, non bibendum risus mollis a. Pellentesque condimentum tempus hendrerit. Nam a est id ligula " +
                    "luctus scelerisque. Duis vel felis orci. Praesent sed arcu sit amet libero rutrum venenatis non at " +
                    "eros. Fusce quis sapien ut nunc ornare rhoncus. Lorem ipsum dolor sit amet, consectetur adipiscing " +
                    "elit. Integer ullamcorper egestas tempor. Ut suscipit ante vel leo volutpat vel ultrices enim volutpat." +
                    " Vivamus semper laoreet diam, ut scelerisque ante scelerisque fermentum. Mauris sit amet elit lorem. " +
                    "Nam vehicula volutpat leo eu suscipit. Sed condimentum sagittis sodales. Fusce rutrum metus at felis " +
                    "dictum posuere.\r\rCum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. " +
                    "Etiam consequat nisi varius lacus sodales vitae dictum mi pharetra. Quisque lobortis ultrices enim, vel " +
                    "blandit lectus tristique sit amet. Phasellus non mi turpis, sed rutrum justo. Sed faucibus odio at massa " +
                    "bibendum ornare. Morbi eu metus tellus. Pellentesque a lacus eu leo lobortis sodales quis at risus. " +
                    "Phasellus vitae lobortis nisl. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere " +
                    "cubilia Curae; Quisque cursus libero eget dolor gravida laoreet. In lobortis eleifend elit ac cursus.\r\rIn " +
                    "consectetur libero ac justo placerat imperdiet. Aenean malesuada est ac augue rhoncus vulputate. Duis lacus " +
                    "orci, posuere eget hendrerit eu, pellentesque quis purus. Integer vehicula viverra neque, ac dignissim " +
                    "purus lobortis id. Aliquam sit amet nisi et arcu eleifend pharetra eget a odio. Suspendisse at nulla " +
                    "at ipsum rutrum pharetra. Curabitur et nisi a ipsum dignissim tincidunt. Maecenas blandit libero sed " +
                    "massa mollis sollicitudin. Ut auctor turpis eu metus facilisis hendrerit molestie elit tincidunt. " +
                    "Nulla egestas consequat tortor, in mollis sapien consequat ac. Ut dapibus libero nec diam rutrum eget " +
                    "imperdiet quam porttitor. Duis egestas vulputate mi, et volutpat odio ultrices id.\r\rSed ligula nisl, " +
                    "placerat id dictum et, aliquet tempor neque. Cras nibh magna, tincidunt non bibendum eu, vestibulum " +
                    "vitae odio. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. " +
                    "Maecenas et consectetur dui. Mauris nec odio sapien, et volutpat quam. Phasellus quis commodo erat. " +
                    "Vivamus dictum feugiat scelerisque. Vivamus ultrices suscipit urna in consectetur. Etiam luctus " +
                    "consectetur massa sit amet imperdiet. Etiam pellentesque ultrices nisi vel auctor. Proin leo ante, " +
                    "consectetur in euismod in, ornare in lorem. Proin a purus eros. Ut eu lectus ac turpis ullamcorper " +
                    "auctor. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. " +
                    "Pellentesque sodales blandit sem sed mattis. Pellentesque congue ultrices scelerisque. " +
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi eget ligula et libero porttitor placerat. Nunc vehicula eleifend enim a egestas. Aenean aliquet, quam eu egestas elementum, leo felis luctus orci, vitae porttitor felis diam et neque. Ut mollis, sapien eget laoreet malesuada, quam mauris tincidunt metus, vitae consectetur sapien turpis at nisi. Proin mattis orci eu augue lacinia dictum. Proin eu nibh mi, a dignissim leo. Donec vel diam non mi consectetur dignissim. Curabitur ornare quam eu lorem malesuada pretium. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nunc elit lectus, pretium vitae sagittis sit amet, aliquet id neque. Maecenas enim nibh, dictum non vulputate in, vulputate et lectus. Ut dignissim erat sit amet nunc suscipit nec placerat lectus placerat. Suspendisse potenti. Etiam ac ante eu velit placerat egestas ac placerat orci. " + 
                    "Praesent eu dui neque, ut vestibulum lacus. Nunc commodo tortor nec tortor luctus at dapibus orci cursus. Duis vestibulum ipsum justo, et egestas arcu. Quisque tincidunt turpis non ligula varius tristique. Quisque vitae tortor nec purus elementum pharetra. Praesent laoreet augue in dolor iaculis dignissim. Sed ultricies, odio eu tempor pellentesque, leo justo luctus mi, vestibulum aliquet odio felis at nibh. Donec in velit sit amet leo aliquet iaculis non eget nisi. Vestibulum euismod pharetra neque quis elementum. Quisque ullamcorper elit egestas lacus tincidunt tincidunt. In iaculis cursus mi vitae feugiat. " +
                    "Aenean scelerisque pharetra mauris ut adipiscing. Curabitur vel quam sit amet leo euismod eleifend. Nullam adipiscing libero non lacus scelerisque ac laoreet purus mattis. Ut vel lobortis tortor. Etiam at ante neque. Proin ultrices elit non neque tristique a egestas libero aliquet. Morbi metus nisl, cursus semper tincidunt eget, vulputate pellentesque lorem. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Praesent nec augue ipsum. Etiam sollicitudin bibendum massa, a consequat urna cursus sit amet. Maecenas condimentum, nisi vel imperdiet mollis, enim dolor eleifend nisl, eu suscipit nisi sem non urna. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam metus justo, facilisis vitae sollicitudin ac, dapibus vitae quam. Fusce placerat, lectus vitae placerat tempus, metus augue convallis felis, ut tempor felis quam eu libero. Nam ligula diam, egestas id ultrices nec, tincidunt id mi. Vestibulum lobortis eros at felis congue consectetur. " + 
                    "Donec id euismod est. Nulla at erat eget erat feugiat ullamcorper. Nulla facilisi. Vivamus quis sem ac libero imperdiet blandit. Morbi id massa velit. Donec velit massa, bibendum vitae viverra quis, auctor ut sem. Aliquam in euismod lectus. Ut ac felis sit amet magna tempor egestas. Duis vitae elit sit amet nisl auctor pretium in eu libero. Fusce volutpat tempus ante eget sagittis. Phasellus vestibulum urna rhoncus risus ultrices facilisis. " +
                    "Integer magna nibh, imperdiet et feugiat id, convallis eu mi. Sed laoreet gravida diam. Morbi a odio vel augue consequat feugiat. Vivamus sed risus vel massa sagittis imperdiet vitae facilisis ipsum. Nunc suscipit turpis id massa commodo cursus. Fusce rutrum velit a quam volutpat vitae consequat leo faucibus. Pellentesque condimentum, sem ac sodales lacinia, lacus lectus aliquam erat, non congue felis sapien in lectus.";

            int outCount = 1;
            int length = testText.Length / directories.Length;
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = (string)SettingUtils.GetSetting(Setting.RootDirectoryName) + "\\" + directories[i];
                IsolatedStorageFile.GetUserStoreForApplication().CreateDirectory(dir);

                int outStart = (outCount - 1) * length;
                string s = testText.Substring(outStart, length);

                int count = 1;
                while (count * 100 < s.Length)
                {
                    int start = (count - 1) * 100;
                    string substring = s.Substring(start, 100);

                    using (IsolatedStorageFile myIso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string path = dir + "/Test Case " + count  + "-doc";
                        bool b = myIso.FileExists(path);
                        IsolatedStorageFileStream stream = myIso.OpenFile(path, FileMode.OpenOrCreate);
                        using (StreamWriter myWriter = new StreamWriter(stream))
                        {
                            myWriter.Write(substring);
                        }
                        stream.Close();
                    }
                    count++;
                }
                outCount++;
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new Delay.HybridOrientationChangesFrame();
            ((Delay.HybridOrientationChangesFrame)RootFrame).Duration = TimeSpan.FromSeconds(0.45);
            //((Delay.HybridOrientationChangesFrame)RootFrame).EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}