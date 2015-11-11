using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NCode.Scanners
{
	/// <summary>
	/// Provides an implementation of <see cref="ObservableCollection{T}"/>
	/// that will raise events when the child <see cref="IScanner{T}"/> are modified.
	/// </summary>
	/// <typeparam name="T">The type of scanners in the collection.</typeparam>
	/// <remarks>
	/// We derive from <see cref="ObservableCollection{T}"/> so that we can leverage its reentrancy logic.
	/// </remarks>
	internal class ScannerCollection<T> : ObservableCollection<IScanner<T>>
	{
		public ScannerCollection()
		{
			// nothing
		}

		public ScannerCollection(IEnumerable<IScanner<T>> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			foreach (var item in collection)
			{
				Items.Add(item);
				Subscribe(item);
			}
		}

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
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
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