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
	public static class TypeExtensions
	{
		public static IEnumerable<Type> IsType<TType>(this IEnumerable<Type> source)
		{
			return IsType(source, typeof(TType));
		}

		public static IEnumerable<TypeInfo> IsType<TType>(this IEnumerable<TypeInfo> source)
		{
			return IsType(source, typeof(TType));
		}

		public static IEnumerable<Type> NotIsType<TType>(this IEnumerable<Type> source)
		{
			return IsType(source, typeof(TType), false);
		}

		public static IEnumerable<TypeInfo> NotIsType<TType>(this IEnumerable<TypeInfo> source)
		{
			return IsType(source, typeof(TType), false);
		}

		public static IEnumerable<TItem> IsType<TItem>(this IEnumerable<TItem> source, Type type, bool expected = true)
			where TItem : Type
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (type == null) throw new ArgumentNullException(nameof(type));
			return source.Where(iter => type.IsAssignableFrom(iter) == expected);
		}

		public static IEnumerable<Type> IsDefined<TAttribute>(this IEnumerable<Type> source, bool inherit = true)
			where TAttribute : Attribute
		{
			return IsDefined(source, typeof(TAttribute), inherit);
		}

		public static IEnumerable<TypeInfo> IsDefined<TAttribute>(this IEnumerable<TypeInfo> source, bool inherit = true)
			where TAttribute : Attribute
		{
			return IsDefined(source, typeof(TAttribute), inherit);
		}

		public static IEnumerable<Assembly> IsDefined<TAttribute>(this IEnumerable<Assembly> source, bool inherit = true)
			where TAttribute : Attribute
		{
			return IsDefined(source, typeof(TAttribute), inherit);
		}

		public static IEnumerable<TItem> IsDefined<TItem>(this IEnumerable<TItem> source, Type attributeType, bool inherit = true)
			where TItem : ICustomAttributeProvider
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));

			return source.Where(iter => iter.IsDefined(attributeType, inherit));
		}

		public static IEnumerable<TItem> IsDefined<TItem, TAttribute>(this IEnumerable<TItem> source, Func<TItem, TAttribute, bool> predicate, bool inherit = true)
			where TItem : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			var attributeType = typeof(TAttribute);

			return source.Where(item => item
				.GetCustomAttributes(attributeType, inherit)
				.OfType<TAttribute>()
				.Any(attr => predicate(item, attr)));
		}

		public static IEnumerable<TItem> IsDefined<TItem, TAttribute>(this IEnumerable<TItem> source, Func<TAttribute, bool> predicate, bool inherit = true)
			where TItem : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<TItem, TAttribute, bool> adapter = (item, attr) => predicate(attr);

			return IsDefined(source, adapter, inherit);
		}

	}
}