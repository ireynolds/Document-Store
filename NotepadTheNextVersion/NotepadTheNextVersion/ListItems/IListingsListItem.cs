// Copyright (C) Isaac Reynolds. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using NotepadTheNextVersion.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using NotepadTheNextVersion.Utilities;
using System.Windows.Media.Animation;
using System.IO.IsolatedStorage;

namespace NotepadTheNextVersion.ListItems
{
    public abstract class IListingsListItem : StackPanel
    {
        private const int SHOW_CHECKBOX_DURATION = 100;
        private const int CHECKBOX_FADEIN_DURATION = 100;
        private const int HIDE_CHECKBOX_DURATION = 90;
        private const int CHECKBOX_FADEOUT_DURATION = 100;
        private static readonly IEasingFunction SHOW_CHECKBOX_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseInOut, Exponent = 2 };
        private static readonly IEasingFunction HIDE_CHECKBOX_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseInOut, Exponent = 2 };

        public readonly IActionable ActionableItem;
        protected readonly StackPanel _contentPanel;
        protected CheckBox _checkBox;
        protected Storyboard _displayCheckBoxStoryboard;
        protected Storyboard _hideCheckBoxStoryboard;
        protected double SLIDE_POSITION
        {
            get
            {
                // return -1 * (_checkBox.Margin.Left + _checkBox.DesiredSize.Width + _checkBox.Margin.Right);
                return -68; // hard-code because sometimes the size is zero
            }
        }

        protected bool _isSelectable;
        public bool IsSelectable
        {
            get
            {
                return _isSelectable;
            }
            set
            {
                OverrideHighlightColor();
                _isSelectable = value;

                if (_checkBox == null)
                    InitCheckBox();

                this.IsChecked = false;
                if (_isSelectable)
                    DisplayCheckBox();
                else
                    HideCheckBox();
            }
        }

        public event EventHandler IsSelectableAnimationCompleted
        {
            add
            {
                if (_hideCheckBoxStoryboard == null)
                    InitHideAnim();
                if (_displayCheckBoxStoryboard == null)
                    InitShowAnim();
                _hideCheckBoxStoryboard.Completed += value;
                _displayCheckBoxStoryboard.Completed += value;
            }
            remove
            {
                _hideCheckBoxStoryboard.Completed -= value;
                _hideCheckBoxStoryboard.Completed -= value;
            }
        }

        public bool IsChecked
        {
            get
            {
                return _checkBox != null && (bool)_checkBox.IsChecked;
            }
            set
            {
                _checkBox.IsChecked = value;
            }
        }

        protected IListingsListItem(IActionable a)
        {
            ActionableItem = a;
            this.Orientation = Orientation.Horizontal;
            this.RenderTransform = new CompositeTransform();
            
            _contentPanel = new StackPanel();
            _contentPanel.Orientation = Orientation.Horizontal;
            this.Children.Add(_contentPanel);
        }

        public abstract ICollection<UIElement> GetNotAnimatedItemsReference();

        public abstract UIElement GetAnimatedItemReference();

        public static IListingsListItem CreateListItem(IActionable a)
        {
            if (a is Document)
                return new DocumentListItem(a);
            else if (a is Directory)
                return new DirectoryListItem(a);
            else
                throw new Exception("Unexpected type");
        }

        protected virtual void InitCheckBox()
        {
            _checkBox = new CheckBox();
            _checkBox.RenderTransform = new CompositeTransform();
            _checkBox.Checked += (object sender, RoutedEventArgs e) =>
            {
                if (this.Parent != null)
                    (this.Parent as ListBox).SelectedItems.Add(this);
            };
            _checkBox.Unchecked += (object sender, RoutedEventArgs e) =>
            {
                if (this.Parent != null)
                    (this.Parent as ListBox).SelectedItems.Remove(this);
            };
            this.Children.Insert(0, _checkBox);
        }

        private void DisplayCheckBox()
        {
            if (_displayCheckBoxStoryboard == null)
            {
                InitShowAnim();
            }
            _checkBox.Visibility = Visibility.Visible;
            _displayCheckBoxStoryboard.Begin();
        }

        private void InitShowAnim()
        {
            _displayCheckBoxStoryboard = new Storyboard();

            DoubleAnimation slide = AnimationUtils.TranslateX(SLIDE_POSITION, 0, SHOW_CHECKBOX_DURATION, SHOW_CHECKBOX_EASE);
            Storyboard.SetTarget(slide, this);
            _displayCheckBoxStoryboard.Children.Add(slide);

            DoubleAnimation fade = AnimationUtils.FadeIn(CHECKBOX_FADEIN_DURATION);
            Storyboard.SetTarget(fade, _checkBox);
            _displayCheckBoxStoryboard.Children.Add(fade);
        }

        private void HideCheckBox()
        {
            if (_hideCheckBoxStoryboard == null)
            {
                InitHideAnim();
            }
            _hideCheckBoxStoryboard.Begin();
        }

        private void InitHideAnim()
        {
            _hideCheckBoxStoryboard = new Storyboard();
            _hideCheckBoxStoryboard.Completed += (object sender, EventArgs e) =>
            {
                _checkBox.Visibility = Visibility.Collapsed;
                _hideCheckBoxStoryboard.Stop();
            };

            DoubleAnimation slide = AnimationUtils.TranslateX(0, SLIDE_POSITION, HIDE_CHECKBOX_DURATION, HIDE_CHECKBOX_EASE);
            Storyboard.SetTarget(slide, this);
            _hideCheckBoxStoryboard.Children.Add(slide);

            DoubleAnimation fade = AnimationUtils.FadeOut(CHECKBOX_FADEOUT_DURATION);
            Storyboard.SetTarget(fade, _checkBox);
            _hideCheckBoxStoryboard.Children.Add(fade);
            _hideCheckBoxStoryboard.Completed += new EventHandler((object sender, EventArgs e) =>
            {
                _checkBox.Visibility = Visibility.Collapsed;
            });
        }

        protected abstract void OverrideHighlightColor();
    }
}
