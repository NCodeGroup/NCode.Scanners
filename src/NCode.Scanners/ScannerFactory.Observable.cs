using System;
using System.Collections.ObjectModel;

namespace NCode.Scanners
{
	public static partial class ScannerFactory
	{
		public static IObservableScanner<T, ObservableCollection<T>> Observable<T>(this IScannerFactory factory, ObservableCollection<T> collection)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			return new ObservableScanner<T, ObservableCollection<T>>(factory, collection);
		}

		public static IObservableScanner<T, ReadOnlyObservableCollection<T>> Observable<T>(this IScannerFactory factory, ReadOnlyObservableCollection<T> collection)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			return new ObservableScanner<T, ReadOnlyObservableCollection<T>>(factory, collection);
		}

	}
}