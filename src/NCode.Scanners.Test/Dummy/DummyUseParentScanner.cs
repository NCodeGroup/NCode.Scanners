using System.Collections.Generic;

namespace NCode.Scanners.Test.Dummy
{
	public class DummyUseParentScanner<T> : UseParentScanner<T>
	{
		public DummyUseParentScanner(IScanner<T> parent)
			: base(parent)
		{
			// nothing
		}

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return GetParentItemsOrEmpty(context);
		}
	}
}