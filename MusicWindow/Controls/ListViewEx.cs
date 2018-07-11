using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicWindow
{
    public class ListViewEx : ListView
    {
        public ListViewEx() : base()
        {
            AllowDrop = true;
            draggedItems = new ArrayList();
            MultiDrag = true;
            //DragEnter += ListViewEx_DragEnter;
            DragEnter += ListViewDragEnter;
            Drop += ListViewDrop;
            PreviewMouseLeftButtonDown += ListViewEx_PreviewMouseLeftButtonDown;
            PreviewMouseMove += ListViewEx_PreviewMouseMove;
        }
        
        Point _startPoint;
        public DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope;
        ArrayList draggedItems;

        public bool MultiDrag { get; set; }

        public delegate void SingleDropDataEventHandler(int Index,object Item);
        public event SingleDropDataEventHandler DropDataEvent;
        protected void On_DropDataEvent(int Index, object Item)
        {
        }

        public delegate void MultiDropDataEventHandler<T>(int Index, List<T> Item);
        public event MultiDropDataEventHandler<object> MultiDropDataEvent;
        protected void On_MultiDropDataEvent<T>(int Index, List<T> Item)
        {
        }

        public void DropData<T>(int index, ObservableCollection<T> list,object item)
        {
            if (list.Count == 0)
                return;

            if (index >= 0)
            {
                list.Remove((T)item);
                list.Insert(index, (T)item);
            }
            else
            {
                list.Remove((T)item);
                list.Add((T)item);
            }
        }

        public void MultiDropData<T>(int index, ObservableCollection<T> list, List<object> items)
        {
            if (list.Count == 0)
                return;
            
            List<T> temp = new List<T>(items.Count);
            object insertion = list[index];

            foreach( var t in items)
                temp.Add((T)t);
            
            int updatedIndex = list.IndexOf((T)insertion);
            if(updatedIndex <0)
            {
                tool.debugError("Unable to retreive Insertion index");
                return;
            }
            foreach (var t in items)
                list.Remove((T)t);

            foreach (var t in temp)
            {
                list.Insert(updatedIndex, (T)t);
                if (!SelectedItems.Contains(t))
                    SelectedItems.Add(t);
            }
            _selItems.Clear();
        }

        private List<object> _selItems = new List<object>();
        private void ListViewEx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Store the mouse position
            _startPoint = e.GetPosition(this);
            if(MultiDrag)
            {
                _selItems.Clear();
                _selItems.AddRange(SelectedItems.Cast<object>());
            }
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            Type checktype = typeof(ListViewItem);
            if (MultiDrag)
                checktype = typeof(IList);
            if (!e.Data.GetDataPresent(checktype) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
        
        private void ListViewEx_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (object selItem in _selItems)
                {
                    if (!SelectedItems.Contains(selItem))
                        SelectedItems.Add(selItem);
                }
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!MultiDrag)
                        BeginSingleDrag(e);
                    else
                        BeingMultiDrag(e);
                }
            }
        }

        private void BeingMultiDrag(MouseEventArgs e)
        {
            ListViewEx listView = this;
            ListViewItem listViewItem =
                 FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null)
                return;
           // object contextData = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;

            DataObject data = new DataObject(typeof(List<object>), _selItems);
            //draggedItems.Clear();
            //draggedItems.Add(contextData);
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

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

        private void BeginSingleDrag(MouseEventArgs e)
        {
            ListViewEx listView = this;
            ListViewItem listViewItem =
                 FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            object contextData = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;

            DataObject data = new DataObject(typeof(ListViewItem),contextData);
            //draggedItems.Clear();
            //draggedItems.Add(contextData);
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
            // get the data for the ListViewItem
            //  Song song = (Song)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
            //  tool.show(3, listView.Items.IndexOf(listViewItem));
        }
        
        private void ListViewDrop(object sender, DragEventArgs e)
        {
            //handle external drag drop
            if (sender != this)
                return;
            if (!MultiDrag)
                InternalSingleDragDrop(e);
            else
                InternalMultiDragDrop(e);
        }

        private void InternalMultiDragDrop(DragEventArgs e)
        {
            //initialize to the last index and check if we have 
            //more then one item
            int index = Items.Count - 1;
            if (index <= 0)
                return;

            if (e.Data.GetDataPresent(typeof(List<object>)))
            {

                List<object> itemToMove = (List<object>)e.Data.GetData(typeof(List<object>));
                ListViewItem itemToReplace = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                // object name = draggedItems[0]; //hack.  Use iteration instead
                object nameToReplace;
                if (itemToReplace != null)
                {
                    nameToReplace = this.ItemContainerGenerator.ItemFromContainer(itemToReplace);
                    index = this.Items.IndexOf(nameToReplace);
                }
                MultiDropDataEvent?.Invoke(index, itemToMove);
            }
        }

        private void InternalSingleDragDrop(DragEventArgs e)
        {
            //initialize to the last index and check if we have 
            //more then one item
            int index = Items.Count - 1;
            if (index <= 0)
                return;

            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                object itemToMove = e.Data.GetData(typeof(ListViewItem));
                ListViewItem itemToReplace = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                // object name = draggedItems[0]; //hack.  Use iteration instead
                object nameToReplace;
                if (itemToReplace != null)
                {
                    nameToReplace = this.ItemContainerGenerator.ItemFromContainer(itemToReplace);
                    index = this.Items.IndexOf(nameToReplace);
                }
                DropDataEvent?.Invoke(index, itemToMove);
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
                Point point = e.GetPosition(this);
                Rect rect = VisualTreeHelper.GetContentBounds(this);

                //Check if within range of list view.
                if (!rect.Contains(point))
                {
                    e.Effects = DragDropEffects.Copy;
                    //this._dragIsOutOfScope = true;
                    //e.Handled = true;
                }
            }
        }

        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(this).X;
                _adorner.OffsetTop = args.GetPosition(this).Y - _startPoint.Y;
            }
        }

        /*
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MyListViewItem();
        }
        */

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
