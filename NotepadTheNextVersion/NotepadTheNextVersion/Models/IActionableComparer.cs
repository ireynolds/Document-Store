﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Models
{
    public class IActionableComparer : IComparer<Directory>
    {
        public int Compare(Directory a, Directory b)
        {
            return a.Path.PathString.CompareTo(b.Path.PathString);
        }
    }
}
