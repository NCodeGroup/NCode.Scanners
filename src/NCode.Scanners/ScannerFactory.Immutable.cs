using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public static partial class ScannerFactory
	{
		public static IScanner<T> Empty<T>(this IScannerFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			return new ImmutableScanner<T>(factory, Enumerable.Empty<T>());
		}

		public static IScanner<T> Immutable<T>(this IScannerFactory factory, params T[] source)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new ImmutableScanner<T>(factory, source);
		}

		public static IScanner<T> Immutable<T>(this IScannerFactory factory, IEnumerable<T> source)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new ImmutableScanner<T>(factory, source);
		}

		public static IScanner<T> AsScanner<T>(this IEnumerable<T> source, IScannerFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new ImmutableScanner<T>(factory, source);
		}

	}
}