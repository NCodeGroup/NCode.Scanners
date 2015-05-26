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
using System.Security;

namespace NCode.Scanners
{
	public static class Transforms
	{
		public static Func<Assembly, IEnumerable<TypeInfo>> AssemblyTypes(Func<Assembly, IEnumerable<TypeInfo>> getter)
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

		public static Func<string, IEnumerable<FileInfo>> DirectoryFiles(IEnumerable<string> patterns, SearchOption option)
		{
			if (patterns == null) throw new ArgumentNullException("patterns");
			return directory =>
			{
				return patterns.SelectMany(pattern => EnumerateFiles(directory, pattern, option));
			};
		}

		public static IEnumerable<FileInfo> EnumerateFiles(string directory, string pattern, SearchOption option)
		{
			if (String.IsNullOrEmpty(directory))
				return Enumerable.Empty<FileInfo>();

			if (String.IsNullOrEmpty(pattern))
				return Enumerable.Empty<FileInfo>();

			try
			{
				var info = new DirectoryInfo(directory);
				return !info.Exists
					? Enumerable.Empty<FileInfo>()
					: info.EnumerateFiles(pattern, option);
			}
			catch (UnauthorizedAccessException)
			{
				// skip on error
			}
			catch (DirectoryNotFoundException)
			{
				// skip on error
			}
			catch (SecurityException)
			{
				// skip on error
			}
			catch (IOException)
			{
				// skip on error
			}
			return Enumerable.Empty<FileInfo>();
		}

		public static IEnumerable<AssemblyName> GetAssemblyName(FileInfo fileInfo)
		{
			if (fileInfo == null || !fileInfo.Exists)
				return Enumerable.Empty<AssemblyName>();

			var path = fileInfo.FullName;
			try
			{
				var assemblyName = AssemblyName.GetAssemblyName(path);
				return new[] { assemblyName };
			}
			catch (BadImageFormatException)
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
			catch (ArgumentException)
			{
				var assemblyName = new AssemblyName
				{
					CodeBase = path
				};
				return new[] { assemblyName };
			}

			return Enumerable.Empty<AssemblyName>();
		}

		public static IEnumerable<Assembly> LoadAssembly(AssemblyName assemblyName)
		{
			if (assemblyName == null)
				return Enumerable.Empty<Assembly>();

			try
			{
				var assembly = Assembly.Load(assemblyName);
				return new[] { assembly };
			}
			catch (BadImageFormatException)
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

			return Enumerable.Empty<Assembly>();
		}

		public static IEnumerable<string> GetSearchPaths(AppDomain appDomain)
		{
			if (appDomain == null)
				throw new ArgumentNullException("appDomain");

			var baseDirectory = appDomain.BaseDirectory;
			var relativeSearchPath = appDomain.RelativeSearchPath;

			if (String.IsNullOrEmpty(relativeSearchPath))
				return new[] { baseDirectory };

			var relativeSearchPaths = relativeSearchPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var absoluteSearchPaths = relativeSearchPaths.Select(dir => Path.Combine(baseDirectory, dir));

			return absoluteSearchPaths;
		}

	}
}