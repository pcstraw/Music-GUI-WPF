using Glaxion.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicWindow
{
    public class ListViewEx<T> : ListView
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
            DataContext = viewModel;
            highlightItem = new HighLightedItem();
        }
        
        Point _startPoint;
        public DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope;
        ArrayList draggedItems;
        internal VMListView<T> viewModel;
        public List<T> _selItems = new List<T>();
        public int CurrentIndex;
        internal HighLightedItem highlightItem;
        
        internal class HighLightedItem
        {
            public HighLightedItem()
            {
                highlightColor = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            }

            Brush originalColor;
            Brush highlightColor;
            ListViewItem _item;

            internal void HighlightBackground(ListViewItem item)
            {
                if (item == null)
                    return;
                originalColor = item.Background;
                item.Background = highlightColor;
                _item = item; 
            }

            internal void RestoreBackground()
            {
                if (originalColor == null||_item == null)
                    return;
                _item.Background = originalColor;
                _item = null;
            }

            internal void RestoreBackground(Brush brush)
            {
                if (originalColor == null || _item == null)
                    return;
                _item.Background = brush;
                _item = null;
            }
        }

        internal void UpdateItemSource()
        {
          //  ItemsSource = null;
            ItemsSource = viewModel.Items;
        }

        public void RemoveSelectedItems()
        {

            for(int i=0;i < SelectedItems.Count;i++)
            {
                viewModel.Items.Remove((T)SelectedItems[i]);
                i--;
            }
            
            SelectedItems.Clear();
            _selItems.Clear();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Delete)
                RemoveSelectedItems();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            //   CacheSelectedItems();
            GetCurrentIndex(Mouse.GetPosition);
            ListViewItem item = GetListViewItem(CurrentIndex);
            item.IsSelected = false;
            SelectedItems.Remove(item.DataContext);
            highlightItem.RestoreBackground();
            highlightItem.HighlightBackground(item);
           // CurrentIndex++;
            base.OnContextMenuOpening(e);
            RestoreCacheSelectedItems();
        }
        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
           
            base.OnContextMenuClosing(e);
            highlightItem.RestoreBackground(Background);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            CacheSelectedItems();
            base.OnPreviewMouseRightButtonDown(e);
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

        public ListViewItem GetListViewItem(int index)
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
                _selItems.Add((T)o);
        }

        internal void RestoreCacheSelectedItems()
        {
            SelectedItems.Clear();
            foreach (object o in _selItems)
                SelectedItems.Add(o);
        }

        private void BeingMultiDrag(MouseEventArgs e)
        {
            ListViewEx<T> listView = this;
            ListViewItem listViewItem =
                 FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            //the listviewitem can be selected on drag before the cache has updated.
            //In this case, add the item to _selitems
            //when dragging between listviews, we use _selitems to get the selected items
            if (!_selItems.Contains((T)listViewItem.DataContext))
            {
                _selItems.Clear(); //make this item the only selected item
                _selItems.Add((T)listViewItem.DataContext);
            }

            RestoreCacheSelectedItems();
            SortCachedSelectedItems();

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;
            
            DataObject data = new DataObject(typeof(ListViewEx<T>), this);
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
            bool isListViewEx = e.Data.GetDataPresent(typeof(ListViewEx<T>));
            if (isListViewEx)
            {
                List<T> itemsToMove = new List<T>();
                ListViewEx<T> source = e.Data.GetData(typeof(ListViewEx<T>)) as ListViewEx<T>;
                int index = GetCurrentIndex(e.GetPosition);

                if (source == this)
                {
                    foreach (T o in _selItems)
                        itemsToMove.Add(o);

                    MoveData(index, itemsToMove);
                    _selItems.Clear();
                }
                else if(source is ListViewEx<T>)
                {
                    foreach (T o in source._selItems)
                        itemsToMove.Add(o);
                    AddData(index, itemsToMove);

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
                    AddDataFromFiles(index, StringSource);
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

        void PrevGreaterThan(T item)
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
        #region Must Override
        protected virtual void AddDataFromFiles(int insertionIndex, List<string> files)
        {
            throw new NotImplementedException("Inherit this class and override this method to manipulate the view model's Items");
        }

        protected virtual void MoveData(int insertIndex, List<T> items)
        {
            viewModel.MoveItems(insertIndex, items);
        }

        protected virtual void AddData(int insertIndex, List<T> items)
        {
            throw new NotImplementedException("Inherit this class and override this method to manipulate the view model's Items");
        }
        
        #endregion
    }
}
