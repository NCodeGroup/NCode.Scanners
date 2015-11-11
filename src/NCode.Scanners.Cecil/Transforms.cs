using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NCode.Scanners.Options;

namespace NCode.Scanners
{
	internal static class Transforms
	{
		public static IEnumerable<AssemblyDefinition> ReadAssembly(IScanContext context, string path)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (path == null) throw new ArgumentNullException(nameof(path));

			try
			{
				var parameters = context.GetReaderParameters();
				var assembly = AssemblyDefinition.ReadAssembly(path, parameters);
				return new[] { assembly };
			}
			catch (BadImageFormatException)
			{
				// skip on error
			}
			catch (DirectoryNotFoundException)
			{
				// skip on error
			}
			catch (FileNotFoundException)
			{
				// skip on error
			}
			catch (FileLoadException)
			{
				// skip on error
			}
			catch (Exception exception)
			{
				var ignore = context.Options.FindAll<ITransformIgnoreException>();
				if (!ignore.Any(_ => _.IgnoreException("ReadAssembly", exception))) throw;
			}

			return Enumerable.Empty<AssemblyDefinition>();
		}

		public static IEnumerable<AssemblyDefinition> ReadAssembly(IScanContext context, FileInfo fileInfo)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

			var path = fileInfo.FullName;
			var assembly = ReadAssembly(context, path);

			return assembly;
		}

		public static IEnumerable<AssemblyDefinition> ReadAssembly(IScanContext context, AssemblyNameReference assemblyName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

			var parameters = context.GetReaderParameters();
			var resolver = parameters.AssemblyResolver ?? new DefaultAssemblyResolver();
			var assembly = resolver.Resolve(assemblyName, parameters);

			return new[] { assembly };
		}

		public static IEnumerable<AssemblyDefinition> ReadAssembly(IScanContext context, AssemblyName assemblyName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

			var path = assemblyName.CodeBase;
			if (!String.IsNullOrEmpty(path) && File.Exists(path))
				return ReadAssembly(context, path);

			var assemblyNameDefinition = assemblyName.GetAssemblyNameDefinition();
			var assembly = ReadAssembly(context, assemblyNameDefinition);

			return assembly;
		}

	}
}