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

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(loaded)
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

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(loaded)
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

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(assemby)
				.GetDefinedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assemby.DefinedTypes, items);
		}

		[Test]
		public void ExecutingAssemblyExportedTypes()
		{
			var assemby = Assembly.GetExecutingAssembly();

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(assemby)
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

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(assemblyName)
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

			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var items = factory.Immutable(assemblyName)
				.LoadAssembly()
				.GetExportedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assembly.GetExportedTypes(), items);
		}

	}
}