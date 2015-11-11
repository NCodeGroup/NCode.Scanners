# NCode.Scanners
This library provides a fluent API to search, filter, transform, and cache .NET types by probing applications (using private-bin folders), directories, files, and AppDomains, and assemblies.

# NCode.Scanners.Cecil
This library provides additional scanners for 'NCode.Scanners' that uses 'Cecil' to inspect assemblies and types without loading them into the current AppDomain.

# NCode.Scanners.Reflection
This library provides additional scanners for 'NCode.Scanners' that uses 'Reflection' to inspect assemblies and types which causes them to the loaded into the current AppDomain.

## Examples
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

## Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][1].

[1]: https://github.com/NCodeGroup/NCode.Scanners/issues
