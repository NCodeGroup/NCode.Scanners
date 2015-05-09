using System.Collections.Generic;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class ImmutableTests
	{
		[Test]
		public void CollectionAsScanner()
		{
			var context = ScannerFactory.CreateContext();
			var collection = new HashSet<string> { "a", "b", "c" };
			var scanner = collection.AsImmutableScanner();
			var items = scanner.Scan(context);
			var expected = new[] { "a", "b", "c" };
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void FromSingleParam()
		{
			var context = ScannerFactory.CreateContext();
			var scanner = ScannerFactory.Immutable("foo");
			var items = scanner.Scan(context);
			var expected = new[] { "foo" };
			CollectionAssert.AreEqual(expected, items);
		}

		[Test]
		public void FromMultipleParams()
		{
			var context = ScannerFactory.CreateContext();
			var scanner = ScannerFactory.Immutable("a", "b");
			var items = scanner.Scan(context);
			var expected = new[] { "a", "b" };
			CollectionAssert.AreEqual(expected, items);
		}

	}
}