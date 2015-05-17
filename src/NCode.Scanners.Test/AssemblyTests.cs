using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class AssemblyTests
	{
		public virtual void Dump(Assembly[] collection)
		{
			foreach (var assembly in collection)
			{
				Console.WriteLine(assembly);
			}
		}

		[Test]
		public void LoadedAssemblyDefinedTypes()
		{
			var loaded = AppDomain.CurrentDomain.GetAssemblies();
			Dump(loaded);

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(loaded)
				.GetDefinedTypes()
				.Scan(context)
				.ToArray();
			var expected = loaded.SelectMany(asm => asm.DefinedTypes);
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void LoadedAssemblyExportedTypes()
		{
			var loaded = AppDomain
				.CurrentDomain
				.GetAssemblies()
				// System.NotSupportedException : The invoked member is not supported in a dynamic assembly.
				.Where(asm => !asm.IsDynamic)
				.ToArray();
			Dump(loaded);

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(loaded)
				.GetExportedTypes()
				.Scan(context)
				.ToArray();
			var expected = loaded.SelectMany(asm => asm.GetExportedTypes());
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void ExecutingAssemblyDefinedTypes()
		{
			var assemby = Assembly.GetExecutingAssembly();

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(assemby)
				.GetDefinedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assemby.DefinedTypes, items);
		}

		[Test]
		public void ExecutingAssemblyExportedTypes()
		{
			var assemby = Assembly.GetExecutingAssembly();

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(assemby)
				.GetExportedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assemby.GetExportedTypes(), items);
		}

		[Test]
		public void ExecutingAssemblyByNameDefinedTypes()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyName = assembly.GetName();

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(assemblyName)
				.LoadAssembly()
				.GetDefinedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assembly.DefinedTypes, items);
		}

		[Test]
		public void ExecutingAssemblyByNameExportedTypes()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyName = assembly.GetName();

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.Immutable(assemblyName)
				.LoadAssembly()
				.GetExportedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assembly.GetExportedTypes(), items);
		}

	}
}