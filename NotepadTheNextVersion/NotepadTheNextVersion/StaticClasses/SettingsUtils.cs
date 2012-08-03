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

namespace NotepadTheNextVersion.StaticClasses
{
    public static class SettingsUtils
    {
        public static SolidColorBrush GetUserSetForegroundBrush()
        {
            ThemeColor foreground = (ThemeColor)SettingsUtils.GetSetting(Setting.NoteEditorThemeColor);
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

        public static SolidColorBrush GetUserSetBackgroundBrush()
        {
            return ((ThemeColor)SettingsUtils.GetSetting(Setting.NoteEditorThemeColor)).Brush();
        }

        public static object GetSetting(Setting setting)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(setting.Key()))
                return IsolatedStorageSettings.ApplicationSettings[setting.Key()];
            else
                return DefaultValue(setting);
        }

        private static object DefaultValue(Setting setting)
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
                    return "home";
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }
    }
}
