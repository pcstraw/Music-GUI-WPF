using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Glaxion.Tools;
using Glaxion.ViewModel;
using MultiSelection;

namespace MusicWindow
{
    public class VMTreeControl :  MSTreeView
    {
        public VMTreeControl() : base()
        {
            _dragThresh = 0.05;
            DropData = new List<string>();
            AllowDrop = true;
        }

        Point _capturedMousePosition;
        double _dragThresh;
        List<string> DropData;
        bool _isDragging;

        private parentItem FindVisualParent<parentItem>(DependencyObject obj) where parentItem : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null && !parent.GetType().Equals(typeof(parentItem)))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as parentItem;
        }

        private bool IsDraggingScrollbar(MouseEventArgs e)
        {
            object original = e.OriginalSource;

            if (!original.GetType().Equals(typeof(ScrollViewer)))
            {
                if (FindVisualParent<ScrollBar>(original as DependencyObject) != null)
                    return true;
            }
            return false;
        }

        bool CheckDragThreshold(MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _capturedMousePosition = e.GetPosition(this);
                return false;
            }
            else
            {
                Point currentMousePos = e.GetPosition(this);
                bool y_thresh = (currentMousePos.Y - _capturedMousePosition.Y) > _dragThresh;
                bool x_thresh = (currentMousePos.X - _capturedMousePosition.X) > _dragThresh;
                if (y_thresh || x_thresh)
                    return true;
            }

            return false;
        }

        void TestDropData()
        {
            if (DropData.Count == 0)
            {
                tool.debugError("Missing Drop Data");
                return;
            }

            foreach (string s in DropData)
                tool.debugSuccess(2, s);
        }

        List<string> GetDragDropData()
        {
            DropData.Clear();

            tool.debugHighlight("getting drop data");

            if (SelectedItems.Count == 0)
                tool.debugWarning("No selected items");

            foreach (var node in SelectedItems)
            {
                VMNode n = node.DataContext as VMNode;
                DropData.Add(n.FilePath);
            }

            //TestDropData();

            return DropData;
        }
        

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isDragging)
            {
                _isDragging = false;
            }
           // tool.debugWarning("Mouse up");
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!CheckDragThreshold(e))
                return;

            if (!_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsDraggingScrollbar(e))
                    return;

                _isDragging = true;

                GetDragDropData();

                DragDrop.DoDragDrop(this, DropData,
                     DragDropEffects.Copy);

                tool.debugSuccess("Drag Started");
            }

            if (_isDragging)
            {
                //Check if within range of list view.
                Point point = e.GetPosition(this);
                Rect rect = VisualTreeHelper.GetContentBounds(this);
                
                if (!rect.Contains(point))
                {
                    _isDragging = false;
                    e.Handled = true;
                }
            }
        }
    }
}
