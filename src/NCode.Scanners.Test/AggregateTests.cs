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
			var factory = ScannerFactory.Create();
			var aggregate = factory.Aggregate<IScanner<string>>();
			Assert.IsNotNull(aggregate);
			CollectionAssert.IsEmpty(aggregate.Scanners);
		}

		[Test]
		public void FactoryParamsSingle()
		{
			var factory = ScannerFactory.Create();
			var source1 = factory.Immutable("a");
			var aggregate = factory.Aggregate(source1);
			Assert.IsNotNull(aggregate);
			CollectionAssert.Contains(aggregate.Scanners, source1);
		}

		[Test]
		public void FactoryParamsMultiple()
		{
			var factory = ScannerFactory.Create();
			var source1 = factory.Immutable("a");
			var source2 = factory.Immutable("b");
			var aggregate = factory.Aggregate(source1, source2);
			Assert.IsNotNull(aggregate);
			CollectionAssert.Contains(aggregate.Scanners, source1);
			CollectionAssert.Contains(aggregate.Scanners, source2);
		}

		[Test]
		public void AggregateAdd()
		{
			var factory = ScannerFactory.Create();

			var aggregate = factory.Aggregate<string>();
			Assert.IsNotNull(aggregate);
			CollectionAssert.IsEmpty(aggregate.Scanners);

			var scannerToAdd = factory.Immutable("foo");
			aggregate.Scanners.Add(scannerToAdd);
			Assert.AreEqual(1, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingSingleParam()
		{
			var factory = ScannerFactory.Create();
			var scannerToAdd = factory.Immutable(Assembly.GetExecutingAssembly());
			var sourceScanner = factory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannerToAdd);
			Assert.AreEqual(2, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingMultipleParams()
		{
			var factory = ScannerFactory.Create();
			var scannerToAdd1 = factory.Immutable(Assembly.GetExecutingAssembly());
			var scannerToAdd2 = factory.Immutable(Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly());
			var sourceScanner = factory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannerToAdd1, scannerToAdd2);
			Assert.AreEqual(3, aggregate.Scanners.Count);
		}

		[Test]
		public void ExtensionUsingEnumerable()
		{
			var factory = ScannerFactory.Create();
			var scannerToAdd1 = factory.Immutable(Assembly.GetExecutingAssembly());
			var scannerToAdd2 = factory.Immutable(Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly());
			var scannersToAdd = new HashSet<IScanner<Assembly>> { scannerToAdd1, scannerToAdd2 };
			var sourceScanner = factory.CurrentDomain();
			var aggregate = sourceScanner.Aggregate(scannersToAdd);
			Assert.AreEqual(3, aggregate.Scanners.Count);
		}

		[Test]
		public void EventAdd()
		{
			var factory = ScannerFactory.Create();
			var aggregate = factory.Aggregate<string>();

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

			var scannerToAdd = factory.Immutable("foo");
			aggregate.Scanners.Add(scannerToAdd);

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventAddNested()
		{
			var factory = ScannerFactory.Create();
			var aggregateInner = factory.Aggregate<string>();
			var aggregateOuter = factory.Aggregate(aggregateInner);

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

			var scannerToAdd = factory.Immutable("foo");
			aggregateInner.Scanners.Add(scannerToAdd);

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventRemove()
		{
			var factory = ScannerFactory.Create();
			var aggregateInner = factory.Aggregate<string>();
			var aggregateOuter = factory.Aggregate(aggregateInner);

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
			var factory = ScannerFactory.Create();
			var aggregateInner = factory.Aggregate<string>();
			var aggregateOuter = factory.Aggregate(aggregateInner);

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