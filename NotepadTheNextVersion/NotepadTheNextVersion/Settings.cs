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
using NotepadTheNextVersion.StaticClasses;

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
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;

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
                return GetValueOrDefault<bool>(Setting.DisplayNoteTitle.Key(), (bool)SettingsUtils.GetSetting(Setting.DisplayNoteTitle));
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
                return GetValueOrDefault<bool>(Setting.OpenToFoldersList.Key(), (bool)SettingsUtils.GetSetting(Setting.OpenToFoldersList));
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
                return GetValueOrDefault<bool>(Setting.SearchFileText.Key(), (bool)SettingsUtils.GetSetting(Setting.SearchFileText));
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
                return GetValueOrDefault<ThemeColor>(Setting.NoteEditorThemeColor.Key(), (ThemeColor)SettingsUtils.GetSetting(Setting.NoteEditorThemeColor));
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
                return GetValueOrDefault<string>(Setting.RootDirectoryName.Key(), (string)SettingsUtils.GetSetting(Setting.RootDirectoryName));
            }
            set
            {
                AddOrUpdateValue(Setting.RootDirectoryName.Key(), value);
                Save();
            }
        }
    }
}
