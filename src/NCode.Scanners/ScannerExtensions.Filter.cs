using System;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		public static IFilterScanner<T> Include<T>(this IScanner<T> source, Func<T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, bool> adapter = (context, item) => predicate(item);

			return Include(source, adapter);
		}

		public static IFilterScanner<T> Include<T>(this IScanner<T> source, Func<IScanContext, T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);

			return filter.Include(predicate);
		}

		public static IFilterScanner<T> Exclude<T>(this IScanner<T> source, Func<T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, bool> adapter = (context, item) => predicate(item);

			return Exclude(source, adapter);
		}

		public static IFilterScanner<T> Exclude<T>(this IScanner<T> source, Func<IScanContext, T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);

			return filter.Exclude(predicate);
		}

	}
}