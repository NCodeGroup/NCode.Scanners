using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class AggregateTests
	{
		[Test]
		public void FactoryParamsEmpty()
		{
			var aggregate = ScannerFactory.Aggregate<IScanner<string>>();
			Assert.IsNotNull(aggregate);
			CollectionAssert.IsEmpty(aggregate.Scanners);
		}

		[Test]
		public void FactoryParamsSingle()
		{
			var source1 = ScannerFactory.Immutable("a");
			var aggregate = ScannerFactory.Aggregate(source1);
			Assert.IsNotNull(aggregate);
			CollectionAssert.Contains(aggregate.Scanners, source1);
		}

		[Test]
		public void FactoryParamsMultiple()
		{
			var source1 = ScannerFactory.Immutable("a");
			var source2 = ScannerFactory.Immutable("b");
			var aggregate = ScannerFactory.Aggregate(source1, source2);
			Assert.IsNotNull(aggregate);
			CollectionAssert.Contains(aggregate.Scanners, source1);
			CollectionAssert.Contains(aggregate.Scanners, source2);
		}

		[Test]
		public void AggregateAdd()
		{
			var aggregate = ScannerFactory.Aggregate<string>();
			Assert.IsNotNull(aggregate);
			CollectionAssert.IsEmpty(aggregate.Scanners);

			var scannerToAdd = ScannerFactory.Immutable("foo");
			aggregate.Scanners.Add(scannerToAdd);
			Assert.AreEqual(1, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingSingleParam()
		{
			var scannerToAdd = ScannerFactory.Immutable(Assembly.GetExecutingAssembly());
			var sourceScanner = ScannerFactory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannerToAdd);
			Assert.AreEqual(2, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingMultipleParams()
		{
			var scannerToAdd1 = ScannerFactory.Immutable(Assembly.GetExecutingAssembly());
			var scannerToAdd2 = ScannerFactory.Immutable(Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly());
			var sourceScanner = ScannerFactory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannerToAdd1, scannerToAdd2);
			Assert.AreEqual(3, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingEnumerable()
		{
			var scannerToAdd1 = ScannerFactory.Immutable(Assembly.GetExecutingAssembly());
			var scannerToAdd2 = ScannerFactory.Immutable(Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly());
			var scannersToAdd = new HashSet<IScanner<Assembly>> { scannerToAdd1, scannerToAdd2 };
			var sourceScanner = ScannerFactory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannersToAdd);
			Assert.AreEqual(3, aggregate.Scanners.Count);
		}

		[Test]
		public void EventAdd()
		{
			var aggregate = ScannerFactory.Aggregate<string>();

			var wasPropertyChanged = false;
			aggregate.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Scanners", args.PropertyName);
			};

			var wasCollectionChanged = false;
			aggregate.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			var scannerToAdd = ScannerFactory.Immutable("foo");
			aggregate.Scanners.Add(scannerToAdd);

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventAddNested()
		{
			var aggregateInner = ScannerFactory.Aggregate<string>();
			var aggregateOuter = ScannerFactory.Aggregate(new[] { aggregateInner });

			var wasPropertyChanged = false;
			aggregateOuter.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Scanners", args.PropertyName);
			};

			var wasCollectionChanged = false;
			aggregateOuter.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			var scannerToAdd = ScannerFactory.Immutable("foo");
			aggregateInner.Scanners.Add(scannerToAdd);

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventRemove()
		{
			var aggregateInner = ScannerFactory.Aggregate<string>();
			var aggregateOuter = ScannerFactory.Aggregate(new[] { aggregateInner });

			var wasPropertyChanged = false;
			aggregateOuter.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Scanners", args.PropertyName);
			};

			var wasCollectionChanged = false;
			aggregateOuter.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			var wasRemoved = aggregateOuter.Scanners.Remove(aggregateInner);
			Assert.IsTrue(wasRemoved, "Checking if the item was removed.");

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventClear()
		{
			var aggregateInner = ScannerFactory.Aggregate<string>();
			var aggregateOuter = ScannerFactory.Aggregate(new[] { aggregateInner });

			var wasPropertyChanged = false;
			aggregateOuter.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Scanners", args.PropertyName);
			};

			var wasCollectionChanged = false;
			aggregateOuter.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			aggregateOuter.Scanners.Clear();

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

	}
}