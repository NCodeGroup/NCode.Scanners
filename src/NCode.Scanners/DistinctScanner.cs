using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public interface IDistinctScanner<T> : IScanner<T>
	{
		IEqualityComparer<T> Comparer { get; }
	}

	public class DistinctScanner<T> : UseParentScanner<T>, IDistinctScanner<T>
	{
		public DistinctScanner(IScanner<T> parent, IEqualityComparer<T> comparer)
			: base(parent)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			Comparer = comparer;
		}

		public virtual IEqualityComparer<T> Comparer { get; private set; }

		public override IEnumerable<T> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Distinct(Comparer);
			return results;
		}

	}
}