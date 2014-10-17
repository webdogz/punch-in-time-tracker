using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PunchIn.ViewModels
{
    public class ObservableElementCollection<T> : ObservableCollection<T>
    {
        public ObservableElementCollection() : base() { }
        public ObservableElementCollection(List<T> list) : base(list) { }
        public class ChildElementPropertyChangedEventArgs : EventArgs
        {
            public object ChildElement { get; set; }
            public ChildElementPropertyChangedEventArgs(object item)
            {
                ChildElement = item;
            }
        }
        public delegate void ChildElementPropertyChangedEventHandler(ChildElementPropertyChangedEventArgs e);
        public event ChildElementPropertyChangedEventHandler ChildElementPropertyChanged;
        private void OnChildElementPropertyChanged(object childelement)
        {
            if (ChildElementPropertyChanged != null)
            {
                ChildElementPropertyChanged(new ChildElementPropertyChangedEventArgs(childelement));
            }
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                {
                    INotifyPropertyChanged convertedItem = item as INotifyPropertyChanged;
                    convertedItem.PropertyChanged += new PropertyChangedEventHandler(convertedItem_PropertyChanged);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                {
                    INotifyPropertyChanged convertedItem = item as INotifyPropertyChanged;
                    convertedItem.PropertyChanged -= new PropertyChangedEventHandler(convertedItem_PropertyChanged);
                }
            }
        }

        void convertedItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnChildElementPropertyChanged(sender);
        }

        protected override void ClearItems()
        {
            foreach (INotifyPropertyChanged item in Items)
            {
                item.PropertyChanged -= convertedItem_PropertyChanged;
            }

            base.ClearItems();
        }
    }
}
