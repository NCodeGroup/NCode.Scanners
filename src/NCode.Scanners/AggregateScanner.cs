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
	/// Represents an <see cref="IScanner{T}"/> that combines a collection of <see cref="IScanner{T}"/> objects.
	/// </summary>
	/// <remarks>
	/// This scanner will propagate any events from it's collection of scanners.
	/// </remarks>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface IAggregateScanner<T> : IScanner<T>
	{
		/// <summary>
		/// Contains the collection of <see cref="IScanner{T}"/> objects.
		/// </summary>
		ICollection<IScanner<T>> Scanners { get; }
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="IAggregateScanner{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public class AggregateScanner<T> : Scanner<T>, IAggregateScanner<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateScanner{T}"/> class.
		/// </summary>
		public AggregateScanner(IScannerFactory factory)
			: this(factory, Enumerable.Empty<IScanner<T>>())
		{
			// nothing
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateScanner{T}"/> class with the specified collection of scanners.
		/// </summary>
		/// <param name="factory">The <see cref="IScannerFactory"/> to use when creating objects.</param>
		/// <param name="scanners">A collection of <see cref="IScanner{T}"/> objects to add to the <see cref="AggregateScanner{T}"/>.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="scanners"/> argument is a null.</exception>
		public AggregateScanner(IScannerFactory factory, IEnumerable<IScanner<T>> scanners)
			: base(factory)
		{
			if (scanners == null) throw new ArgumentNullException(nameof(scanners));

			var collection = new ScannerCollection<T>(scanners);
			collection.CollectionChanged += HandleCollectionChanged;
			Scanners = collection;
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnPropertyChanged("Scanners");
			OnCollectionChanged();
		}

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return Scanners.SelectMany(scanner => scanner.Scan(context));
		}

		#endregion

		#region IAggregateScanner<T> Members

		public virtual ICollection<IScanner<T>> Scanners { get; }

		#endregion

	}
}