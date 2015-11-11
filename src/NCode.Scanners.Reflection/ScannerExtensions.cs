using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NCode.Scanners
{
	public static partial class ScannerExtensions
	{
		#region IsDefined

		//
		// NOTE: the 'inherit' argument cannot be removed nor can it be given a
		// default value otherwise the 'IsDefined' overloads would be ambigous
		// with the Cecil variations.
		//

		public static IFilterScanner<T> IsDefined<T>(this IScanner<T> source, Type attributeType, bool inherit)
			where T : ICustomAttributeProvider
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));

			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);

			return filter.Include(item => item
				.IsDefined(attributeType, inherit));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<IScanContext, T, TAttribute, bool> predicate, bool inherit)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);
			var attributeType = typeof(TAttribute);

			return filter.Include((context, item) => item
				.GetCustomAttributes(attributeType, inherit)
				.OfType<TAttribute>()
				.Any(attr => predicate(context, item, attr)));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<T, TAttribute, bool> predicate, bool inherit)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, TAttribute, bool> adapter = (context, item, attr) => predicate(item, attr);

			return IsDefined(source, adapter, inherit);
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<TAttribute, bool> predicate, bool inherit)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, TAttribute, bool> adapter = (context, item, attr) => predicate(attr);

			return IsDefined(source, adapter, inherit);
		}

		#endregion

		#region IsAssignableFrom

		public static IFilterScanner<TypeInfo> IsAssignableFrom(this IScanner<TypeInfo> source, Type type)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (type == null) throw new ArgumentNullException(nameof(type));

			var filter = source as IFilterScanner<TypeInfo> ?? new FilterScanner<TypeInfo>(source);

			return filter.Include(type.IsAssignableFrom);
		}

		public static IFilterScanner<TypeInfo> IsAssignableFrom<T>(this IScanner<TypeInfo> source)
		{
			return IsAssignableFrom(source, typeof(T));
		}

		#endregion

		#region Reflect

		public static IReflectionScanner<Assembly> Reflect(this IScanner<Assembly> source, ReflectionContext context)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (context == null) throw new ArgumentNullException(nameof(context));

			return new ReflectionAssemblyScanner(source, context);
		}

		public static IReflectionScanner<TypeInfo> Reflect(this IScanner<TypeInfo> source, ReflectionContext context)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (context == null) throw new ArgumentNullException(nameof(context));

			return new ReflectionTypeInfoScanner(source, context);
		}

		#endregion

		#region Transform

		public static IScanner<AssemblyName> GetAssemblyName(this IScanner<FileInfo> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(Transforms.GetAssemblyName);
		}

		public static IScanner<Assembly> LoadAssembly(this IScanner<AssemblyName> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(Transforms.LoadAssembly);
		}

		public static IScanner<TypeInfo> GetDefinedTypes(this IScanner<Assembly> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(SafeGetTypes(asm => asm.DefinedTypes));
		}

		public static IScanner<TypeInfo> GetExportedTypes(this IScanner<Assembly> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(SafeGetTypes(asm => asm.ExportedTypes.Select(type => type.GetTypeInfo())));
		}

		private static Func<Assembly, IEnumerable<TypeInfo>> SafeGetTypes(Func<Assembly, IEnumerable<TypeInfo>> getter)
		{
			return assembly =>
			{
				IEnumerable<TypeInfo> types;
				try
				{
					types = getter(assembly);
				}
				catch (NotSupportedException)
				{
					// System.NotSupportedException : The invoked member is not supported in a dynamic assembly.
					if (!assembly.IsDynamic) throw;
					types = Enumerable.Empty<TypeInfo>();
				}
				catch (ReflectionTypeLoadException exception)
				{
					types = exception.Types.Where(type => type != null).Select(type => type.GetTypeInfo());
				}
				return types;
			};
		}

		#endregion

	}
}