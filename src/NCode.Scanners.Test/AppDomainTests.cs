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
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class AppDomainTests
	{
		[Test]
		public void CurrentDomain()
		{
			var factory = ScannerFactory.Create();
			var scanner = factory.CurrentDomain();
			var expected = AppDomain.CurrentDomain;
			Assert.AreSame(expected, scanner.AppDomain);
		}

		[Test]
		public void LoadedAssembies()
		{
			var factory = ScannerFactory.Create();

			var appDomain = AppDomain.CurrentDomain;
			var scanner = factory.AppDomain(appDomain);
			Assert.AreSame(appDomain, scanner.AppDomain);

			var context = factory.CreateContext();
			var items = scanner.Scan(context);

			var expected = appDomain.GetAssemblies();
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void EventLoadAssembly()
		{
			var factory = ScannerFactory.Create();

			var appDomain = AppDomain.CurrentDomain;
			var scanner = factory.AppDomain(appDomain);
			var context = factory.CreateContext();

			const string assemblyName = "System.ComponentModel.Composition.Registration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

			var items1 = scanner.Scan(context);
			var assemblies1 = appDomain.GetAssemblies();
			var isLoaded1 = assemblies1.Any(asm => asm.FullName.StartsWith(assemblyName));
			Assert.IsFalse(isLoaded1, "Checking if assembly was initially loaded into the AppDomain.");
			CollectionAssert.AreEqual(assemblies1, items1);

			var wasPropertyChanged = false;
			scanner.PropertyChanged += (sender, args) => wasPropertyChanged = true;

			var wasCollectionChanged = false;
			scanner.CollectionChanged += (sender, args) => wasCollectionChanged = true;

			var assembly = appDomain.Load(assemblyName);
			Assert.IsNotNull(assembly, "Checking if the new assembly was successfully loaded.");

			var items2 = scanner.Scan(context);
			var assemblies2 = appDomain.GetAssemblies();
			var isLoaded2 = assemblies2.Any(asm => asm.FullName.StartsWith(assemblyName));
			Assert.IsTrue(isLoaded2, "Checking if the scanner returned the new assembly.");
			CollectionAssert.AreEqual(assemblies2, items2);

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

	}
}