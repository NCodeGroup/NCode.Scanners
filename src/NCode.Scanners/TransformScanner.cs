using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public interface ITransformScanner<out TIn, out TOut> : IScanner<TOut>, IUseParentScanner<TIn>
	{
		// nothing
	}

	public class TransformScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, TOut> _transform;

		public TransformScanner(IScanner<TIn> parent, Func<IScanContext, TIn, TOut> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException("transform");
			_transform = transform;
		}

		public override IEnumerable<TOut> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Select(item => _transform(context, item));
			return results;
		}
	}

	public class TransformManyScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, IEnumerable<TOut>> _transform;

		public TransformManyScanner(IScanner<TIn> parent, Func<IScanContext, TIn, IEnumerable<TOut>> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException("transform");
			_transform = transform;
		}

		public override IEnumerable<TOut> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.SelectMany(item => _transform(context, item));
			return results;
		}
	}

}