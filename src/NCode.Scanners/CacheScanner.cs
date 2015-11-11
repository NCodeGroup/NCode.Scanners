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
using System.Collections.Specialized;
using System.Linq;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that automatically caches items
	/// from another scanner.
	/// </summary>
	/// <remarks>
	/// In addition to propagating the events from it's parent, this scanner will
	/// also cache the items from it's parent and continue to return those items
	/// until the cache is invalidated. The cache of items will be invalidated
	/// automatically when the parent raises it's <see cref="E:CollectionChanged"/>
	/// event or when the <see cref="Invalidate"/> method is called.
	/// </remarks>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface ICacheScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		/// <summary>
		/// Invalidates the cache of items so that the next call to <see cref="M:Scan"/>
		/// will retrieve the items from the parent scanner and cache them again.
		/// </summary>
		void Invalidate();
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="ICacheScanner{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public class CacheScanner<T> : UseParentScanner<T>, ICacheScanner<T>
	{
		private T[] _cache;

		/// <summary>
		/// Initializes a new instance of <see cref="CacheScanner{T}"/> that uses the specified parent scanner.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
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