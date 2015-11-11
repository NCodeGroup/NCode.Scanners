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
	/// Represents an <see cref="IScanner{T}"/> that can project items into
	/// another representation using a function delegate.
	/// </summary>
	/// <typeparam name="TIn">The type of item that this scanner uses from it's parent.</typeparam>
	/// <typeparam name="TOut">The type of item that this scanner provides.</typeparam>
	public interface ITransformScanner<out TIn, out TOut> : IScanner<TOut>, IUseParentScanner<TIn>
	{
		// nothing
	}

	/// <summary>
	/// Provides the implementation of <see cref="ITransformScanner{TIn,TOut}"/> class.
	/// </summary>
	/// <typeparam name="TIn">The type of item that this scanner uses from it's parent.</typeparam>
	/// <typeparam name="TOut">The type of item that this scanner provides.</typeparam>
	public class TransformScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, TOut> _transform;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformScanner{TIn, TOut}"/> class.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="transform">A transform function to apply to each item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="transform"/> argument is a null.</exception>
		public TransformScanner(IScanner<TIn> parent, Func<IScanContext, TIn, TOut> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException(nameof(transform));
			_transform = transform;
		}

		public override IEnumerable<TOut> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Select(item => _transform(context, item));
			return results;
		}
	}

	/// <summary>
	/// Provides the implementation of <see cref="ITransformScanner{TIn,TOut}"/>
	/// class that flattens the resulting sequence.
	/// </summary>
	/// <typeparam name="TIn">The type of item that this scanner uses from it's parent.</typeparam>
	/// <typeparam name="TOut">The type of item that this scanner provides.</typeparam>
	public class TransformManyScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, IEnumerable<TOut>> _transform;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformManyScanner{TIn, TOut}"/> class.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="transform">A transform function to apply to each item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="transform"/> argument is a null.</exception>
		public TransformManyScanner(IScanner<TIn> parent, Func<IScanContext, TIn, IEnumerable<TOut>> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException(nameof(transform));
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