using System;
using NotepadTheNextVersion.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Views
{
    public abstract class ListingsListItem : StackPanel
    {
        // Actionable
        public readonly IActionable ActionableItem;

        //public bool HasVariableOpacity;

        //public static readonly DependencyProperty ListingOpacityProperty = DependencyProperty.Register("ListingOpacity", typeof(double), typeof(ListingsListItem), null);
        //public double ListingOpacity
        //{
        //    get
        //    {
        //        return (double)GetValue(ListingOpacityProperty);
        //    }
        //    set
        //    {
        //        if (HasVariableOpacity)
        //            SetValue(ListingOpacityProperty, value);
        //    }
        //}

        protected ListingsListItem(IActionable a)
        {
            // Initialize fields
            ActionableItem = a;
            this.Orientation = Orientation.Horizontal;
        }

        public abstract ICollection<UIElement> GetNotAnimatedItemsReference();

        public abstract UIElement GetAnimatedItemReference();

        public static ListingsListItem CreateListItem(IActionable a)
        {
            if (a is Document)
                return new DocumentListItem(a);
            else if (a is Directory)
                return new DirectoryListItem(a);
            else
                throw new Exception("Unexpected type");
        }
    }
}
