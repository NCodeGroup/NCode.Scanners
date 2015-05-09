#region Copyright Preamble

// 
//    Copyright 2015 NCode Group
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
using System.Collections.ObjectModel;
using System.IO;

namespace NCode.Scanners
{
	public static class ScannerFactory
	{
		public static IScanContext CreateContext()
		{
			return new ScanContext();
		}

		#region Generic Scanners

		public static IScanner<T> AsImmutableScanner<T>(this IEnumerable<T> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return new ImmutableScanner<T>(source);
		}

		public static IScanner<T> Immutable<T>(params T[] source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return new ImmutableScanner<T>(source);
		}

		public static IAggregateScanner<T> Aggregate<T>(params IScanner<T>[] source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return new AggregateScanner<T>(source);
		}

		public static IObservableScanner<T, ObservableCollection<T>> Observable<T>(ObservableCollection<T> collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			return new ObservableScanner<T, ObservableCollection<T>>(collection);
		}

		public static IObservableScanner<T, ReadOnlyObservableCollection<T>> Observable<T>(ReadOnlyObservableCollection<T> collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			return new ObservableScanner<T, ReadOnlyObservableCollection<T>>(collection);
		}

		#endregion

		#region Assembly Scanners

		public static IAppDomainScanner CurrentDomain()
		{
			return AppDomain(System.AppDomain.CurrentDomain);
		}

		public static IAppDomainScanner AppDomain(AppDomain appDomain)
		{
			if (appDomain == null) throw new ArgumentNullException("appDomain");
			return new AppDomainScanner(appDomain);
		}

		#endregion

		#region FileInfo Scanners

		public static IScanner<FileInfo> AppFiles()
		{
			return AppFiles(Constants.DefaultSearchPatterns);
		}

		public static IScanner<FileInfo> AppFiles(IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var appDomain = System.AppDomain.CurrentDomain;
			return Immutable(appDomain)
				.Transform(Transforms.GetSearchPaths)
				.Transform(Transforms.DirectoryFiles(patterns, option));
		}

		public static IScanner<FileInfo> Directory(string directory, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var directories = new[] { directory };
			return Directories(directories, Constants.DefaultSearchPatterns, option);
		}

		public static IScanner<FileInfo> Directory(string directory, IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var directories = new[] { directory };
			return Directories(directories, patterns, option);
		}

		public static IScanner<FileInfo> Directories(IEnumerable<string> directories, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return Directories(directories, Constants.DefaultSearchPatterns, option);
		}

		public static IScanner<FileInfo> Directories(IEnumerable<string> directories, IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return directories.AsImmutableScanner()
				.Transform(Transforms.DirectoryFiles(patterns, option));
		}

		#endregion

	}
}