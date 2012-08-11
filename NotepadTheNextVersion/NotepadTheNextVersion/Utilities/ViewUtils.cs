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

namespace NotepadTheNextVersion.Utilities
{
    public static class ViewUtils
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
    }
}
