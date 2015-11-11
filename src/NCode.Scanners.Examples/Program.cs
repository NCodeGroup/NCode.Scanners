using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NCode.Scanners.Examples
{
	[Guid(MyGuid)]
	internal static class Program
	{
		private const string MyGuid = "E0EB8734-36E6-4AEE-989D-5A7A40D87C72";

		private static void Main()
		{
			UsingCecil();
			UsingReflection();
			SystemFilesFromGac();
		}

		private static void UsingCecil()
		{
			var factory = ScannerFactory.Create();

			var items = factory
				.FilesInLocalPath()   // returns IScanner<FileInfo>
				.ReadAssembly()       // returns IScanner<AssemblyDefinition>
				.GetDefinedTypes()    // returns IScanner<TypeDefiniton>
				.IsDefined((GuidAttribute attr) => attr.Value == MyGuid)
				.Scan();              // returns IEnumerable<TypeDefiniton>

			// ...
			DisplayItems(items);
		}
		private static void UsingReflection()
		{
			var factory = ScannerFactory.Create();

			var items = factory
				.FilesInLocalPath()   // returns IScanner<FileInfo>
				.GetAssemblyName()    // returns IScanner<AssemblyName>
				.LoadAssembly()       // returns IScanner<Assembly>
				.GetDefinedTypes()    // returns IScanner<TypeInfo>
				.IsDefined((GuidAttribute attr) => attr.Value == MyGuid, false)
				.Scan();              // returns IEnumerable<TypeInfo>

			// ...
			DisplayItems(items);
		}

		private static void SystemFilesFromGac()
		{
			var factory = ScannerFactory.Create();

			var dirs = new[] { @"C:\Windows\assembly\GAC_MSIL", @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL" };
			var items = factory
				.FilesInDirectory(dirs, SearchOption.AllDirectories)
				.Include(file => file.Name.StartsWith("System."))
				.Scan();

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