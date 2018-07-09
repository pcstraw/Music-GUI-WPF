using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Glaxion.Tools;

namespace Glaxion.Music
{
    public enum ItemState { Normal, IsPlaying, WasPlaying, WasSelected, Missing, Reset, IsPlayingInOtherPanel,
        IsThePlayingTrack
    }

    public delegate void TracksChangedDelegate();
    
    //virtual listbox class
    //used to represent a windows form ListView
    public class VItem
    {
        public VItem()
        {
            CurrentColor = new ColorScheme(Color.Black,Color.White);
            OldColor = CurrentColor;
            Columns.Add("name");
            Columns.Add("path");
        }

        
        public ColorScheme CurrentColor;
        public ColorScheme OldColor;
        public List<string> Columns = new List<string>();
        public object Tag;
        public ItemState State;
        public bool Selected;
        public bool Checked;
        public string Name;

        public void HighLightColors(ColorScheme scheme)
        {
            OldColor = CurrentColor;
            CurrentColor = scheme;
        }
        public void RestoreColors()
        {
            CurrentColor = OldColor;
        }
        public void SetColors(ColorScheme scheme)
        {
            CurrentColor = scheme;
            OldColor = scheme;
        }

        internal VItem Clone()
        {
            VItem item = new VItem();
            item.Name = Name;
            item.Columns[0] = Columns[0];
            item.Columns[1] = Columns[1];
            item.Tag = Tag;
            item.CurrentColor = CurrentColor;
            item.OldColor = OldColor;
            item.Selected = Selected;
            item.State = State;
            item.Checked = Checked;
            return item;
        }
    }

    public interface IListView
    {
        void Add(VItem item);
        void Insert(int index, VItem item);
        void RefreshColors();
        void Remove(int index);
        //void PasteFrom(int index,string[] files);
       // void Remove(VItem item);
    }

    public class VListView
    {
        public VListView(IListView ListViewInterface,ColorScheme colors)
        {
            SetListViewInterface(ListViewInterface);
            CurrentColors = colors;
        }
        void SetListViewInterface(IListView ListViewInterface)
        {
            _view = ListViewInterface;
            _items = new List<VItem>();
        }
        protected IListView _view;
        List<VItem> _items;
        //public List<int> SelectedIndices = new List<int>();
        public List<VItem> CheckedItems = new List<VItem>();
        public ColorScheme CurrentColors;
        public TracksChangedDelegate tracksChangedDelegate;
        public virtual VItem CreateItem(string file)
        {
            VItem i = new VItem();
            i.Columns[0] = Path.GetFileNameWithoutExtension(file);
            i.Columns[1] = file;
            i.SetColors(CurrentColors);
            i.Name = file;
            //move this to derived createItem.  See playlistManager CreateItem
            Song song = SongInfo.Instance.GetInfo(file);
            if (song != null)
                i.Tag = song;
            return i;
        }

        public virtual VItem Add(VItem item)
        {
            _items.Add(item);
            _view.Add(item);
            return item;
        }

        public virtual VItem Insert(int index, VItem item)
        {
            _items.Insert(index, item);
            _view.Insert(index, item);
            return item;
        }

        public virtual VItem Remove(int index)
        {

            VItem item = _items[index];
            _items.Remove(item);
            _view.Remove(index);
            return item;
        }
        public virtual VItem Remove(VItem item)
        {
            if (!_items.Contains(item))
            {
                tool.show(5, "Crash averted:  getting the index of an item not in Items");
                return null;
            }
            int index = IndexOf(item);
            _view.Remove(index);
            _items.Remove(item);
            return item;
        }
        
        public void MoveIndicesTo(int insertionIndex,List<int> indices)
        {
            List<VItem> removedItems = new List<VItem>();
            indices.Reverse();
            ClearSelection();
            foreach (int i in indices)
            {
                removedItems.Add(_items[i]);
            }

            int _i = insertionIndex;
            if (insertionIndex >= ItemCount)
                _i = ItemCount-1;
            if (insertionIndex < 0)
                _i = 0;
            

            VItem cache_index = _items[_i];
            foreach (VItem i in removedItems)
            {
                Remove(i);
            }
            
            _i = _items.IndexOf(cache_index);
            if (_i != 0)
                _i++;
            foreach (VItem i in removedItems)
            {
                VItem newi = i.Clone();
                newi.Selected = true;
                Insert(_i, newi);
            }
            CallTracksChangedDelegate();
        }

        public void CallTracksChangedDelegate()
        {
            if (tracksChangedDelegate != null)
                tracksChangedDelegate.Invoke();
        }

        public List<VItem> Items
        {
            get
            {
                return _items;
            }
        }

