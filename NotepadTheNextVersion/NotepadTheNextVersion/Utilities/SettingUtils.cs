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
using NotepadTheNextVersion.Enumerations;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Utilities
{
    public static class SettingUtils
    {
        /// <summary>
        /// Returns the user-set foreground brush for use in the note editor.
        /// </summary>
        /// <returns></returns>
        public static SolidColorBrush GetUserSetForegroundBrush()
        {
            ThemeColor foreground = (ThemeColor)SettingUtils.GetSetting(Setting.NoteEditorThemeColor);
            switch (foreground)
            {
                case ThemeColor.dark:
                    return new SolidColorBrush(Colors.White);
                case ThemeColor.light:
                    return new SolidColorBrush(Colors.Black);
                case ThemeColor.phone:
                    return (SolidColorBrush)IsolatedStorageSettings.ApplicationSettings["PhoneBackgroundBrush"];
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }

        /// <summary>
        /// Returns the user-set background brush for use in the note editor.
        /// </summary>
        /// <returns></returns>
        public static SolidColorBrush GetUserSetBackgroundBrush()
        {
            return ((ThemeColor)SettingUtils.GetSetting(Setting.NoteEditorThemeColor)).Brush();
        }

        /// <summary>
        /// Returns the user-set value for the given setting, or the default setting if no
        /// user-set value exists.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static object GetSetting(Setting setting)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(setting.Key()))
                return IsolatedStorageSettings.ApplicationSettings[setting.Key()];
            else
                return DefaultValue(setting);
        }

        /// <summary>
        /// Returns the default value for a given setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static object DefaultValue(Setting setting)
        {
            switch (setting)
            {
                case Setting.DisplayNoteTitle:
                    return true;
                case Setting.OpenToFoldersList:
                    return false;
                case Setting.SearchFileText:
                    return true;
                case Setting.NoteEditorThemeColor:
                    return ThemeColor.light;
                case Setting.RootDirectoryName:
                    return "home-dir";
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }
    }
}
