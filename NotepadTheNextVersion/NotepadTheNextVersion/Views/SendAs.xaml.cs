// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using NotepadTheNextVersion.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using NotepadTheNextVersion.Utilities;
using NotepadTheNextVersion.Enumerations;

namespace NotepadTheNextVersion.ListItems
{
    public partial class SendAs : PhoneApplicationPage
    {
        private Document _currentDocument;
        private bool _hasBeenNavigatedTo;

        private SolidColorBrush _background;
        private SolidColorBrush _foreground;

        public SendAs()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_hasBeenNavigatedTo)
                Utils.TryGoBack(NavigationService);
            GetArgs();
            UpdateView();
            base.OnNavigatedTo(e);
        }

        #region Event Handlers

        private void Email_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask t = new EmailComposeTask();
            t.Body = String.Format("==== {0} ====\n\n{1}", _currentDocument.Path.PathString, _currentDocument.Text);
            t.Show();
            _hasBeenNavigatedTo = true;
        }

        private void Sms_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SmsComposeTask t = new SmsComposeTask();
            t.Body = _currentDocument.Text;
            t.Show();
            _hasBeenNavigatedTo = true;
        }

        #endregion

        #region Private Helpers

        // Adds UI elements to the page
        private void UpdateView()
        {
            _background = SettingUtils.GetUserSetBackgroundBrush();
            _foreground = SettingUtils.GetUserSetForegroundBrush();

            TextBlock email = new TextBlock();
            TextBlock sms = new TextBlock();
            
            LayoutRoot.Children.Add(sms);
            LayoutRoot.Children.Add(email);
            LayoutRoot.Background = _background;

            email.Text = "email";
            email.FontSize = 50;
            email.Foreground = _foreground;
            email.Margin = new Thickness(24, 12, 0, 0);
            email.FontFamily = new FontFamily("Segoe WP SemiLight");
            email.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(Email_Tap);

            sms.Text = "sms";
            sms.FontSize = 50;
            sms.Foreground = _foreground;
            sms.Margin = new Thickness(24, 0, 0, 0);
            sms.FontFamily = new FontFamily("Segoe WP SemiLight");
            sms.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(Sms_Tap);
        }

        private void GetArgs()
        {
            _currentDocument = (Document)Utils.CreateActionableFromPath(new PathStr(NavigationContext.QueryString["param"]));
        }

        #endregion
    }
}