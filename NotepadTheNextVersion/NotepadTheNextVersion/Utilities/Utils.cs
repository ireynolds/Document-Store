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
using Microsoft.Phone.Shell;
using NotepadTheNextVersion.AppBars;
using Microsoft.Phone.Controls;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.ListItems;
using NotepadTheNextVersion.Models;
using System.Linq;
using System.Windows.Navigation;

namespace NotepadTheNextVersion.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Creates a new icon button with the given parameters
        /// </summary>
        /// <param name="text"></param>
        /// <param name="imagePath"></param>
        /// <param name="e">The click event handler for this button.</param>
        /// <returns></returns>
        public static ApplicationBarIconButton CreateIconButton(string text, string imagePath, EventHandler e)
        {
            ApplicationBarIconButton b = new ApplicationBarIconButton();
            b.IconUri = new Uri(imagePath, UriKind.Relative);
            b.Text = text;
            b.Click += e;
            return b;
        }

        /// <summary>
        /// Creates a new menu item with the given parameters
        /// </summary>
        /// <param name="text"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static ApplicationBarMenuItem CreateMenuItem(string text, EventHandler e)
        {
            ApplicationBarMenuItem b = new ApplicationBarMenuItem();
            b.Text = text;
            b.Click += e;
            return b;
        }

        /// <summary>
        /// Given a Path, uses the ".doc" and ".dir" tags to return the correct IActionable type.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IActionable CreateActionableFromPath(Models.PathStr p)
        {
            if (FileUtils.IsDoc(p.PathString))
                return new LDocument(p);
            else
                return new LDirectory(p);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemPathString">Unescaped.</param>
        /// <returns></returns>
        public static ShellTile GetTile(string ItemPathString)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(x =>
                {
                    var uri = x.NavigationUri.ToString();
                    var index = uri.IndexOf('=');
                    if (index != -1)
                    {
                        if (FileUtils.IsDoc(ItemPathString))
                        {
                            return uri.Substring(index + 1).StartsWith(Uri.EscapeDataString(ItemPathString) + "&istemp=");
                        }
                        else
                        {
                            return uri.Substring(index + 1).Equals(Uri.EscapeDataString(ItemPathString));
                        }
                    }
                    return false;
                });
        }

        public static void TryGoBack(NavigationService NavigationService)
        {
            TryGoBack(NavigationService, App.Listings);
        }

        public static void TryGoBack(NavigationService NavigationService, Uri DefaultLocation)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(DefaultLocation);
        }
    }
}
