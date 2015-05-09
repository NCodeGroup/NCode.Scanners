using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NCode.Scanners
{
	public interface IObservableScanner<out T, out TCollection> : IScanner<T>
		where TCollection : IReadOnlyCollection<T>, INotifyCollectionChanged
	{
		TCollection Items { get; }
	}

	public class ObservableScanner<T, TCollection> : Scanner<T>, IObservableScanner<T, TCollection>
		where TCollection : IReadOnlyCollection<T>, INotifyCollectionChanged
	{
		private readonly TCollection _collection;

		public ObservableScanner(TCollection collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			_collection = collection;

			collection.CollectionChanged += HandleCollectionChanged;
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnPropertyChanged("Items");
			OnCollectionChanged();
		}

		#region ICollectionScanner<T> Members

		public virtual TCollection Items
		{
			get { return _collection; }
		}

		#endregion

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return _collection;
		}

	}
}