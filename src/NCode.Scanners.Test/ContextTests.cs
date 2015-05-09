using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class ContextTests
	{
		[Test]
		public void CreateContextIsNotNull()
		{
			var context = ScannerFactory.CreateContext();
			Assert.IsNotNull(context);
		}

		[Test]
		public void CreateContextTwiceIsDifferent()
		{
			var context1 = ScannerFactory.CreateContext();
			var context2 = ScannerFactory.CreateContext();
			Assert.AreNotSame(context1, context2);
		}

	}
}