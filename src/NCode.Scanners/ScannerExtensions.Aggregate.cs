using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, params IScanner<T>[] scanners)
		{
			if (scanners == null) throw new ArgumentNullException(nameof(scanners));

			return Aggregate(source, scanners.AsEnumerable());
		}

		public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, IEnumerable<IScanner<T>> scanners)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (scanners == null) throw new ArgumentNullException(nameof(scanners));

			var aggregate = source as IAggregateScanner<T>;
			if (aggregate == null)
			{
				aggregate = new AggregateScanner<T>(source.Factory);
				aggregate.Scanners.Add(source);
			}

			foreach (var scanner in scanners)
			{
				aggregate.Scanners.Add(scanner);
			}

			return aggregate;
		}

	}
}