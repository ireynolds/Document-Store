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
using System.Collections.ObjectModel;

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
            var foreground = SettingUtils.GetSetting<ThemeColor>(Setting.NoteEditorThemeColor);
            switch (foreground)
            {
                case ThemeColor.dark:
                    return new SolidColorBrush(Colors.White);
                case ThemeColor.light:
                    return new SolidColorBrush(Colors.Black);
                case ThemeColor.phone:
                    return (SolidColorBrush)App.AppSettings["PhoneBackgroundBrush"];
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
            return SettingUtils.GetSetting<ThemeColor>(Setting.NoteEditorThemeColor).Brush();
        }

        /// <summary>
        /// Returns the user-set value for the given setting, or the default setting if no
        /// user-set value exists.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static T GetSetting<T>(Setting setting)
        {
            if (App.AppSettings.Contains(setting.Key()))
                return (T)App.AppSettings[setting.Key()];
            else
                return (T)DefaultValue(setting);
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
                    return "root" + FileUtils.DIRECTORY_EXTENSION;
                case Setting.FavoritesList:
                    if (!App.AppSettings.Contains(setting.Key()))
                        App.AppSettings.Add(setting.Key(), new Collection<string>());
                    return App.AppSettings[setting.Key()];
                case Setting.ShowHiddenItems:
                    return false;
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }
    }
}
