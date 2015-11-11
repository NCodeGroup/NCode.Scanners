using System;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		public static ICacheScanner<T> Cache<T>(this IScanner<T> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return new CacheScanner<T>(source);
		}
	}
}