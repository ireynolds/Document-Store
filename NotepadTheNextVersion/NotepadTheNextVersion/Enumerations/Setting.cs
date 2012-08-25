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

namespace NotepadTheNextVersion.Enumerations
{
    public enum Setting
    {
        DisplayNoteTitle,
        OpenToFoldersList,
        SearchFileText,
        NoteEditorThemeColor,
        RootDirectoryName,
        FavoritesList,
        ShowHiddenItems
    }

    public static class Extensions2
    {
        public static string Key(this Setting setting)
        {
            switch (setting)
            {
                case Setting.DisplayNoteTitle:
                    return "DisplayNoteTitleSetting";
                case Setting.OpenToFoldersList:
                    return "OpenToFoldersListSetting";
                case Setting.SearchFileText:
                    return "SearchFileTextSetting";
                case Setting.NoteEditorThemeColor:
                    return "NoteEditorThemeColorSetting";
                case Setting.RootDirectoryName:
                    return "RootDirectoryNameSetting";
                case Setting.FavoritesList:
                    return "FavoritesList";
                case Setting.ShowHiddenItems:
                    return "ShowHiddenItems";
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }
    }
}
