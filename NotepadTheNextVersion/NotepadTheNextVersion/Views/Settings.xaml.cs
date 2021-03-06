﻿// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Models;
using System;

namespace NotepadTheNextVersion.ListItems
{
    public partial class Settings : PhoneApplicationPage
    {
        private NotepadTheNextVersion.Settings _appSettings;
        private TextBox _homeDirectoryNameTextBox;

        public Settings()
        {
            InitializeComponent();
            _appSettings = new NotepadTheNextVersion.Settings();
            this.Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, EventArgs e)
        {
            this.Loaded -= PageLoaded;
            UpdateView();
        }

        private void UpdateView()
        {
            SettingsPanel.Children.Add(CreateBoundToggleSwitch("Open to document explorer", "OpenToFoldersList"));
            SettingsPanel.Children.Add(CreateBoundToggleSwitch("Also search document text", "SearchFileText"));
            SettingsPanel.Children.Add(CreateBoundToggleSwitch("Show document title in editor", "DisplayNoteTitle"));
            SettingsPanel.Children.Add(CreateBoundToggleSwitch("Show hidden items", "ShowHiddenItems"));
            SettingsPanel.Children.Add(CreateDescriptionBlock("To hide a document or directory, preface its name with a period."));
            //SettingsPanel.Children.Add(new TextBlock()
            //{
            //    Text = "Select new name for the home directory",
            //    Style = (Style)App.Current.Resources["PhoneTextSubtleStyle"]
            //});
            //SettingsPanel.Children.Add(CreateRootNameTextBox());
            SettingsPanel.Children.Add(CreateThemeColorListPicker());
            SettingsPanel.Children.Add(CreateDescriptionBlock("The color you select will be the background color " + 
                "of the document editor. The 'phone theme' setting sets the background color according to the " + 
                "phone's theme."));
        }

        private TextBox CreateRootNameTextBox()
        {
            _homeDirectoryNameTextBox = new TextBox();
            _homeDirectoryNameTextBox.Margin = new Thickness(0, 0, 0, 35);
            _homeDirectoryNameTextBox.LostFocus += new RoutedEventHandler(HomeDirectoryNameTextBox_LostFocus);
            _homeDirectoryNameTextBox.Text = _appSettings.RootDirectoryDisplayName;
            return _homeDirectoryNameTextBox;
        }

        private ListPicker CreateThemeColorListPicker()
        {
            ListPicker p = new ListPicker();
            p.Header = "Select document editor background color";
            p.Margin = new Thickness(12, 35, 12, 30);
            p.Items.Add(ThemeColor.dark);
            p.Items.Add(ThemeColor.light);
            p.Items.Add(ThemeColor.phone);
            p.SetBinding(ListPicker.SelectedItemProperty, GetBinding("NoteEditorThemeColor"));
            return p;
        }

        private ToggleSwitch CreateBoundToggleSwitch(string header, string propertyPath)
        {
            ToggleSwitch ts = new ToggleSwitch()
            {
                Header = header,
                Margin = new Thickness(0, 0, 0, 10)
            };
            ts.SetBinding(ToggleSwitch.IsCheckedProperty, GetBinding(propertyPath));
            return ts;
        }

        private TextBlock CreateDescriptionBlock(string text)
        {
            TextBlock tb = new TextBlock()
            {
                Text = text,
                FontSize = 22,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(12, 0, 12, 0)
            };
            return tb;
        }

        private Binding GetBinding(string propertyPath)
        {
            Binding b = new Binding();
            b.Path = new PropertyPath(propertyPath);
            b.Mode = BindingMode.TwoWay;
            b.Source = _appSettings;
            return b;
        }

        private void HomeDirectoryNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var oldName = _appSettings.RootDirectoryDisplayName;
            string newName = _homeDirectoryNameTextBox.Text;
            if (oldName.Equals(newName))
                return;
            
            MessageBoxResult r = MessageBox.Show("If this operation fails, it may delete all of your data. You should export all your documents first.\r\rPress OK to continue, or Cancel to quit.",
                "Warning", MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.Cancel)
            {
                _homeDirectoryNameTextBox.Text = oldName;
                return;
            }

            try
            {
                oldName = System.IO.Path.Combine(oldName + FileUtils.DIRECTORY_EXTENSION, "");
                newName = System.IO.Path.Combine(newName + FileUtils.DIRECTORY_EXTENSION, "");

                if (FileUtils.DirectoryExists(newName))
                    (new Directory(new PathStr(newName))).Delete(true);
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isf.MoveDirectory(oldName, newName);
                }
                _appSettings.AddOrUpdateValue(Setting.RootDirectoryName.Key(), newName);                    
            }
            catch (IsolatedStorageException)
            {
                MessageBox.Show("Document Store was unable to change the name of the home directory. Please try again later.", "An error occurred", MessageBoxButton.OK);
            }
        }
    }
}