using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NCode.Scanners.Examples
{
	internal static class Program
	{
		private static void Main()
		{
			SimpleExample();
			SystemFilesFromGac();
		}

		private static void SimpleExample()
		{
			var items = ScannerFactory
				.AppFiles()           // returns IScanner<FileInfo>
				.GetAssemblyName()    // returns IScanner<AssemblyName>
				.LoadAssembly()       // returns IScanner<Assembly>
				.GetDefinedTypes()    // returns IScanner<TypeInfo>
				.Scan(ScannerFactory.CreateContext());

			// ...
			DisplayItems(items);
		}

		private static void SystemFilesFromGac()
		{
			var items = ScannerFactory
				.Directory(@"C:\Windows\Microsoft.NET\assembly\GAC_MSIL", SearchOption.AllDirectories)
				.Include(file => file.Name.StartsWith("System."))
				.Scan(ScannerFactory.CreateContext());

			// ...
			DisplayItems(items);
		}

		private static void DisplayItems(IEnumerable<object> items)
		{
			var counter = 0;
			foreach (var item in items)
			{
				Console.WriteLine("[{0}] {1}", ++counter, item);
			}
		}

	}
}