        public void ClearSelection()
        {
            foreach (VItem _sele in Items)
            {
                _sele.Selected = false;
            }
        }
        public void RemoveSelectedTracks(List<int> tracks)
        {
            //StoreCurrentState();
            if (tracks == null)
                return;
            /*
            if(tracks.Count != Items.Count)
            {
                tool.show(1,"Something went wrong: index count and item count do not match");
                throw new Exception("Handle index count != item count");
                return;
            }
            */
            int track_count = tracks.Count;
            List<VItem> list = new List<VItem>(ItemCount);
            foreach (int index in tracks)
            {
                if(index > Items.Count)
                {
                    tool.show(1, "Something went wrong: index count and item count do not match");
                    continue;
                    //throw new Exception("Handle index count != item count");
                    
                }
                list.Add(Items[index]);
                //VItem item = _items[index];
                //_items.Remove(item);
            }
            /*
            foreach (int index in tracks)
            {
                Remove(index);
            }
            */
            foreach (VItem i in list)
                Remove(i);
            CallTracksChangedDelegate();
        }
        //add external files
        public virtual void AddFiles(int dropIndex, string[] arr)
        {
            List<VItem> insertItems = new List<VItem>();
            foreach (string s in arr)
            {
                //fix:  if the file is a directory
                //then load as a playlist in playlistManager. 
                //virtual function
                if (Directory.Exists(s))
                {
                    List<string> s_array = tool.LoadAudioFiles(s, SearchOption.TopDirectoryOnly);
                    foreach (string s2 in s_array)
                    {
                        VItem i1 = CreateItem(s2);
                        if (i1 == null)
                            continue;
                        insertItems.Add(i1);
                    }
                    continue;
                }
                else
                {
                    VItem i2 = CreateItem(s);
                    if (i2 == null)
                        continue;
                    insertItems.Add(i2);
                }
            }
            insertItems.Reverse();
            foreach (VItem item in insertItems)
            {
                item.Selected = true;
                Insert(dropIndex, item);
            }
        }
        //resturn list of indices instead
        public void CheckForRepeat(List<int> SelectedIndices)
        {
            foreach (int item in SelectedIndices)
            {
                string file = Items[item].Columns[1];
                if (!tool.StringCheck(file))
                    continue;

                foreach (VItem other in Items)
                {
                    string s = other.Name;
                    if (!tool.StringCheck(s))
                        continue;
                    if (s == file)
                        other.Selected = true;
                }
            }
        }

        //rename method
        public void MoveSelectedToBottom(IEnumerable<int> SelectedIndices)
        {
            foreach (int i in SelectedIndices)
            {
                Remove(i);
                Insert(ItemCount, Items[i]);
            }
            CallTracksChangedDelegate();
        }
        //rename method
        public void MoveSelectedToTop(IEnumerable<int> SelectedIndices)
        {
            foreach (int i in SelectedIndices)
            {
                Remove(i);
                Insert(0, Items[i]);
            }
            CallTracksChangedDelegate();
        }
        
        public void MoveSelectedItemsTo(int index,List<int> selectedIndices)
        {
            List<VItem> insertItems = new List<VItem>(selectedIndices.Count);
            List<VItem> removedItems = new List<VItem>(selectedIndices.Count);
            //if (index > ItemCount)
             //   index = ItemCount;
            VItem ref_item = Items[index];
            
            //tool.show(5, ref_item.Name);
            if (ref_item == null)
            {
                tool.show(5, "Ref item is null");
                return;
            }
            ClearSelection();
            int last_index = 0;
            foreach (int item in selectedIndices)
            {
                if (item == index)
                    return;
                VItem i = Items[item];
                i.Selected = true;
                insertItems.Add(i);
                removedItems.Add(Items[item]);
                last_index = item;
                
            }
            
            foreach (VItem removeItem in removedItems)
            {
                Remove(removeItem);
            }
            index = Items.IndexOf(ref_item);

            if (index == -1)
            {
                tool.show(3, "Index is -1");
                return;
            }
            if (index > last_index)
            {
                index++;
                //index+=2;
                //tool.show(1, "lower drop");
            }
            for (int i = insertItems.Count - 1; i >= 0; i--)
            {
                Insert(index, insertItems[i]);
            }
            CallTracksChangedDelegate();
        }
        //override method?
        public void FindMissingFiles()
        {
            foreach (VItem i in Items)
            {
                if (i.Tag.GetType() != typeof(string))
                    throw new Exception("No file path set");

                if (!File.Exists(i.Name))
                    i.SetColors(new ColorScheme(Color.Black, MusicPlayer.MissingColor));
            }
        }
        /*
        public IEnumerable<VItem> SelectedItems
        {
            get
            {
                return GetSelectedItems();
            }
        }
        */
        public int ItemCount
        {
            get
            {
                return _items.Count;
            }
        }

        public IEnumerable<VItem> GetItems()
        {
            foreach(VItem item in _items)
            {
                yield return item;
            }
        }
        /*
        public IEnumerable<VItem> GetSelectedItems()
        {
            foreach(int i in SelectedIndices)
            {
                yield return _items[i];
            }
        }
        */

        public VItem GetItem(int index)
        {
            return _items[index];
        }

        public int IndexOf(VItem item)
        {
            for(int i = 0;i<_items.Count;i++)
            {
                if (_items[i] == item)
                    return i;
            }
            return -1;
        }
    }
}
