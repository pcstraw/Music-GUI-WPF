using Glaxion.Tools;
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
            DragEnter += ListViewDragEnter;
            Drop += ListViewDrop;
            PreviewMouseLeftButtonDown += ListViewEx_PreviewMouseLeftButtonDown;
            PreviewMouseMove += ListViewEx_PreviewMouseMove;
            QueryContinueDrag += ListViewQueryContinueDrag;
            DataContext = viewModel;
            Loaded += ListViewEx_Loaded;
            dragThreshold = 150.0;
            // SelectionMode = SelectionMode.Extended;
        }

        ScrollViewer viewer;
        double dragThreshold;

        VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        T FindSimpleVisualChild<T>(DependencyObject element) where T : class
        {
            if (VisualTreeHelper.GetChildrenCount(element) == 0)
                return null;
            while (element != null)
            {
                if (element is T)
                    return element as T;
                element = VisualTreeHelper.GetChild(element, 0);
            }
            return null;
        }

        private void ListViewEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            viewer = FindSimpleVisualChild<ScrollViewer>(this);
            if (viewer != null)
            {
                viewer.ScrollChanged += Viewer_ScrollChanged;
                viewer.DragOver += Viewer_DragOver;
                viewer.PreviewMouseLeftButtonDown += Viewer_PreviewMouseLeftButtonDown;
                viewer.PreviewMouseMove += Viewer_PreviewMouseMove;
                // Visual States are always on the first child of the control template 
                
            }
        }

        private void Viewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
          //  _startPoint = e.GetPosition(this);

        }

        private void Viewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           // _startPoint = e.GetPosition(this);
            tool.debugSuccess(_startPoint.Y);
        }

        private void Viewer_DragOver(object sender, DragEventArgs e)
        {
            if (_adorner != null)
            {

               // Point p = e.GetPosition(viewer);
                // _startPoint.Y -= e.VerticalOffset;
               // _adorner.OffsetLeft += p.X;
               // _adorner.OffsetTop += p.Y - _viewerstart.Y;
            }
        }
        bool isScrolling;
        private void Viewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(_adorner != null)
            {

                double t_change = e.VerticalOffset;
                tool.debugSuccess(t_change);

                //      _layer.RenderTransformOrigin = new Point(_layer.RenderTransformOrigin.X, _layer.RenderTransformOrigin.Y + e.VerticalOffset);
                // _layer = AdornerLayer.GetAdornerLayer(viewer as Visual);
                //  _layer.Add(_adorner);

                _adorner.SetOffsets(_adorner.OffsetLeft,50);
                //  Point p = Mouse.GetPosition((IInputElement)viewer);
              //    _startPoint.Y = e.VerticalOffset;
                //  _adorner.OffsetLeft = p.X;
                //  _adorner.OffsetTop = p.Y;
                // _startPoint = Mouse.GetPosition(this);
                // _startPoint = e.GetPosition(this);
                //  if(e.VerticalChange)
                // isDragScrolling = true;

                // _startPoint.Y += p.Y + e.VerticalChange;
                // _adorner.OffsetTop = e.VerticalChange;
            }
        }

        private void Group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        bool alreadyHookedScrollEvents = false;

        Point _startPoint;
        public DragAdorner _adorner;
        public SimpleCircleAdorner _testAdorner;
        private AdornerLayer _layer;
        public DragAdorner _hadorner;
        private AdornerLayer _hlayer;
        private bool _dragIsOutOfScope;
        ArrayList draggedItems;
        internal VMListView<T> viewModel;
        public List<T> _selItems = new List<T>();
        public int CurrentIndex;

        public bool isMouseWithinBounds { get; private set; }
        
        internal void UpdateItemSource()
        {
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

        internal int PreContextIndex = -1;
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            PreContextIndex = GetCurrentIndex(Mouse.GetPosition);
            ListViewItem item = GetListViewItem(CurrentIndex);
            if (item == null)
                return;
            
            UnHighlightItem();
            HighlightItem(item);
            base.OnContextMenuOpening(e);
            RestoreCacheSelectedItems();
        }

        ListViewItem _hitem;
        List<VMItem> highlightedItems = new List<VMItem>();
        private void HighlightItem(ListViewItem listViewItem)
        {
            VMItem vitem = listViewItem.Content as VMItem;
            if(vitem != null)
            {
                vitem.ColourIndex = -2;
                _hitem = listViewItem;
                highlightedItems.Add(vitem);
            }
            return;
        }

        private void UnHighlightItem()
        {
            if (_hitem == null)
                return;
            VMItem vitem = _hitem.Content as VMItem;
            if (vitem != null)
            {
                vitem.ColourIndex = 0;
                _hitem = null;
            }

            foreach(VMItem item in highlightedItems)
            {
                item.ColourIndex = 0;
            }
            highlightedItems.Clear();
            return;
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            UnHighlightItem();
            base.OnContextMenuClosing(e);
            PreContextIndex = -1;
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

            CurrentIndex = index;
            isMouseWithinBounds = IsMouseOverTarget(this, getPosition);
            return index;
        }
        

        public ListViewItem GetListViewItem(int index)
        {
            if (index < 0)
                return null;
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

        Point _viewerstart;
        private void ListViewEx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Store the mouse position
            _viewerstart = e.GetPosition(viewer);
            _startPoint = e.GetPosition(this);
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

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance *dragThreshold||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance*dragThreshold)
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
                int index = CurrentIndex;//GetCurrentIndex(e.GetPosition);
                /*
                if(index >= Items.Count)
                {
                    throw new Exception("Index should be valid with the range");
                }
                */

                if (source == this)
                {
                    foreach (T o in _selItems)
                        itemsToMove.Add(o);

                    MoveData(index, itemsToMove);
                    _selItems.Clear();
                    return;
                }
                index = GetCurrentIndex(e.GetPosition);
                if (index < 0)
                    index = Items.Count;

                SelectedItems.Clear();
                _selItems.Clear();

                foreach (T o in source._selItems)
                    itemsToMove.Add(o);

                AddData(index, itemsToMove);
            }
            else
            {
                DataObject dataObj = e.Data as DataObject;
                List<string> StringSource = dataObj.GetData(typeof(List<string>)) as List<string>;
                if (StringSource != null)
                {
                    int index = GetCurrentIndex(e.GetPosition);
                    if (index < 0)
                        index = Items.Count;
                    SelectedItems.Clear();
                    _selItems.Clear();
                    AddDataFromFiles(index, StringSource);
                }
            }
        }

        public class SimpleCircleAdorner : Adorner
        {

            public SimpleCircleAdorner(UIElement adornedElement)

                : base(adornedElement)

            { }

            double X;
            double Y;

            public Point centre;
            public ListViewItem item;

            Point AddPoint(Point p1)
            {
                return new Point(X + p1.X, Y + p1.Y);
            }

            public void MovePosition(Point p)
            {
                this.X = p.X;
                this.Y = p.Y;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

                SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);

                renderBrush.Opacity = 0.2;

                Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);

                double renderRadius = 5.0;
                
                drawingContext.DrawEllipse(renderBrush, renderPen, AddPoint(adornedElementRect.TopLeft), renderRadius, renderRadius);

                drawingContext.DrawEllipse(renderBrush, renderPen, AddPoint(adornedElementRect.TopRight), renderRadius, renderRadius);

                drawingContext.DrawEllipse(renderBrush, renderPen, AddPoint(adornedElementRect.BottomLeft), renderRadius, renderRadius);

                drawingContext.DrawEllipse(renderBrush, renderPen, AddPoint(adornedElementRect.BottomRight), renderRadius, renderRadius);

            }

        }

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _testAdorner = new SimpleCircleAdorner(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _testAdorner.Opacity = 0.5;
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(this as Visual);
          //  _layer.Add(_testAdorner);
            _layer.Add(_adorner);
            _testAdorner.item = listViewItem;
            //_layer.Add(_adorner);
        }

        private void ListViewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            /*
            if (this._dragIsOutOfScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
            */
        }

        private void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == this)
            {
               
            }
        }

        //need to compensate for scrolling
        internal bool isDragScrolling;
        void ListViewDragOver(object sender, DragEventArgs e)
        {
            GetCurrentIndex(e.GetPosition);
            if (_adorner != null)
            {
                _adorner.OffsetLeft = e.GetPosition(this).X;
                _adorner.OffsetTop = e.GetPosition(this).Y - _startPoint.Y;
                
               // tool.debugWarning(_adorner.OffsetTop);
            }
            if (_testAdorner!= null)
            {
               // _testAdorner.centre = Mouse.GetPosition(MainControl.Current);
                //_testAdorner.MovePosition(e.GetPosition(this));
               // _testAdorner.= e.GetPosition(this).X;
               // _testAdorner.OffsetTop = e.GetPosition(this).Y ;

                //tool.debugWarning(_testAdorner.centre.Y);
            }

        }
        //dep
        internal void UpdateDragAdorner()
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = Mouse.GetPosition(null).X;
                _adorner.OffsetTop = Mouse.GetPosition(null).Y - _startPoint.Y;
            }
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
        internal virtual void AddDataFromFiles(int insertionIndex, List<string> files)
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
