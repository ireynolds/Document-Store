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
using System.Windows.Threading;
using System.Collections.Generic;
using NotepadTheNextVersion.ListItems;
using NotepadTheNextVersion.Enumerations;
using NotepadTheNextVersion.Utilities;

namespace NotepadTheNextVersion.Models
{
    public class TimedItemAnimator
    {
        private IList<IListingsListItem> _items;
        private DispatcherTimer _timer;
        private event AnimateEvent _forEachItem;
        private event CompletedEvent _completed;

        public IList<IListingsListItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        public delegate void AnimateEvent(IListingsListItem item);
        public event AnimateEvent ForEachItem
        {
            add
            {
                _forEachItem += value;
            }
            remove
            {
                _forEachItem -= value;
            }
        }

        private int _count;
        private IListingsListItem _nextItem
        {
            get
            {
                if (_items == null)
                    throw new InvalidOperationException();
                if (_count >= _items.Count)
                    return null;
                return _items[_count++];
            }
        }

        public delegate void CompletedEvent(IList<IListingsListItem> itemsNotAdded);
        public event CompletedEvent Completed
        {
            add
            {
                _completed += value;
            }
            remove
            {
                _completed -= value;
            }
        }

        public TimeSpan Interval
        {
            get
            {
                return _timer.Interval;
            }
            set
            {
                if (!_timer.Interval.Equals(value))
                    _timer.Interval = value;
            }
        }

        public TimedItemAnimator(IList<IListingsListItem> items)
        {
            _items = items;
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(DoWork);

            _timer.Interval = TimeSpan.FromMilliseconds(70);
        }

        private void DoWork(object sender, EventArgs e)
        {
            var item = _nextItem;
            if (item != null)
                _forEachItem(item);
            else
            {
                Stop();
                FireCompleted();
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void FireCompleted()
        {
            if (_completed == null)
                return;

            IList<IListingsListItem> lst = new List<IListingsListItem>();
            for (int i = _count; i < _items.Count; i++)
                lst.Add(_items[i]);
            _completed(lst);
        }

        public void Start()
        {
            ParamUtils.CheckForNull(_items, "items");
            ParamUtils.CheckForNull(_forEachItem, "ForEachItem");
            _timer.Start();
        }
    }
}
