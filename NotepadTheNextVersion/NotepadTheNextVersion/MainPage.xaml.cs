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
            var NavToDocs = SettingUtils.GetSetting<bool>(Setting.OpenToFoldersList);
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
            string path = FileUtils.GetNumberedDocumentPath("Temp", root.Path.PathString);
            var doc = new Document(new PathStr(path)) { IsTemp = true };
            FileUtils.CreateDocument(doc.Path.Parent.PathString, doc.DisplayName);
            return doc;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }
    }
}