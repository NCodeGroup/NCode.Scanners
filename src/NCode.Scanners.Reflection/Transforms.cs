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
using NCode.Scanners.Options;

namespace NCode.Scanners
{
	internal static class Transforms
	{
		public static IEnumerable<AssemblyName> GetAssemblyName(IScanContext context, FileInfo fileInfo)
		{
			if (fileInfo == null || !fileInfo.Exists)
				return Enumerable.Empty<AssemblyName>();

			try
			{
				var path = fileInfo.FullName;
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
			catch (Exception exception)
			{
				var ignore = context.Options.FindAll<ITransformIgnoreException>();
				if (!ignore.Any(_ => _.IgnoreException("GetAssemblyName", exception))) throw;
			}

			return Enumerable.Empty<AssemblyName>();
		}

		public static IEnumerable<Assembly> LoadAssembly(IScanContext context, AssemblyName assemblyName)
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
			catch (Exception exception)
			{
				var ignore = context.Options.FindAll<ITransformIgnoreException>();
				if (!ignore.Any(_ => _.IgnoreException("LoadAssembly", exception))) throw;
			}

			return Enumerable.Empty<Assembly>();
		}

	}
}