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

namespace NotepadTheNextVersion.Enumerations
{
    public enum Notice
    {
        Loading,
        Empty
    }

    public static class NoticeExtensions
    {
        public static string GetText(this Notice notice)
        {
            switch (notice)
            {
                case Notice.Loading:
                    return "Loading all items...";
                case Notice.Empty:
                    return "There are currently no items. Press the '+' icon below to add a new item";
                default:
                    throw new Exception("Unexpected enum type");
            }
        }
    }
}
