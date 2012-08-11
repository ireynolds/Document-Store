using Microsoft.Phone.Controls;
using System;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.Enumerations;
using System.IO.IsolatedStorage;

namespace NotepadTheNextVersion
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            //InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            bool NavToDocs = (bool)SettingUtils.GetSetting(Setting.OpenToFoldersList);
            if (NavToDocs)
            {
                (new Directory(PathBase.Root)).Open(NavigationService);
            }
            else
            {
                CreateTempFile().Open(NavigationService);
            }
        }

        private Document CreateTempFile()
        {
            Directory root = new Directory(PathBase.Root);
            string name = FileUtils.GetNumberedName("Temp", root);
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Document d = new Document(root.Path.NavigateIn(name)) { IsTemp = true };
                IsolatedStorageFileStream f = isf.CreateFile(d.Path.PathString);
                f.Close();
                return d;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }
    }
}