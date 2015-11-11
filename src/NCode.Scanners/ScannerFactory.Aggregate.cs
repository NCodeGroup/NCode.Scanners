using System;
using System.Collections.Generic;

namespace NCode.Scanners
{
	public static partial class ScannerFactory
	{
		public static IAggregateScanner<T> Aggregate<T>(this IScannerFactory factory, params IScanner<T>[] source)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new AggregateScanner<T>(factory, source);
		}

		public static IAggregateScanner<T> Aggregate<T>(this IScannerFactory factory, IEnumerable<IScanner<T>> source)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new AggregateScanner<T>(factory, source);
		}

	}
}