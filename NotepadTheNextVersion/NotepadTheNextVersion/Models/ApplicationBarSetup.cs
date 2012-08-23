// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Shell;
using NotepadTheNextVersion.Enumerations;
using System.Windows.Controls;
using NotepadTheNextVersion.ListItems;
using Microsoft.Phone.Controls;
using NotepadTheNextVersion.Models;

namespace NotepadTheNextVersion.AppBars
{
    public abstract class ApplicationBarSetup
    {
        /// <summary>
        /// Contains the ApplicationBar that this manages.
        /// </summary>
        protected ApplicationBar _appBar;

        /// <summary>
        /// Contains a reference to the page on which this appbar sits.
        /// </summary>
        protected Listings _page;

        /// <summary>
        /// A List of all buttons in the order they appear (left to right).
        /// </summary>
        protected ButtonList _buttons;

        /// <summary>
        /// A List of all menu items in the order they appear (top to bottom).
        /// </summary>
        protected ItemList _menuItems;

        /// <summary>
        /// Gets a reference to the ApplicationBar that this manages.
        /// </summary>
        public ApplicationBar AppBar
        {
            get
            {
                return _appBar;
            }
        }
        
        /// <summary>
        /// Creates a new ApplicationBarSetup and initializes _appBar.
        /// </summary>
        protected ApplicationBarSetup(Listings Page)
        {
            _appBar = new ApplicationBar();
            _page = Page;
        }

        protected void Invoke(Action a)
        {
            _page.Dispatcher.BeginInvoke(a);
        }

        /// <summary>
        /// If this ApplicationBar's items/buttons change according to the selection in a ListBox
        /// on the page, add this to that ListBox's SelectionChanged event. This will change the
        /// relevant items/buttons on the application bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual void SelectedItemsChanged(object sender, SelectionChangedEventArgs e) {}

        /// <summary>
        /// Clears the current Application Bar and adds the given elements.
        /// </summary>
        /// <param name="AppBar"></param>
        /// <param name="Buttons"></param>
        /// <param name="MenuItems"></param>
        public static void SetElements(ApplicationBar AppBar, ButtonList Buttons, ItemList MenuItems)
        {
            ClearElements(AppBar);
            foreach (ApplicationBarIconButton b in Buttons)
                AppBar.Buttons.Add(b);
            foreach (ApplicationBarMenuItem i in MenuItems)
                AppBar.MenuItems.Add(i);
        }

        /// <summary>
        /// Sets the Enabled property of the given elements to the value of the given flag.
        /// </summary>
        /// <param name="AppBar"></param>
        /// <param name="IsEnabledFlag"></param>
        /// <param name="Buttons"></param>
        /// <param name="MenuItems"></param>
        public static void SetEnabledElements(bool IsEnabledFlag, ICollection<ApplicationBarIconButton> Buttons, ICollection<ApplicationBarMenuItem> MenuItems)
        {
            foreach (ApplicationBarIconButton b in Buttons)
                b.IsEnabled = IsEnabledFlag;
            foreach (ApplicationBarMenuItem i in MenuItems)
                i.IsEnabled = IsEnabledFlag;
        }

        /// <summary>
        /// Clears all buttons and menu items out of the given ApplicationBar.
        /// </summary>
        /// <param name="AppBar"></param>
        public static void ClearElements(ApplicationBar AppBar)
        {
            AppBar.Buttons.Clear();
            AppBar.MenuItems.Clear();
        }

        /// <summary>
        /// Sets the IsEnabled property of each button and item in the given ApplicationBar
        /// to the given bool.
        /// </summary>
        /// <param name="AppBar"></param>
        /// <param name="IsEnabledFlag"></param>
        public static void SetAllEnabled(ApplicationBar AppBar, bool IsEnabledFlag)
        {
            foreach (ApplicationBarIconButton b in AppBar.Buttons)
                b.IsEnabled = IsEnabledFlag;
            foreach (ApplicationBarMenuItem i in AppBar.MenuItems)
                i.IsEnabled = IsEnabledFlag;
        }
    }
}
