using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NCode.Scanners
{
	public interface IUseParentScanner<out T>
	{
		IScanner<T> Parent { get; }
	}

	public abstract class UseParentScanner<TIn, TOut> : Scanner<TOut>, IUseParentScanner<TIn>
	{
		private readonly IScanner<TIn> _parent;

		protected UseParentScanner(IScanner<TIn> parent)
		{
			if (parent == null) throw new ArgumentNullException("parent");
			_parent = parent;

			parent.PropertyChanged += HandlePropertyChanged;
			parent.CollectionChanged += HandleCollectionChanged;
		}

		#region IUseParentScanner<TIn> Members

		public virtual IScanner<TIn> Parent
		{
			get { return _parent; }
		}

		#endregion

		private void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("Parent"));
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected virtual IEnumerable<TIn> GetParentItemsOrEmpty(IScanContext context)
		{
			var parent = Parent ?? ImmutableScanner<TIn>.Empty;
			var items = parent.Scan(context) ?? Enumerable.Empty<TIn>();
			return items;
		}

	}

	public abstract class UseParentScanner<T> : UseParentScanner<T, T>
	{
		protected UseParentScanner(IScanner<T> parent)
			: base(parent)
		{
			// nothing
		}
	}

}