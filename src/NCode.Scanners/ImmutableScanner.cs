using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public class ImmutableScanner<T> : Scanner<T>
	{
		private readonly IEnumerable<T> _source;

		public static readonly IScanner<T> Empty = new ImmutableScanner<T>(Enumerable.Empty<T>());

		public ImmutableScanner(IEnumerable<T> source)
		{
			_source = source ?? Enumerable.Empty<T>();
		}

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return _source;
		}
	}
}