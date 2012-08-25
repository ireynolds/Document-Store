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

namespace NotepadTheNextVersion.Utilities
{
    public static class StringUtils
    {
        /// <summary>
        /// Returns true if the given string is null or whitespace characters.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(string s)
        {
            if (s == null)
                return true;
            return s.Trim().Equals(string.Empty);
        }

        /// <summary>
        /// Returns true if the parameter string are value-equal (ignoring case).
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(string thisString, string other)
        {
            return thisString.ToLower().Equals(other.ToLower());
        }

        /// <summary>
        /// Returns true if the first parameter starts with the second parameter.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool StartsWithIgnoreCase(string thisString, string other)
        {
            return thisString.ToLower().StartsWith(other.ToLower());
        }
    }
}
