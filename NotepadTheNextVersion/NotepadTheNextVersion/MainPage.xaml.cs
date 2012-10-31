// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

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
                (new LDirectory(PathBase.Root)).Open(NavigationService);
            }
            else
            {
                CreateTempFile().Open(NavigationService);
            }
        }

        private LDocument CreateTempFile()
        {
            LDirectory root = new LDirectory(PathBase.Root);
            string path = FileUtils.GetNumberedDocumentPath("Temp", root.Path.PathString);
            var doc = new LDocument(new PathStr(path)) { IsTemp = true };
            FileUtils.CreateDocument(doc.Path.Parent.PathString, doc.DisplayName);
            return doc;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }
    }
}