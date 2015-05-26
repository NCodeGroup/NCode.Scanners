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
using System.ComponentModel;

namespace NCode.Scanners
{
	/// <summary>
	/// A fluent interface that is used to scan for items of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface IScanner<out T> : INotifyPropertyChanged, INotifyCollectionChanged, IFluentInterface
	{
		/// <summary>
		/// Retrieves the collection of items that this scanner provides.
		/// </summary>
		/// <param name="context">An <see cref="IScanContext"/> that provides scanning options at runtime.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> that contains the items provided by this scanner.</returns>
		IEnumerable<T> Scan(IScanContext context);
	}

	/// <summary>
	/// Provides a base class for <see cref="IScanner{T}"/> implementations.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public abstract class Scanner<T> : IScanner<T>
	{
		#region IScanner<T> Members

		public abstract IEnumerable<T> Scan(IScanContext context);

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Event that is raised after a property on this scanner has changed.
		/// </summary>
		/// <remarks>
		/// Parent scanners should invalidate their caches (if any) when this event occurs.
		/// </remarks>
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Helper function to raise the <see cref="PropertyChanged"/> event with the specified property name.
		/// </summary>
		/// <param name="propertyName">Contains the name of the property that changed.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the specified event data.
		/// </summary>
		/// <param name="args">A <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			var handler = PropertyChanged;
			if (handler == null) return;
			handler(this, args);
		}

		#endregion

		#region INotifyCollectionChanged Members

		/// <summary>
		/// Event that is raised after the collection of items that this scanner provides has changed.
		/// </summary>
		/// <remarks>
		/// Parent scanners should invalidate their caches (if any) when this event occurs.
		/// </remarks>
		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Helper function to raise the <see cref="CollectionChanged"/> event.
		/// </summary>
		protected void OnCollectionChanged()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Raises the <see cref="CollectionChanged"/> event with the specified event data.
		/// </summary>
		/// <param name="args">A <see cref="NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var handler = CollectionChanged;
			if (handler == null) return;
			handler(this, args);
		}

		#endregion

	}
}