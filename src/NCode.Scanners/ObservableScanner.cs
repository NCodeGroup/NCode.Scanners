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

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that returns items from an
	/// observable collection and monitors for changes with <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <remarks>
	/// This scanner will propagate events from the observable collection using
	/// the <see cref="INotifyCollectionChanged"/> interface.
	/// </remarks>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	/// <typeparam name="TCollection">The type of the observable collection that must support <see cref="INotifyCollectionChanged"/>.</typeparam>
	public interface IObservableScanner<out T, out TCollection> : IScanner<T>
		where TCollection : IReadOnlyCollection<T>, INotifyCollectionChanged
	{
		/// <summary>
		/// Contains the observation collection. When items are added to this
		/// collection, this scanner will also raise it's <see cref="E:PropertyChanged"/>
		/// and <see cref="E:CollectionChanged"/> events.
		/// </summary>
		TCollection Items { get; }
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="IObservableScanner{T, TCollection}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	/// <typeparam name="TCollection">The type of the observable collection that must support <see cref="INotifyCollectionChanged"/>.</typeparam>
	public class ObservableScanner<T, TCollection> : Scanner<T>, IObservableScanner<T, TCollection>
		where TCollection : IReadOnlyCollection<T>, INotifyCollectionChanged
	{
		private readonly TCollection _collection;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableScanner{T, TCollection}"/> class with the specified observable collection.
		/// </summary>
		/// <param name="collection">The observable collection to use.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> argument is a null.</exception>
		public ObservableScanner(TCollection collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			_collection = collection;

			collection.CollectionChanged += HandleCollectionChanged;
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnPropertyChanged("Items");
			OnCollectionChanged();
		}

		#region ICollectionScanner<T> Members

		public virtual TCollection Items
		{
			get { return _collection; }
		}

		#endregion

		public override IEnumerable<T> Scan(IScanContext context)
		{
			return _collection;
		}

	}
}