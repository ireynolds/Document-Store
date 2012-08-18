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

        // Our isolated storage settings
        IsolatedStorageSettings isolatedStore; 

        // Constructor that gets the application settings.
        public Settings()
        {
            try
            {
                // Get the settings for this application.
                isolatedStore = App.AppSettings;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }
        }

        // Update a setting value for our application. If the setting does not
        // exist, then add the setting.
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (isolatedStore.Contains(Key))
            {
                // If the value has changed
                if (isolatedStore[Key] != value)
                {
                    // Store the new value
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }

           return valueChanged;
        }



        // Get the current value of the setting, or if it is not found, set the 
        // setting to the default setting.
        public valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;

            // If the key exists, retrieve the value.
            if (isolatedStore.Contains(Key))
            {
                value = (valueType)isolatedStore[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }



        // Save the settings.
        public void Save()
        {
            isolatedStore.Save();
        }



        // Property to get and set a DisplayNoteTitle Setting Key.
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

        // Property to get and set an OpenToFoldersList Setting Key.
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

        // Property to get and set a SearchFileText Setting Key.
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

        // Property to get and set a NoteEditorThemeColor Setting Key.
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
    }
}
