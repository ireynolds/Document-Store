// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.Utilities;
using System.Collections.ObjectModel;

namespace NotepadTheNextVersion
{

    public class Settings
    {
        IsolatedStorageSettings isolatedStore; 

        public Settings()
        {
            isolatedStore = App.AppSettings;
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            if (isolatedStore.Contains(Key))
            {
                if (isolatedStore[Key] != value)
                {
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }

           return valueChanged;
        }



        public valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;

            if (isolatedStore.Contains(Key))
            {
                value = (valueType)isolatedStore[Key];
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }



        public void Save()
        {
            isolatedStore.Save();
        }



        public bool DisplayNoteTitle
        {
            get
            {
                return GetValueOrDefault<bool>(Setting.DisplayNoteTitle.Key(), SettingUtils.GetSetting<bool>(Setting.DisplayNoteTitle));
            }
            set
            {
                AddOrUpdateValue(Setting.DisplayNoteTitle.Key(), value);
                Save();
            }
        }

        public bool OpenToFoldersList
        {
            get
            {
                return GetValueOrDefault<bool>(Setting.OpenToFoldersList.Key(), SettingUtils.GetSetting<bool>(Setting.OpenToFoldersList));
            }
            set
            {
                AddOrUpdateValue(Setting.OpenToFoldersList.Key(), value);
                Save();
            }
        }

        public bool SearchFileText
        {
            get
            {
                return GetValueOrDefault<bool>(Setting.SearchFileText.Key(), SettingUtils.GetSetting<bool>(Setting.SearchFileText));
            }
            set
            {
                AddOrUpdateValue(Setting.SearchFileText.Key(), value);
                Save();
            }
        }

        public ThemeColor NoteEditorThemeColor
        {
            get
            {
                return GetValueOrDefault<ThemeColor>(Setting.NoteEditorThemeColor.Key(), SettingUtils.GetSetting<ThemeColor>(Setting.NoteEditorThemeColor));
            }
            set
            {
                AddOrUpdateValue(Setting.NoteEditorThemeColor.Key(), value);
                Save();
            }
        }

        public string RootDirectoryName
        {
            get
            {
                return GetValueOrDefault<string>(Setting.RootDirectoryName.Key(), SettingUtils.GetSetting<string>(Setting.RootDirectoryName));
            }
            set
            {
                AddOrUpdateValue(Setting.RootDirectoryName.Key(), value);
                Save();
            }
        }

        public Collection<string> FavoritesList
        {
            get
            {
                return GetValueOrDefault<Collection<string>>(Setting.FavoritesList.Key(), SettingUtils.GetSetting<Collection<string>>(Setting.FavoritesList));
            }
            set
            {
                AddOrUpdateValue(Setting.FavoritesList.Key(), value);
                Save();
            }
        }

        public string RootDirectoryDisplayName
        {
            get
            {
                var val = GetValueOrDefault<string>(Setting.RootDirectoryName.Key(), SettingUtils.GetSetting<string>(Setting.RootDirectoryName));
                val = val.Substring(0, val.Length - 4);
                return val;
            }
            set
            {
                AddOrUpdateValue(Setting.RootDirectoryName.Key(), value + FileUtils.DIRECTORY_EXTENSION);
                Save();
            }
        }

        public bool ShowHiddenItems
        {
            get
            {
                return GetValueOrDefault<bool>(Setting.ShowHiddenItems.Key(), SettingUtils.GetSetting<bool>(Setting.ShowHiddenItems));
            }
            set
            {
                AddOrUpdateValue(Setting.ShowHiddenItems.Key(), value);
                Save();
            }
        }
    }
}
