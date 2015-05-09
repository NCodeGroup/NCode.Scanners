using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace NCode.Scanners
{
	public interface ICacheScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		void Invalidate();
	}

	public class CacheScanner<T> : UseParentScanner<T>, ICacheScanner<T>
	{
		private T[] _cache;

		public CacheScanner(IScanner<T> parent)
			: base(parent)
		{
			// nothing
		}

		#region ICacheScanner<T> Members

		public virtual void Invalidate()
		{
			_cache = null;
		}

		#endregion

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			Invalidate();
			base.OnCollectionChanged(args);
		}

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			var cache = _cache;
			if (cache != null)
				return cache;

			var items = GetParentItemsOrEmpty(context);
			cache = items.ToArray();
			_cache = cache;

			return cache;
		}

		#endregion
	}
}