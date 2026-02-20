namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Counts INotifyCollectionChanged events by action type.
    /// We count events (notifications), not items.
    /// </summary>
    public sealed class CollectionChangedCounter
    {
        public int AddEvents
        {
            get; private set;
        }
        public int RemoveEvents
        {
            get; private set;
        }
        public int ResetEvents
        {
            get; private set;
        }
        public int ReplaceEvents
        {
            get; private set;
        }
        public int MoveEvents
        {
            get; private set;
        }

        public void Handler(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                AddEvents++;
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                RemoveEvents++;
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                ResetEvents++;
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                ReplaceEvents++;
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                MoveEvents++;
                return;
            }
        }
    }
}
