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
using System.Security;
using NCode.Scanners.Options;

namespace NCode.Scanners
{
	internal static class Transforms
	{
		public static IEnumerable<FileInfo> EnumerateFiles(IScanContext context, string directory, string pattern, SearchOption option)
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
			catch (Exception exception)
			{
				var ignore = context.Options.FindAll<ITransformIgnoreException>();
				if (!ignore.Any(_ => _.IgnoreException("EnumerateFiles", exception))) throw;
			}

			return Enumerable.Empty<FileInfo>();
		}

	}
}