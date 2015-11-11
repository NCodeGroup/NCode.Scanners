using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace NCode.Scanners
{
	public static class ScannerExtensions
	{
		#region AssemblyDefinition

		public static IScanner<AssemblyDefinition> ReadAssembly(this IScanner<FileInfo> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(Transforms.ReadAssembly);
		}

		public static IScanner<AssemblyDefinition> ReadAssembly(this IScanner<AssemblyNameReference> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(Transforms.ReadAssembly);
		}

		public static IScanner<AssemblyDefinition> ReadAssembly(this IScanner<AssemblyName> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(Transforms.ReadAssembly);
		}

		#endregion

		#region TypeDefinition

		public static IScanner<TypeDefinition> GetDefinedTypes(this IScanner<AssemblyDefinition> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(asm =>
				asm.Modules.SelectMany(mod =>
					mod.GetTypes()));
		}

		public static IScanner<TypeDefinition> GetExportedTypes(this IScanner<AssemblyDefinition> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Transform(asm =>
				asm.Modules.SelectMany(mod =>
					mod.ExportedTypes.Select(type =>
						type.Resolve())));
		}

		#endregion

		#region IsDefined

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			var attributeType = typeof(TAttribute);
			return IsDefined(source, attributeType.FullName);
		}

		public static IFilterScanner<T> IsDefined<T>(this IScanner<T> source, TypeReference attributeType)
		where T : ICustomAttributeProvider
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));

			return IsDefined(source, attributeType.FullName);
		}

		public static IFilterScanner<T> IsDefined<T>(this IScanner<T> source, string attributeTypeName)
			where T : ICustomAttributeProvider
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (attributeTypeName == null) throw new ArgumentNullException(nameof(attributeTypeName));

			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);

			return filter.Include(item => item
				.CustomAttributes
				.Any(attr => attr.AttributeType.FullName == attributeTypeName));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<IScanContext, T, TAttribute, bool> predicate)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			var attributeType = typeof(TAttribute);
			var filter = source as IFilterScanner<T> ?? new FilterScanner<T>(source);

			return filter.Include((context, item) => item
				.CustomAttributes
				// NOTE: cannot compare using 'MetadataToken' because the attribute uses RefType but we have DefType
				.Where(attr => attr.AttributeType.FullName == attributeType.FullName)
				.Select(attr => attr.ResolveAttribute<TAttribute>())
				.Any(attr => predicate(context, item, attr)));
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<T, TAttribute, bool> predicate)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, TAttribute, bool> adapter = (context, item, attr) => predicate(item, attr);

			return IsDefined(source, adapter);
		}

		public static IFilterScanner<T> IsDefined<T, TAttribute>(this IScanner<T> source, Func<TAttribute, bool> predicate)
			where T : ICustomAttributeProvider
			where TAttribute : Attribute
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			Func<IScanContext, T, TAttribute, bool> adapter = (context, item, attr) => predicate(attr);

			return IsDefined(source, adapter);
		}

		#endregion

		#region IsAssignableFrom

		public static IFilterScanner<TypeDefinition> IsAssignableFrom(this IScanner<TypeDefinition> source, TypeDefinition type)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (type == null) throw new ArgumentNullException(nameof(type));

			var filter = source as IFilterScanner<TypeDefinition> ?? new FilterScanner<TypeDefinition>(source);

			return filter.Include(type.IsAssignableFrom);
		}

		public static IFilterScanner<TypeDefinition> IsAssignableFrom<T>(this IScanner<TypeDefinition> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			var type = typeof(T);
			var filter = source as IFilterScanner<TypeDefinition> ?? new FilterScanner<TypeDefinition>(source);

			return filter.Include(type.IsAssignableFrom);
		}

		#endregion

	}
}