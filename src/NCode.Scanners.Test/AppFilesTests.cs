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

using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class AppFilesTests
	{
		[Test]
		public void ExecutingAssembly()
		{
			var factory = ScannerFactory.Create();

			var context = factory.CreateContext();
			var scanner = factory.FilesInLocalPath();
			var files = scanner.Scan(context).ToArray();
			CollectionAssert.IsNotEmpty(files);

			var self = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			var containsSelf = files.Any(file => file.Name == self);
			Assert.IsTrue(containsSelf, "Checking if our assembly file exists in the collection.");
		}

		[Test]
		public void ExecutingAssemblyByFileDefinedTypes()
		{
			var factory = ScannerFactory.Create();

			var assemby = Assembly.GetExecutingAssembly();
			var fileName = Path.GetFileName(assemby.Location);

			var context = factory.CreateContext();
			var items = factory.FilesInLocalPath()
				.Include(file => file.Name == fileName)
				.GetAssemblyName()
				.LoadAssembly()
				.GetDefinedTypes()
				.Scan(context)
				.ToArray();
			CollectionAssert.AreEqual(assemby.DefinedTypes, items);
		}

	}
}