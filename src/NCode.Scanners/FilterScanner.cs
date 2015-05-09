using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	public interface IFilterScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		IFilterScanner<T> Include(Func<T, bool> predicate);

		IFilterScanner<T> Exclude(Func<T, bool> predicate);
	}

	public class FilterScanner<T> : UseParentScanner<T>, IFilterScanner<T>
	{
		private readonly IList<Func<T, bool>> _includeList;
		private readonly IList<Func<T, bool>> _excludeList;

		public FilterScanner(IScanner<T> parent)
			: base(parent)
		{
			_includeList = new List<Func<T, bool>>();
			_excludeList = new List<Func<T, bool>>();
		}

		#region IFilterScanner<T> Members

		public virtual IFilterScanner<T> Include(Func<T, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");

			_includeList.Add(predicate);

			OnPropertyChanged("Include");
			OnCollectionChanged();

			return this;
		}

		public virtual IFilterScanner<T> Exclude(Func<T, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");
			if (_includeList.Count == 0) throw new InvalidOperationException("At least one include must exist before an exclude can be added.");

			_excludeList.Add(predicate);

			OnPropertyChanged("Exclude");
			OnCollectionChanged();

			return this;
		}

		#endregion

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var result = items
				.Where(item => _includeList.Any(predicate => predicate(item)))
				.Where(item => !_excludeList.Any(predicate => predicate(item)));
			return result;
		}

		#endregion
	}
}