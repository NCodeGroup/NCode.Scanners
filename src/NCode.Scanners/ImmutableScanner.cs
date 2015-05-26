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

using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that returns a fixed collection of items.
	/// </summary>
	/// <remarks>
	/// This scanner simply stores and returns the <see cref="IEnumerable{T}"/>
	/// sequence that was originally provided. Care must be taken with sequences
	/// that use deferred execution from <c>LINQ</c>.
	/// </remarks>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public class ImmutableScanner<T> : Scanner<T>
	{
		private readonly IEnumerable<T> _source;

		/// <summary>
		/// Returns an empty <see cref="IScanner{T}"/>.
		/// </summary>
		public static readonly IScanner<T> Empty = new ImmutableScanner<T>(Enumerable.Empty<T>());

		/// <summary>
		/// Intializes a new instance of the <see cref="ImmutableScanner{T}"/> class with the specified collection of items.
		/// </summary>
		/// <param name="source">The collection of items that this scanner will return.</param>
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