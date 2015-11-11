using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NCode.Scanners
{
	public static partial class ScannerFactory
	{
		public static IScanner<FileInfo> FilesInLocalPath(this IScannerFactory factory)
		{
			return FilesInLocalPath(factory, Constants.DefaultSearchPatterns);
		}

		public static IScanner<FileInfo> FilesInLocalPath(this IScannerFactory factory, IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var appDomain = AppDomain.CurrentDomain;
			var directories = GetSearchDirs(appDomain);
			return FilesInDirectory(factory, directories, patterns, option);
		}

		//

		public static IScanner<FileInfo> FilesInDirectory(this IScannerFactory factory, string directory, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var directories = new[] { directory };
			return FilesInDirectory(factory, directories, Constants.DefaultSearchPatterns, option);
		}

		public static IScanner<FileInfo> FilesInDirectory(this IScannerFactory factory, string directory, IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			var directories = new[] { directory };
			return FilesInDirectory(factory, directories, patterns, option);
		}

		public static IScanner<FileInfo> FilesInDirectory(this IScannerFactory factory, IEnumerable<string> directories, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			return FilesInDirectory(factory, directories, Constants.DefaultSearchPatterns, option);
		}

		public static IScanner<FileInfo> FilesInDirectory(this IScannerFactory factory, IEnumerable<string> directories, IEnumerable<string> patterns, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (directories == null) throw new ArgumentNullException(nameof(directories));
			if (patterns == null) throw new ArgumentNullException(nameof(patterns));

			return factory
				.Immutable(directories)
				.Transform((context, directory) => patterns.SelectMany(pattern => Transforms.EnumerateFiles(context, directory, pattern, option)));
		}

		//

		private static IEnumerable<string> GetSearchDirs(AppDomain appDomain)
		{
			if (appDomain == null)
				throw new ArgumentNullException(nameof(appDomain));

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