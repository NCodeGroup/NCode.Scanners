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
using System.Reflection;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that uses <see cref="ReflectionContext"/>
	/// to customize the representation of an object using a reflection context.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public interface IReflectionScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		/// <summary>
		/// Contains the context that can provide reflection over objects.
		/// </summary>
		ReflectionContext ReflectionContext { get; }
	}

	/// <summary>
	/// Provides the base implementation for the <see cref="IReflectionScanner{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of item that this scanner provides.</typeparam>
	public abstract class ReflectionScanner<T> : UseParentScanner<T>, IReflectionScanner<T>
	{
		private readonly ReflectionContext _reflectionContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionScanner{T}"/>
		/// class with the specified reflection context.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="reflectionContext">The context used to reflect over types.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="reflectionContext"/> argument is a null.</exception>
		protected ReflectionScanner(IScanner<T> parent, ReflectionContext reflectionContext)
			: base(parent)
		{
			if (reflectionContext == null)
				throw new ArgumentNullException("reflectionContext");

			_reflectionContext = reflectionContext;
		}

		/// <summary>
		/// Gets the representation, in this reflection context, of an item that is
		/// represented by an object from another reflection context.
		/// </summary>
		/// <param name="item">The external representation of the item to represent
		/// in this context.</param>
		/// <returns>The representation of the item in this reflection context.</returns>
		protected abstract T MapItem(T item);

		#region IReflectionScanner<T> Members

		public virtual ReflectionContext ReflectionContext
		{
			get { return _reflectionContext; }
		}

		#endregion

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Select(MapItem);
			return results;
		}

		#endregion
	}

	/// <summary>
	/// Provides the implementation for the <see cref="IReflectionScanner{T}"/>
	/// for <see cref="Assembly"/> objects.
	/// </summary>
	public class ReflectionAssemblyScanner : ReflectionScanner<Assembly>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionAssemblyScanner"/> class.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="reflectionContext">The context used to reflect over <see cref="Assembly"/> objects.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="reflectionContext"/> argument is a null.</exception>
		public ReflectionAssemblyScanner(IScanner<Assembly> parent, ReflectionContext reflectionContext)
			: base(parent, reflectionContext)
		{
			// nothing
		}

		protected override Assembly MapItem(Assembly item)
		{
			return ReflectionContext.MapAssembly(item);
		}
	}

	/// <summary>
	/// Provides the implementation for the <see cref="IReflectionScanner{T}"/>
	/// for <see cref="TypeInfo"/> objects.
	/// </summary>
	public class ReflectionTypeInfoScanner : ReflectionScanner<TypeInfo>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionTypeInfoScanner"/> class.
		/// </summary>
		/// <param name="parent">The parent <see cref="IScanner{T}"/> that this scanner retieves it's items from.</param>
		/// <param name="reflectionContext">The context used to reflect over <see cref="TypeInfo"/> objects.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="parent"/> argument is a null.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="reflectionContext"/> argument is a null.</exception>
		public ReflectionTypeInfoScanner(IScanner<TypeInfo> parent, ReflectionContext reflectionContext)
			: base(parent, reflectionContext)
		{
			// nothing
		}

		protected override TypeInfo MapItem(TypeInfo item)
		{
			return ReflectionContext.MapType(item);
		}
	}

}