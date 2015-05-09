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
			var context = ScannerFactory.CreateContext();
			var scanner = ScannerFactory.AppFiles();
			var files = scanner.Scan(context).ToArray();
			CollectionAssert.IsNotEmpty(files);

			var self = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			var containsSelf = files.Any(file => file.Name == self);
			Assert.IsTrue(containsSelf, "Checking if our assembly file exists in the collection.");
		}

		[Test]
		public void ExecutingAssemblyByFileDefinedTypes()
		{
			var assemby = Assembly.GetExecutingAssembly();
			var fileName = Path.GetFileName(assemby.Location);

			var context = ScannerFactory.CreateContext();
			var items = ScannerFactory.AppFiles()
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