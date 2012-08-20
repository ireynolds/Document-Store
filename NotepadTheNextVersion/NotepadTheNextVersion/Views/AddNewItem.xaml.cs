﻿using System;
using System.IO.IsolatedStorage;
using NotepadTheNextVersion.Models;
using NotepadTheNextVersion.Utilities;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace NotepadTheNextVersion.ListItems
{
    // Creates a new document or directory in the current space. The name is determined by a GUID
    // (to minimize collisions) and prefaced by a ".". The "." prevents the item from being displayed
    // in the listings page.
    //
    // Parameters
    //   Directory  the directory in which to create the new item
    public partial class AddNewItem : PhoneApplicationPage
    {
        // The parent directory of the to-be-created item
        private Directory _currentDirectory;

        public AddNewItem()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);
            GetArgs();
            UpdateView();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationService.RemoveBackEntry();
        }

        #region Event Handlers

        private void Dir_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {    
            // Create a temporary directory that will be renamed in the new window
            string tempPath = FileUtils.GetNumberedDirectoryPath("Untitled", _currentDirectory.Path.PathString);
            Directory newDirectory = new Directory(new PathStr(tempPath)) { IsTemp = true };
            FileUtils.CreateDirectory(newDirectory.Path.Parent.PathString, newDirectory.DisplayName);
            newDirectory.NavToRename(NavigationService);
        }

        private void Doc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Create a temporary document that will be renamed in the next window
            string tempPath = FileUtils.GetNumberedDocumentPath("Untitled", _currentDirectory.Path.PathString);
            Document newDocument = new Document(new PathStr(tempPath)) { IsTemp = true };
            FileUtils.CreateDocument(newDocument.Path.Parent.PathString, newDocument.DisplayName);
            newDocument.NavToRename(NavigationService);
        }

        #endregion

        #region Private Helpers

        // Adds UI elements to the page
        private void UpdateView()
        {
            TextBlock label = new TextBlock();
            TextBlock dir = new TextBlock();
            TextBlock doc = new TextBlock();

            LayoutRoot.Margin = new Thickness(12, 0, 12, 0);
            LayoutRoot.Children.Add(label);
            LayoutRoot.Children.Add(doc);
            LayoutRoot.Children.Add(dir);

            label.Text = "NEW";
            label.Style = (Style)App.Current.Resources["PhoneTextNormalStyle"];
            label.Margin = new Thickness(12, 17, 0, 15);

            dir.Text = "directory";
            dir.FontSize = 50;
            dir.Margin = new Thickness(12, 0, 0, 0);
            dir.FontFamily = new FontFamily("Segoe WP SemiLight");
            dir.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(Dir_Tap);

            doc.Text = "document";
            doc.FontSize = 50;
            doc.Margin = new Thickness(12, 0, 0, 0);
            doc.FontFamily = new FontFamily("Segoe WP SemiLight");
            doc.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(Doc_Tap);
        }

        private void GetArgs()
        {
            IList<IActionable> args = ParamUtils.GetArguments();
            _currentDirectory = (Directory)args[0];
        }

        #endregion
    }
}