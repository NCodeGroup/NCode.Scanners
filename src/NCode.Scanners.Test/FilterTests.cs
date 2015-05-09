using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class FilterTests
	{
		[Test]
		public void SingleInclude()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			var filter = input
				.Include(type => type.IsInterface);

			var context = ScannerFactory.CreateContext();
			var items = filter.Scan(context).ToArray();

			var contains1 = items.Any(type => type.IsInterface);
			var contains2 = items.Any(type => type.IsClass);
			Assert.IsTrue(contains1, "Checking the include logic.");
			Assert.IsFalse(contains2, "Checking the include logic didn't include anything else.");
		}

		[Test]
		public void MultipleInclude()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			var filter = input
				.Include(type => type.IsInterface)
				.Include(type => type.IsClass);

			var context = ScannerFactory.CreateContext();
			var items = filter.Scan(context).ToArray();

			var contains1 = items.Any(type => type.IsInterface);
			var contains2 = items.Any(type => type.IsClass);
			Assert.IsTrue(contains1, "Checking the first include criteria.");
			Assert.IsTrue(contains2, "Checking the second include criteria.");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "At least one include must exist before an exclude can be added.")]
		public void ExcludeFailsWithNoInclude()
		{
			var scanner = ScannerFactory.CurrentDomain();
			scanner.Exclude(asm => asm.IsDynamic);

			var context = ScannerFactory.CreateContext();
			scanner.Scan(context);
		}

		[Test]
		public void IncludeThenExclude()
		{
			var context = ScannerFactory.CreateContext();
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			var before = input
				.Include(type => type.IsInterface)
				.Include(type => type.IsClass);
			var itemsBefore = before.Scan(context).ToArray();

			//

			var beforeContainsInterface = itemsBefore.Any(type => type.IsInterface);
			Assert.IsTrue(beforeContainsInterface, "Checking beforeContainsInterface");

			var beforeContainsClass = itemsBefore.Any(type => type.IsClass);
			Assert.IsTrue(beforeContainsClass, "Checking beforeContainsClass");

			var beforeAllPublic = itemsBefore.All(type => type.IsPublic);
			Assert.IsFalse(beforeAllPublic, "Checking beforeAllPublic");

			var after = before.Exclude(type => !type.IsPublic);
			var itemsAfter = after.Scan(context).ToArray();

			//

			var afterContainsInterface = itemsAfter.Any(type => type.IsInterface);
			Assert.IsTrue(afterContainsInterface, "Checking afterContainsInterface");

			var afterContainsClass = itemsAfter.Any(type => type.IsClass);
			Assert.IsTrue(afterContainsClass, "Checking afterContainsClass");

			var afterAllPublic = itemsAfter.All(type => type.IsPublic);
			Assert.IsTrue(afterAllPublic, "Checking afterAllPublic");
		}

		[Test]
		public void IsDefined()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain);

			var filter = input.IsDefined(typeof(GuidAttribute));
			var context = ScannerFactory.CreateContext();
			var items = filter.Scan(context).ToArray();

			CollectionAssert.IsNotEmpty(items);
		}

		[Test]
		public void IsDefinedSpecific()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain);

			var assembly = Assembly.GetExecutingAssembly();
			var guid = Marshal.GetTypeLibGuidForAssembly(assembly);

			var filter = input.IsDefined((GuidAttribute attr) => Guid.Parse(attr.Value) == guid);
			var context = ScannerFactory.CreateContext();
			var item = filter.Scan(context).Single();

			Assert.AreSame(assembly, item);
		}

		[Test]
		public void IsType()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			var filter = input.IsType<IDisposable>();

			var context = ScannerFactory.CreateContext();
			var items = filter.Scan(context).ToArray();

			var all = items.All(type => typeof(IDisposable).IsAssignableFrom(type));
			Assert.IsTrue(all, "Checking if all items are disposable");
		}

		[Test]
		public void EventInclude()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			IFilterScanner<TypeInfo> filter = new FilterScanner<TypeInfo>(input);

			var wasPropertyChanged = false;
			filter.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Include", args.PropertyName);
			};

			var wasCollectionChanged = false;
			filter.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			filter.Include(type => type.IsInterface);
			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventExclude()
		{
			var appDomain = AppDomain.CurrentDomain;
			var input = ScannerFactory.AppDomain(appDomain).GetDefinedTypes();

			IFilterScanner<TypeInfo> filter = new FilterScanner<TypeInfo>(input);

			// before we can exclude, we must include something first
			filter.Include(type => type.IsClass);

			var wasPropertyChanged = false;
			filter.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Exclude", args.PropertyName);
			};

			var wasCollectionChanged = false;
			filter.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			filter.Exclude(type => !type.IsPublic);
			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

	}
}