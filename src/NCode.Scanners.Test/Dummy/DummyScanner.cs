using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NCode.Scanners.Test.Dummy
{
	public class DummyScanner<T> : Scanner<T>
	{
		public void FireOnPropertyChanged(PropertyChangedEventArgs args)
		{
			OnPropertyChanged(args);
		}

		public void FireOnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			OnCollectionChanged(args);
		}

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return Enumerable.Empty<T>();
		}
	}
}