using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NCode.Scanners
{
	public interface IAggregateScanner<T> : IScanner<T>
	{
		ICollection<IScanner<T>> Scanners { get; }
	}

	public class AggregateScanner<T> : Scanner<T>, IAggregateScanner<T>
	{
		public AggregateScanner()
			: this(Enumerable.Empty<IScanner<T>>())
		{
			// nothing
		}

		public AggregateScanner(IEnumerable<IScanner<T>> scanners)
		{
			if (scanners == null) throw new ArgumentNullException("scanners");

			var collection = new ScannerCollection<T>();
			foreach (var scanner in scanners)
			{
				// Add items to the collection after we instantiate the collection
				// so that each item is subscribed to the notify events.
				collection.Add(scanner);
			}

			// subscribe to the event after we have added all the items
			collection.CollectionChanged += HandleCollectionChanged;
			Scanners = collection;
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnPropertyChanged("Scanners");
			OnCollectionChanged();
		}

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return Scanners.SelectMany(scanner => scanner.Scan(context));
		}

		#endregion

		#region IAggregateScanner<T> Members

		public virtual ICollection<IScanner<T>> Scanners { get; private set; }

		#endregion
	}

	/// <remarks>
	/// We derive from <see cref="ObservableCollection{T}"/> so that we can leverage its reentrancy logic.
	/// </remarks>
	internal class ScannerCollection<T> : ObservableCollection<IScanner<T>>
	{
		private void Subscribe(IScanner<T> item)
		{
			if (item == null) return;
			item.PropertyChanged += HandlePropertyChanged;
			item.CollectionChanged += HandleCollectionChanged;
		}

		private void Unsubscribe(IScanner<T> item)
		{
			if (item == null) return;
			item.PropertyChanged -= HandlePropertyChanged;
			item.CollectionChanged -= HandleCollectionChanged;
		}

		private void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(args.PropertyName));
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void InsertItem(int index, IScanner<T> item)
		{
			base.InsertItem(index, item);
			Subscribe(item);
		}

		protected override void RemoveItem(int index)
		{
			var item = base[index];
			base.RemoveItem(index);
			Unsubscribe(item);
		}

		protected override void SetItem(int index, IScanner<T> item)
		{
			var prevItem = base[index];
			base.SetItem(index, item);
			Unsubscribe(prevItem);
			Subscribe(item);
		}

		protected override void ClearItems()
		{
			var items = Items.ToArray();
			base.ClearItems();

			foreach (var item in items)
			{
				Unsubscribe(item);
			}
		}

	}
}