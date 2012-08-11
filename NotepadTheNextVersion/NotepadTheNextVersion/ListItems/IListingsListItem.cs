using System;
using NotepadTheNextVersion.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using NotepadTheNextVersion.Utilities;
using System.Windows.Media.Animation;

namespace NotepadTheNextVersion.ListItems
{
    public abstract class IListingsListItem : StackPanel
    {
        private const int SHOW_CHECKBOX_DURATION = 150;
        private const int CHECKBOX_FADEIN_DURATION = 125;
        private const int HIDE_CHECKBOX_DURATION = 100;
        private const int CHECKBOX_FADEOUT_DURATION = 125;
        private static readonly IEasingFunction SHOW_CHECKBOX_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 4 };
        private static readonly IEasingFunction HIDE_CHECKBOX_EASE = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 3 };

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

        public bool IsChecked
        {
            get
            {
                return (bool)_checkBox.IsChecked;
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
                _displayCheckBoxStoryboard = new Storyboard();

                DoubleAnimation slide = AnimationUtils.TranslateX(SLIDE_POSITION, 0, SHOW_CHECKBOX_DURATION, SHOW_CHECKBOX_EASE);
                Storyboard.SetTarget(slide, this);
                _displayCheckBoxStoryboard.Children.Add(slide);

                DoubleAnimation fade = AnimationUtils.FadeIn(CHECKBOX_FADEIN_DURATION);
                Storyboard.SetTarget(fade, _checkBox);
                _displayCheckBoxStoryboard.Children.Add(fade);
            }
            _checkBox.Visibility = Visibility.Visible;
            _displayCheckBoxStoryboard.Begin();
        }

        private void HideCheckBox()
        {
            if (_hideCheckBoxStoryboard == null)
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
            }
            _hideCheckBoxStoryboard.Completed +=new EventHandler((object sender, EventArgs e) =>
            {
                //this.Children.Remove(_checkBox);
                _checkBox.Visibility = Visibility.Collapsed;
            });
            _hideCheckBoxStoryboard.Begin();
        }
    }
}
