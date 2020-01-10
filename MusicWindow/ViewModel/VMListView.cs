using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        
        public void MoveItems(int index, IList<T> items)
        {
            if (Items.Count == 0 || items.Count == 0)
                return;

            if (index < 0)
            {
                return;
                /*
                if (Items.Count > 1)
                    index = Items.Count - 1;  //probably causing issues with items falling down the bottom
                else
                    index = 0;
                    */
            }
            
            if(index == 0)
            {
              //  Tools.tool.show(4, "0 index test");
            }
            

            int max_index = Items.Count;
            int last_index = Items.IndexOf(items[items.Count - 1]);
            if (index >= max_index)
                throw new Exception("getting index is too high");

            //insert below
            if (index > last_index)
            {
                index = index - (items.Count-1);
               // tool.show(5, "Drop below");
            }
            //throw new Exception("Handle drop below case");

            foreach (T t in items)
            {
                if (index == Items.IndexOf(t))
                    return;
            }

            foreach (T t in items)
                Items.Remove(t);

            if (index > Items.Count)
                index = Items.Count;
            
            foreach (T t in items)
            {
                Items.Insert(index, t);
            }
            return;
            /*
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
            
            
            if (last_index <= index)
            {
                //index--;
            }

            if (index > Items.Count)
                index = Items.Count;


            foreach (T t in removedItems)
                Items.Remove(t);
            

            foreach (T t in insertedItems)
            {
                Items.Insert(index, t);
            }
            */
        }
    }
}
