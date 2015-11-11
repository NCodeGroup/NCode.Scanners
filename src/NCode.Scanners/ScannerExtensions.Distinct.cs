using System;
using System.Collections.Generic;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		public static IDistinctScanner<T> Distinct<T>(this IScanner<T> source)
		{
			var comparer = EqualityComparer<T>.Default;

			return Distinct(source, comparer);
		}

		public static IDistinctScanner<T> Distinct<T>(this IScanner<T> source, IEqualityComparer<T> comparer)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

			return new DistinctScanner<T>(source, comparer);
		}

	}
}