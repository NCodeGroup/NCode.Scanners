using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class DistinctTests
	{
		[Test]
		public void DuplicateNumbers()
		{
			var duplicate = new[] { 1, 1, 2, 3, 4, 4, 4, 6 };
			var expected = new[] { 1, 2, 3, 4, 6 };

			var context = ScannerFactory.CreateContext();
			var input = ScannerFactory.Immutable(duplicate);
			var scanner = input.Distinct();
			var output = scanner.Scan(context);

			CollectionAssert.AreEqual(expected, output);
		}

	}
}