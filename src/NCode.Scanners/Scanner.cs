using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NCode.Scanners
{
	public interface IScanner<out T> : INotifyPropertyChanged, INotifyCollectionChanged, IFluentInterface
	{
		IEnumerable<T> Scan(IScanContext context);
	}

	public abstract class Scanner<T> : IScanner<T>
	{
		#region IScanner<T> Members

		public abstract IEnumerable<T> Scan(IScanContext context);

		#endregion

		#region INotifyPropertyChanged Members

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			var handler = PropertyChanged;
			if (handler == null) return;
			handler(this, args);
		}

		#endregion

		#region INotifyCollectionChanged Members

		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected void OnCollectionChanged()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var handler = CollectionChanged;
			if (handler == null) return;
			handler(this, args);
		}

		#endregion

	}
}