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
	/// Represents an <see cref="IScanner{T}"/> that returns distinct items from
	/// another scanner by using an <see cref="IEqualityComparer{T}"/> to compare
	/// the items.
	/// </summary>
	/// <remarks>
	/// This scanner will propagate events from it's parent scanner.
	/// </remarks>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface IDistinctScanner<T> : IScanner<T>, IUseParentScanner<T>
	{
		/// <summary>
		/// Contains the <see cref="IEqualityComparer{T}"/> that is used to compare items.
		/// </summary>
		IEqualityComparer<T> Comparer { get; }
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="IDistinctScanner{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public class DistinctScanner<T> : UseParentScanner<T>, IDistinctScanner<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DistinctScanner{T}"/> class that uses the specified <see cref="IScanner{T}"/> and <see cref="IEqualityComparer{T}"/> for comparing items.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing items.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="comparer"/> argument is a null.</exception>
		public DistinctScanner(IScanner<T> parent, IEqualityComparer<T> comparer)
			: base(parent)
		{
			if (comparer == null) throw new ArgumentNullException("comparer");
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