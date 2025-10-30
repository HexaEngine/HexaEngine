namespace HexaEngine.UI
{
    using System;
    using System.Collections.ObjectModel;

    public class ResourceDictionaryCollection : ObservableCollection<ResourceDictionary>
    {
        private readonly ResourceDictionary owner;

        internal ResourceDictionaryCollection(ResourceDictionary owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            for (int i = 0; i < Count; i++)
            {
                owner.RemoveParentOwners(this[i]);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, ResourceDictionary item)
        {
            ArgumentNullException.ThrowIfNull(item);

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, ResourceDictionary item)
        {
            ArgumentNullException.ThrowIfNull(item);

            base.SetItem(index, item);
        }
    }
}