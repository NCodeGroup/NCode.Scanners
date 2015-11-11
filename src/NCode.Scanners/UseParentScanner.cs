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
using System.Linq;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that can be used to create a
	/// hierarchy of parent-child scanners.
	/// </summary>
	/// <remarks>
	/// This interface does not derive from <see cref="IScanner{T}"/> with the
	/// same generic <typeparamref name="T"/> argument because the parent and
	/// child scanners may use different item types.
	/// 
	/// In order to propagate events from parent-child hierarchies, this scanner
	/// will automatically subscribe to the parent's <see cref="E:PropertyChanged"/>
	/// and <see cref="E:CollectionChanged"/> events and then raise the
	/// corresponding event again for the current scanner. When <see cref="E:PropertyChanged"/>
	/// is raised again, the property name for <see cref="PropertyChangedEventArgs"/>
	/// will always be <c>Parent</c>. When <see cref="E:CollectionChanged"/> is
	/// raised again, the changed action for <see cref="NotifyCollectionChangedEventArgs"/>
	/// will always have the <see cref="NotifyCollectionChangedAction.Reset"/> value.
	/// </remarks>
	/// <typeparam name="T">The type of item that the parent scanner provides.</typeparam>
	public interface IUseParentScanner<out T>
	{
		/// <summary>
		/// Contains the parent <see cref="IScanner{T}"/> that this child scanner uses.
		/// </summary>
		IScanner<T> Parent { get; }
	}

	/// <summary>
	/// Provides a base class for <see cref="IUseParentScanner{T}"/> implementations
	/// that use different item types for the parent and child scanners.
	/// </summary>
	/// <typeparam name="TIn">The type of item that the parent scanner provides.</typeparam>
	/// <typeparam name="TOut">The type of item that this child scanner provides.</typeparam>
	public abstract class UseParentScanner<TIn, TOut> : Scanner<TOut>, IUseParentScanner<TIn>
	{
		private readonly IScanner<TIn> _parent;

		private static IScanner<TIn> NonNull(IScanner<TIn> parent)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			return parent;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="UseParentScanner{TIn,TOut}"/> that uses the specified parent scanner.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{TIn}"/> that this child scanner uses.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		protected UseParentScanner(IScanner<TIn> parent)
			: base(NonNull(parent).Factory)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			_parent = parent;

			parent.PropertyChanged += HandlePropertyChanged;
			parent.CollectionChanged += HandleCollectionChanged;
		}

		#region IUseParentScanner<TIn> Members

		public virtual IScanner<TIn> Parent
		{
			get { return _parent; }
		}

		#endregion

		private void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("Parent"));
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Helper function for child scanner implementations that will always
		/// returns a valid non-null collection of items from the parent <see cref="IScanner{TIn}"/>.
		/// </summary>
		/// <param name="context">An <see cref="IScanContext"/> that provides scanning options at runtime.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> that contains the items provided by the <see cref="Parent"/> scanner.</returns>
		protected virtual IEnumerable<TIn> GetParentItemsOrEmpty(IScanContext context)
		{
			var parent = Parent ?? context.Factory.Empty<TIn>();
			var items = parent.Scan(context) ?? Enumerable.Empty<TIn>();
			return items;
		}
	}

	/// <summary>
	/// Provides a base class for <see cref="IUseParentScanner{T}"/> implementations
	/// that use the same item type for the parent and child scanners.
	/// </summary>
	/// <typeparam name="T">The type of item provided by the parent and child scanners.</typeparam>
	public abstract class UseParentScanner<T> : UseParentScanner<T, T>
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UseParentScanner{T}"/> that uses the specified parent scanner.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this child scanner uses.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		protected UseParentScanner(IScanner<T> parent)
			: base(parent)
		{
			// nothing
		}
	}

}