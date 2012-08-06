using System;
using NotepadTheNextVersion.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace NotepadTheNextVersion.ListItems
{
    public abstract class IListingsListItem : StackPanel
    {
        public readonly IActionable ActionableItem;

        protected IListingsListItem(IActionable a)
        {
            ActionableItem = a;
            this.Orientation = Orientation.Horizontal;
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
    }
}
