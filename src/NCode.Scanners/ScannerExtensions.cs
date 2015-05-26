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
using System.IO;
using System.Linq;
using System.Reflection;

namespace NCode.Scanners
{
	public static class ScannerExtensions
	{
		public static ICacheScanner<T> Cache<T>(this IScanner<T> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return new CacheScanner<T>(source);
		}

		public static IDistinctScanner<T> Distinct<T>(this IScanner<T> source, IEqualityComparer<T> comparer)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (comparer == null) throw new ArgumentNullException("comparer");
			return new DistinctScanner<T>(source, comparer);
		}

		public static IDistinctScanner<T> Distinct<T>(this IScanner<T> source)
		{
			var comparer = EqualityComparer<T>.Default;
			return Distinct(source, comparer);
		}

		public static IReflectionScanner<Assembly> Reflect(this IScanner<Assembly> source, ReflectionContext context)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (context == null) throw new ArgumentNullException("context");
			return new ReflectionAssemblyScanner(source, context);
		}

		public static IReflectionScanner<TypeInfo> Reflect(this IScanner<TypeInfo> source, ReflectionContext context)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (context == null) throw new ArgumentNullException("context");
			return new ReflectionTypeInfoScanner(source, context);
		}

		public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, params IScanner<T>[] scanners)
		{
			if (scanners == null) throw new ArgumentNullException("scanners");
			return Aggregate(source, scanners.AsEnumerable());
		}

		public static IAggregateScanner<T> Aggregate<T>(this IScanner<T> source, IEnumerable<IScanner<T>> scanners)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (scanners == null) throw new ArgumentNullException("scanners");
			var aggregate = source as IAggregateScanner<T>;
			if (aggregate == null)
			{
				aggregate = new AggregateScanner<T>();
				aggregate.Scanners.Add(source);
			}
			foreach (var scanner in scanners)
			{
				aggregate.Scanners.Add(scanner);
			}
			return aggregate;
		}

		public static IFilterScanner<T> Include<T>(this IScanner<T> source, Func<T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (predicate == null) throw new ArgumentNullException("predicate");
			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);
			return filter.Include(predicate);
		}

		public static IFilterScanner<T> Exclude<T>(this IScanner<T> source, Func<T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (predicate == null) throw new ArgumentNullException("predicate");
			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);
			return filter.Exclude(predicate);
		}

		public static IFilterScanner<T> IsDefined<T>(this IScanner<T> source, Type attributeType, bool inherit = true)
			where T : ICustomAttributeProvider
		{
			if (source == null) throw new ArgumentNullException("source");
			if (attributeType == null) throw new ArgumentNullException("attributeType");
			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);
			return filter.Include(item => item.IsDefined(attributeType, inherit));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<T, TAttribute, bool> predicate, bool inherit = true)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException("source");
			if (predicate == null) throw new ArgumentNullException("predicate");
			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);
			var attributeType = typeof(TAttribute);
			return filter.Include(item => item.GetCustomAttributes(attributeType, inherit).OfType<TAttribute>().Any(attr => predicate(item, attr)));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<TAttribute, bool> predicate, bool inherit = true)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (predicate == null) throw new ArgumentNullException("predicate");
			Func<T, TAttribute, bool> adapter = (item, attr) => predicate(attr);
			return IsDefined(source, adapter, inherit);
		}

		public static IFilterScanner<TypeInfo> IsType(this IScanner<TypeInfo> source, Type type)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (type == null) throw new ArgumentNullException("type");
			var filter = source as IFilterScanner<TypeInfo> ?? new FilterScanner<TypeInfo>(source);
			return filter.Include(type.IsAssignableFrom);
		}

		public static IFilterScanner<TypeInfo> IsType<T>(this IScanner<TypeInfo> source)
		{
			return IsType(source, typeof(T));
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<TIn, TOut> transform)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (transform == null) throw new ArgumentNullException("transform");
			Func<IScanContext, TIn, TOut> adapter = (context, input) => transform(input);
			return new TransformScanner<TIn, TOut>(source, adapter);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<IScanContext, TIn, TOut> transform)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (transform == null) throw new ArgumentNullException("transform");
			return new TransformScanner<TIn, TOut>(source, transform);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<TIn, IEnumerable<TOut>> transform)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (transform == null) throw new ArgumentNullException("transform");
			Func<IScanContext, TIn, IEnumerable<TOut>> adapter = (context, input) => transform(input);
			return new TransformManyScanner<TIn, TOut>(source, adapter);
		}

		public static ITransformScanner<TIn, TOut> Transform<TIn, TOut>(this IScanner<TIn> source, Func<IScanContext, TIn, IEnumerable<TOut>> transform)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (transform == null) throw new ArgumentNullException("transform");
			return new TransformManyScanner<TIn, TOut>(source, transform);
		}

		//

		public static IScanner<AssemblyName> GetAssemblyName(this IScanner<FileInfo> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return source.Transform(Transforms.GetAssemblyName);
		}

		public static IScanner<Assembly> LoadAssembly(this IScanner<AssemblyName> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return source.Transform(Transforms.LoadAssembly);
		}

		public static IScanner<TypeInfo> GetDefinedTypes(this IScanner<Assembly> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return source.Transform(Transforms.AssemblyTypes(asm => asm.DefinedTypes));
		}

		public static IScanner<TypeInfo> GetExportedTypes(this IScanner<Assembly> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return source.Transform(Transforms.AssemblyTypes(asm => asm.ExportedTypes.Select(type => type.GetTypeInfo())));
		}

	}
}