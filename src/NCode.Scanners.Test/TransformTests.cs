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
	public class TransformTests
	{
		[Test]
		public void Self()
		{
			var factory = ScannerFactory.Create();
			var appDomain = AppDomain.CurrentDomain;
			var context = factory.CreateContext();
			var input = factory.AppDomain(appDomain).GetDefinedTypes();

			var before = input.Scan(context);
			var transform = input.Transform(info => info);
			var after = transform.Scan(context);

			CollectionAssert.AreEqual(before, after);
		}

		[Test]
		public void SelfWithContext()
		{
			var factory = ScannerFactory.Create();
			var appDomain = AppDomain.CurrentDomain;
			var context = factory.CreateContext();
			var input = factory.AppDomain(appDomain).GetDefinedTypes();

			var before = input.Scan(context);
			var transform = input.Transform((ctx, info) =>
			{
				Assert.AreSame(context, ctx);
				return info;
			});
			var after = transform.Scan(context);

			CollectionAssert.AreEqual(before, after);
		}

		[Test]
		public void Many()
		{
			var factory = ScannerFactory.Create();
			var appDomain = AppDomain.CurrentDomain;
			var context = factory.CreateContext();
			var input = factory.AppDomain(appDomain);

			var before = input.Scan(context);
			var transform = input.Transform(info => info.DefinedTypes);
			var after = transform.Scan(context);

			var itemsBefore = before.SelectMany(_ => _.DefinedTypes);
			CollectionAssert.AreEqual(itemsBefore, after);
		}

		[Test]
		public void ManyWithContext()
		{
			var factory = ScannerFactory.Create();
			var appDomain = AppDomain.CurrentDomain;
			var context = factory.CreateContext();
			var input = factory.AppDomain(appDomain);

			var before = input.Scan(context);
			var transform = input.Transform((ctx, assembly) =>
			{
				Assert.AreSame(context, ctx);
				return assembly.DefinedTypes;
			});
			var after = transform.Scan(context);

			var itemsBefore = before.SelectMany(_ => _.DefinedTypes);
			CollectionAssert.AreEqual(itemsBefore, after);
		}

	}
}