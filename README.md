# NCode.Scanners
This library provides a fluent API to search, filter, transform, and cache .NET types by probing applications (using private-bin folders), directories, files, and AppDomains, and assemblies.

## Examples
	private static void SimpleExample()
	{
		var items = ScannerFactory
			.AppFiles()           // returns IScanner<FileInfo>
			.GetAssemblyName()    // returns IScanner<AssemblyName>
			.LoadAssembly()       // returns IScanner<Assembly>
			.GetDefinedTypes()    // returns IScanner<TypeInfo>
			.Scan(ScannerFactory.CreateContext())
			.ToArray();

		// ...
		DisplayItems(items);
	}

	private static void SystemFilesFromGac()
	{
		var items = ScannerFactory
			.Directory(@"C:\Windows\Microsoft.NET\assembly\GAC_MSIL", SearchOption.AllDirectories)
			.Include(file => file.Name.StartsWith("System."))
			.Scan(ScannerFactory.CreateContext())
			.ToArray();

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

## Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][1].

[1]: https://github.com/NCodeGroup/NCode.Scanners/issues
