﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Glaxion.Tools;

namespace Glaxion.ViewModel
{
    public class VMListView<T>
    {
        public VMListView()
        {
            Items = new ObservableCollection<T>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ObservableCollection<T> _Items;
        public ObservableCollection<T> Items {
            get { return _Items; }
            set { _Items = value; OnPropertyChanged(); }
        }

        ObservableCollection<T> _SelectedItems;
        public ObservableCollection<T> SelectedItems
        {
            get { return _SelectedItems; }
            set { _SelectedItems = value; OnPropertyChanged(); }
        }
        
        public void MoveItems(int index, List<T> items)
        {
            if (Items.Count == 0 || items.Count == 0)
                return;

            //Sort(items, Items);
            List<T> insertedItems = new List<T>(items.Count);
            List<T> removedItems = new List<T>(items.Count);

            if (index >= Items.Count)
                index = Items.Count - 1;
            if (index < 0)
                index = 0;
            int last_index = 0;

            foreach (T t in items)
            {
                T item = t;
                last_index = Items.IndexOf(item);
                //looks like we're dropping on one of the selected items.
                //cancel drag drop
                if (last_index == index)
                    return;

                insertedItems.Add(item);
                removedItems.Add(item);
            }
            
            foreach (T t in removedItems)
                Items.Remove(t);

            if (index > Items.Count)
                index = Items.Count;

            foreach (T t in insertedItems)
                Items.Insert(index, t);
        }
    }
}