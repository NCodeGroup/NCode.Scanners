#region Copyright Preamble
// 
//    Copyright © 2015 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that can filter items from it's
	/// parent scanner by using function delegates to include and exclude items.
	/// </summary>
	/// <remarks>
	/// Note that the methods on this scanner always append additional include
	/// or exclude function predicates.
	/// </remarks> 
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface IFilterScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		/// <summary>
		/// Fluent method that adds additional function predicates to <c>include</c>
		/// items from the parent scanner.
		/// </summary>
		/// <param name="predicate">A function delegate that returns <c>true</c> when
		/// items need to be included.</param>
		/// <returns>The <see cref="IFilterScanner{T}"/> fluent interface.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="predicate"/>
		/// argument is a null.</exception>
		IFilterScanner<T> Include(Func<IScanContext, T, bool> predicate);

		/// <summary>
		/// Fluent method that adds additional function predicates to <c>include</c>
		/// items from the parent scanner.
		/// </summary>
		/// <param name="predicate">A function delegate that returns <c>true</c> when
		/// items needs to be excluded.</param>
		/// <returns>The <see cref="IFilterScanner{T}"/> fluent interface.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="predicate"/>
		/// argument is a null.</exception>
		/// <exception cref="InvalidOperationException">Adding an exclude without any
		/// includes.</exception>
		IFilterScanner<T> Exclude(Func<IScanContext, T, bool> predicate);
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="IFilterScanner{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public class FilterScanner<T> : UseParentScanner<T>, IFilterScanner<T>
	{
		private readonly IList<Func<IScanContext, T, bool>> _includeList;
		private readonly IList<Func<IScanContext, T, bool>> _excludeList;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterScanner{T}"/> class
		/// with the specified <see cref="IScanner{T}"/> as it's parent.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		public FilterScanner(IScanner<T> parent)
			: base(parent)
		{
			_includeList = new List<Func<IScanContext, T, bool>>();
			_excludeList = new List<Func<IScanContext, T, bool>>();
		}

		#region IFilterScanner<T> Members

		public virtual IFilterScanner<T> Include(Func<IScanContext, T, bool> predicate)
		{
			if (predicate == null) throw new ArgumentNullException("predicate");

			_includeList.Add(predicate);

			OnPropertyChanged("Include");
			OnCollectionChanged();

			return this;
		}

		public virtual IFilterScanner<T> Exclude(Func<IScanContext, T, bool> predicate)
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
				.Where(item => _includeList.Any(predicate => predicate(context, item)))
				.Where(item => !_excludeList.Any(predicate => predicate(context, item)));
			return result;
		}

		#endregion

	}
}