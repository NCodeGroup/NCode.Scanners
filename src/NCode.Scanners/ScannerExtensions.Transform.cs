using System;
using System.Collections.Generic;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<TIn, TOut> transform)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (transform == null) throw new ArgumentNullException(nameof(transform));

			Func<IScanContext, TIn, TOut> adapter = (context, input) => transform(input);

			return new TransformScanner<TIn, TOut>(source, adapter);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<IScanContext, TIn, TOut> transform)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (transform == null) throw new ArgumentNullException(nameof(transform));

			return new TransformScanner<TIn, TOut>(source, transform);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<TIn, IEnumerable<TOut>> transform)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (transform == null) throw new ArgumentNullException(nameof(transform));

			Func<IScanContext, TIn, IEnumerable<TOut>> adapter = (context, input) => transform(input);

			return new TransformManyScanner<TIn, TOut>(source, adapter);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<IScanContext, TIn, IEnumerable<TOut>> transform)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (transform == null) throw new ArgumentNullException(nameof(transform));

			return new TransformManyScanner<TIn, TOut>(source, transform);
		}

	}
}