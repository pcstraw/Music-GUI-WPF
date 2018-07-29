using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiSelection
{
    public enum SelectionModalities
    {
        SingleSelectionOnly,
        MultipleSelectionOnly,
        KeyboardModifiersMode
    }

    public class SelectedItemsCollection : ObservableCollection<MSTreeViewItem> { }

    public class MSTreeView : ItemsControl
    {
        #region Properties
        private MSTreeViewItem _lastClickedItem = null;
        public bool BlockSelection;
        public SelectionModalities SelectionMode
        {
            get { return (SelectionModalities)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(SelectionModalities), typeof(MSTreeView), new UIPropertyMetadata(SelectionModalities.SingleSelectionOnly));

        private SelectedItemsCollection _selectedItems = new SelectedItemsCollection();
        public SelectedItemsCollection SelectedItems
        {
            get { return _selectedItems; }
        }
        #endregion

        #region Constructors
        static MSTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                    typeof(MSTreeView), new FrameworkPropertyMetadata(typeof(MSTreeView)));
   
        }
        #endregion

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MSTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MSTreeViewItem;
        }

        internal void OnSelectionChanges(MSTreeViewItem viewItem)
        {
            MSTreeViewItem newItem = viewItem;
            if (newItem == null)
                return;

            bool isNewItemMultipleSelected = viewItem.IsSelected;

            if (isNewItemMultipleSelected)
                AddItemToSelection(viewItem);
            else
                RemoveItemFromSelection(viewItem);
        }

        internal void OnViewItemMouseDown(MSTreeViewItem viewItem)
        {

            MSTreeViewItem newItem = viewItem;
            if (newItem == null)
                return;
            
            switch (this.SelectionMode)
            {
                case SelectionModalities.MultipleSelectionOnly:
                    ManageCtrlSelection(newItem);
                    break;
                case SelectionModalities.SingleSelectionOnly:
                    ManageSingleSelection(newItem);
                    break;
                case SelectionModalities.KeyboardModifiersMode:
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        // ... TODO ... right now we use the same behavior of Shit Keyword
                        //ManageCtrlSelection(newItem);
                        ManageShiftSelection(viewItem);
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        ManageCtrlSelection(newItem);
                    }
                    else
                    {
                        ManageSingleSelection(newItem);
                    }
                    break;
            }
            _lastClickedItem = viewItem.IsSelected ? viewItem : null;
        }

        #region Methods
        public void UnselectAll()
        {
            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    if (item is MSTreeViewItem)
                    {
                        ((MSTreeViewItem)item).UnselectAllChildren();
                    }
                    else
                    {
                        MSTreeViewItem tvItem = this.ItemContainerGenerator.ContainerFromItem(item) as MSTreeViewItem;

                        if (tvItem != null)
                            tvItem.UnselectAllChildren();
                    }
                }
            }
        }

        public void SelectAllExpandedItems()
        {
            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    if (item is MSTreeViewItem)
                    {
                        ((MSTreeViewItem)item).SelectAllExpandedChildren();
                    }
                    else
                    {
                        MSTreeViewItem tvItem = this.ItemContainerGenerator.ContainerFromItem(item) as MSTreeViewItem;

                        if (tvItem != null)
                            tvItem.SelectAllExpandedChildren();
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        private void AddItemToSelection(MSTreeViewItem newItem)
        {
            if (!_selectedItems.Contains(newItem))
                _selectedItems.Add(newItem);
        }

        private void RemoveItemFromSelection(MSTreeViewItem newItem)
        {
            if (_selectedItems.Contains(newItem))
                _selectedItems.Remove(newItem);
        }

        private void ManageCtrlSelection(MSTreeViewItem viewItem)
        {
            bool isViewItemMultipleSelected = viewItem.IsSelected;

            if (isViewItemMultipleSelected)
                AddItemToSelection(viewItem);
            else if (!isViewItemMultipleSelected)
                RemoveItemFromSelection(viewItem);
        }

        private void ManageSingleSelection(MSTreeViewItem viewItem)
        {
            bool isViewItemMultipleSelected = viewItem.IsSelected;

            UnselectAll();

            if (isViewItemMultipleSelected)
            {
                viewItem.IsSelected = isViewItemMultipleSelected;
                AddItemToSelection(viewItem);
            }
        }
        /*
        public MSTreeViewItem GetItemFromData(int i)
        {
            return (MSTreeViewItem)(this.ItemContainerGenerator.ContainerFromIndex(i));
        }
        */

        private MSTreeViewItem HasSidlingItem(MSTreeViewItem item)
        {
            MSTreeViewItem parent = item.GetParentTreeViewItem();

            if (parent == null)
                return null;

            MSTreeViewItem sidling = null;

            foreach(var obj in parent.Items)
            {
                //object i = GetItemFromData(parent.Items.IndexOf(obj));

                // if (obj == null)
                //     continue;
                MSTreeViewItem i = parent.GetChildItemFromData(obj);
                if(i != item && i.IsSelected)
                {
                    sidling = i;
                    break;
                }
            }
            return sidling;
        }
        
        private void SelectBetween(MSTreeViewItem item1, MSTreeViewItem item2)
        {
            //if (item1.Parent != item2.Parent)
            //    return;
            
            MSTreeViewItem parent = item1.GetParentTreeViewItem();
            int index_1 = parent.Items.IndexOf(item1.DataContext);
            int index_2 = parent.Items.IndexOf(item2.DataContext);

            if(index_1 < index_2)
            {
                for(int i=index_1; i < index_2;i++)
                {
                    MSTreeViewItem item = parent.GetChildItemFromData(parent.Items[i]);
                    if (item == null)
                        continue;

                    item.Select();
                }
            }
            if(index_1 > index_2)
            {
                for (int i = index_1; i > index_2; i--)
                {
                    MSTreeViewItem item = parent.GetChildItemFromData(parent.Items[i]);
                    if (item == null)
                        continue;

                    item.Select();
                }
            }
        }

        /// <summary>
        /// ... TODO ...
        /// </summary>
        /// <param name="viewItem"></param>
        private void ManageShiftSelection(MSTreeViewItem viewItem)
        {
            bool isViewItemMultipleSelected = viewItem.IsSelected;
            if (!isViewItemMultipleSelected)
                return;

            MSTreeViewItem sidling = HasSidlingItem(viewItem);
            if (sidling == null)
                return;

            SelectBetween(viewItem, sidling);
        }

        //dep
        /// <summary>
        /// ... TODO ...
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        private bool IsItem1ListedBeforeItem2(MSTreeViewItem item1,
                                              MSTreeViewItem item2)
        {
            /*
            // Perform a Backword search (up)
            if (item1.ParentMultipleSelectionTreeViewItem != null) // item1 has a brother!
            {
                ItemCollection brothers = item1.ParentMultipleSelectionTreeViewItem.Items;
                int indexOfItem1 = brothers.IndexOf(item1);
                int indexOfItem2 = brothers.IndexOf(item2);
                if (indexOfItem2 >= 0) //item1 and item2 are brothers
                {
                    return indexOfItem1 < indexOfItem2 ? true : false;
                }

                
            }
            */
            
            return true;
        }

        /// <summary>
        /// ... TODO ...
        /// </summary>
        /// <param name="fromItem"></param>
        /// <param name="toItem"></param>
        private void SelectRange(MSTreeViewItem fromItem, 
                                 MSTreeViewItem toItem)
        {
        }
        #endregion

    }
}
