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
			var appDomain = AppDomain.CurrentDomain;
			var context = ScannerFactory.CreateContext();
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			var before = input.Scan(context);
			var transform = input.Transform(info => info);
			var after = transform.Scan(context);

			CollectionAssert.AreEqual(before, after);
		}

		[Test]
		public void SelfWithContext()
		{
			var appDomain = AppDomain.CurrentDomain;
			var context = ScannerFactory.CreateContext();
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

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
			var appDomain = AppDomain.CurrentDomain;
			var context = ScannerFactory.CreateContext();
			var input = ScannerFactory.AppDomain(appDomain);

			var before = input.Scan(context);
			var transform = input.Transform(info => info.DefinedTypes);
			var after = transform.Scan(context);

			var itemsBefore = before.SelectMany(_ => _.DefinedTypes);
			CollectionAssert.AreEqual(itemsBefore, after);
		}

		[Test]
		public void ManyWithContext()
		{
			var appDomain = AppDomain.CurrentDomain;
			var context = ScannerFactory.CreateContext();
			var input = ScannerFactory.AppDomain(appDomain);

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