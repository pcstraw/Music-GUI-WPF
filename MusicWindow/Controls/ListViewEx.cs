using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicWindow
{
    public interface IViewModel
    {
        void AddDataFromFiles(int insertIndex,List<string> files);
        void MoveData(int insertIndex, List<object> items);
        void AddData(int insertIndex, List<object> items);
    }

    public class ListViewEx : ListView
    {
        public ListViewEx() : base()
        {
            AllowDrop = true;
            draggedItems = new ArrayList();
            //DragEnter += ListViewEx_DragEnter;
            DragEnter += ListViewDragEnter;
            Drop += ListViewDrop;
            PreviewMouseLeftButtonDown += ListViewEx_PreviewMouseLeftButtonDown;
            PreviewMouseMove += ListViewEx_PreviewMouseMove;
            QueryContinueDrag += ListViewQueryContinueDrag;
        }
        
        Point _startPoint;
        public DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope;
        ArrayList draggedItems;
        IViewModel _viewModelInterface;
        public List<object> _selItems = new List<object>();
        public int CurrentIndex;
        
        //move to VMListView  also see MultiSelect TreeView sort implementation
        void Sort<T>(List<object> items,ObservableCollection<T> fullList)
        {
            int first_index = fullList.IndexOf((T)items[0]);
            
            for(int i=0;i<items.Count;i++)
            {
                T item = (T)items[i];
                int index = fullList.IndexOf(item);
                if (index < first_index)
                {
                    items.RemoveAt(i);
                    items.Insert(0, item);
                    first_index = index;
                }
            }
            items.Reverse();
        }

        internal void SetViewModelInterface(IViewModel viewModelInterface)
        {
            _viewModelInterface = viewModelInterface;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            CacheSelectedItems();
            base.OnPreviewMouseLeftButtonDown(e);
        }

        //called before the event handler
        protected override void OnDrop(DragEventArgs e)
        {
            //List<object> items = dataObj.GetData(typeof(List<object>)) as List<object>;
            base.OnDrop(e); //calls event handler
        }

        delegate Point GetPositionDelegate(IInputElement element);
        private int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                ListViewItem item = GetListViewItem(i);
                if (item == null)
                    continue;
                if (IsMouseOverTarget(item, getPosition))
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                index = Items.Count;
            CurrentIndex = index;
            return index;
        }
        private ListViewItem GetListViewItem(int index)
        {
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;
            return ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }
        private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        //currently using this
        /*
        int GetDragInsertionIndex(DragEventArgs e)
        {
            ListViewItem itemToReplace = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
           // object nameToReplace;
            int index = Items.Count;
            if (itemToReplace != null)
            {
                // nameToReplace = this.ItemContainerGenerator.ItemFromContainer(itemToReplace);
                 index =this.ItemContainerGenerator.IndexFromContainer(itemToReplace);
             //   FrameworkElement element = (FrameworkElement)e.OriginalSource;
             //   ListViewItem lvi = (ListViewItem)this.ItemContainerGenerator.ContainerFromItem(element.DataContext);
             //   index = Items.IndexOf(lvi);
            }
            if (index > Items.Count)
                index = Items.Count;

            return index;
        }
        */

        private void ListViewEx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Store the mouse position
            _startPoint = e.GetPosition(this);
            
            /*
            if(_selItems.Count == 0)
            {
                HitTestResult result = VisualTreeHelper.HitTest(this, _startPoint);
                DependencyObject obj = result.VisualHit;
                if (obj == null)
                    return;

                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>(obj);
                if (listViewItem == null)
                    return;
                //_selItems.AddRange(SelectedItems.Cast<object>());
                object data = ItemContainerGenerator.ItemFromContainer(listViewItem);
               // _selItems.Clear();
                
              //  _selItems.Add(data);
            }
            */
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            Type checktype = typeof(ListViewItem);
            checktype = typeof(IList);
            if (!e.Data.GetDataPresent(checktype) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
        
        
        private void ListViewEx_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            GetCurrentIndex(e.GetPosition);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance *2.0||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance*2.0)
                {
                    BeingMultiDrag(e);
                }
            }
        }

        internal void CacheSelectedItems()
        {
            _selItems.Clear();
            foreach(object o in SelectedItems)
                _selItems.Add(o);
        }

        internal void RestoreCacheSelectedItems()
        {
            SelectedItems.Clear();
            foreach (object o in _selItems)
                SelectedItems.Add(o);
        }

        private void BeingMultiDrag(MouseEventArgs e)
        {
            ListViewEx listView = this;
            ListViewItem listViewItem =
                 FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            //the listviewitem can be selected on drag before the cache has updated.
            //In this case, add the item to _selitems
            //when dragging between listviews, we use _selitems to get the selected items
            if (!_selItems.Contains(listViewItem.DataContext))
            {
                _selItems.Clear(); //make this item the only selected item
                _selItems.Add(listViewItem.DataContext);
            }

            RestoreCacheSelectedItems();
            SortCachedSelectedItems();

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;
            
           

            DataObject data = new DataObject(typeof(ListViewEx), this);
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            _selItems.Clear();

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
        }

        
        private void ListViewDrop(object sender, DragEventArgs e)
        {
           HandleDragDrop(e);
        }

        private void HandleDragDrop(DragEventArgs e)
        {
            bool isListViewEx = e.Data.GetDataPresent(typeof(ListViewEx));
            if (isListViewEx)
            {
                List<object> itemsToMove = new List<object>();
                ListViewEx source = e.Data.GetData(typeof(ListViewEx)) as ListViewEx;
                int index = GetCurrentIndex(e.GetPosition);

                if (source == this)
                {
                    foreach (object o in _selItems)
                        itemsToMove.Add(o);

                    _viewModelInterface?.MoveData(index, itemsToMove);
                }
                else if(source is ListViewEx)
                {
                    foreach (object o in source._selItems)
                        itemsToMove.Add(o);
                    _viewModelInterface?.AddData(index, itemsToMove);

                    this.UpdateLayout();
                }
            }
            else
            {
                DataObject dataObj = e.Data as DataObject;
                List<string> StringSource = dataObj.GetData(typeof(List<string>)) as List<string>;
                if (StringSource != null)
                {
                    int index = GetCurrentIndex(e.GetPosition);
                    _viewModelInterface?.AddDataFromFiles(index, StringSource);
                }
            }
        }
        
        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(this as Visual);
            _layer.Add(_adorner);
        }

        private void ListViewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            
            if (this._dragIsOutOfScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }

        private void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == this)
            {
               
            }
        }

        //need to compensate for scrolling
        void ListViewDragOver(object sender, DragEventArgs e)
        {
            GetCurrentIndex(e.GetPosition);
            if (_adorner != null)
            {
                _adorner.OffsetLeft = e.GetPosition(this).X;
                _adorner.OffsetTop = e.GetPosition(this).Y - _startPoint.Y;
            }
            /*
            Point point = e.GetPosition(this);
            Rect rect = VisualTreeHelper.GetContentBounds(this);

            //Check if within range of list view.
            if (!rect.Contains(point))
            {
                e.Effects = DragDropEffects.Copy;
               // this._dragIsOutOfScope = true;
                //e.Handled = true;
            }
            */
        }

        internal void SortCachedSelectedItems()
        {
            for (int i = 0; i < _selItems.Count; i++)
            {
                PrevGreaterThan( _selItems[i]);
            }
            _selItems.Reverse();
        }

        void PrevGreaterThan(object item)
        {
            int index = Items.IndexOf(item);
            if (index < 0)
                return;

            int prev_item = _selItems.IndexOf(item) - 1;
            if (prev_item < 0)
                return;

            int prev = Items.IndexOf(_selItems[prev_item]);
            if (prev < 0)
                return;

            if (prev > index)
            {
                _selItems.Remove(item);
                _selItems.Insert(prev_item, item);
                PrevGreaterThan(item);
            }
        }

        public ListViewItem GetItemFromData(object item)
        {
            return ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
        }

        private static T FindAnchestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
