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

using System.Collections.Generic;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class ImmutableTests
	{
		[Test]
		public void AsScanner()
		{
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var collection = new HashSet<string> { "a", "b", "c" };
			var scanner = collection.AsScanner(factory);
			var items = scanner.Scan(context);
			var expected = new[] { "a", "b", "c" };
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void FromSingleParam()
		{
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var scanner = factory.Immutable("foo");
			var items = scanner.Scan(context);
			var expected = new[] { "foo" };
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void FromMultipleParams()
		{
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var scanner = factory.Immutable("a", "b");
			var items = scanner.Scan(context);
			var expected = new[] { "a", "b" };
			CollectionAssert.AreEqual(expected, items);
		}

	}
}