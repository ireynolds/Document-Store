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
        FavoritesList
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
                default:
                    throw new Exception("Unrecognized enum type");
            }
        }
    }
}
