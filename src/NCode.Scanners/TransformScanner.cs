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
	public interface ITransformScanner<out TIn, out TOut> : IScanner<TOut>, IUseParentScanner<TIn>
	{
		// nothing
	}

	public class TransformScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, TOut> _transform;

		public TransformScanner(IScanner<TIn> parent, Func<IScanContext, TIn, TOut> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException("transform");
			_transform = transform;
		}

		public override IEnumerable<TOut> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Select(item => _transform(context, item));
			return results;
		}
	}

	public class TransformManyScanner<TIn, TOut> : UseParentScanner<TIn, TOut>, ITransformScanner<TIn, TOut>
	{
		private readonly Func<IScanContext, TIn, IEnumerable<TOut>> _transform;

		public TransformManyScanner(IScanner<TIn> parent, Func<IScanContext, TIn, IEnumerable<TOut>> transform)
			: base(parent)
		{
			if (transform == null) throw new ArgumentNullException("transform");
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