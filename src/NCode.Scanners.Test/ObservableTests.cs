using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class ObservableTests
	{
		[Test]
		public void EventAdd()
		{
			var collection = new ObservableCollection<string>();
			var scanner = ScannerFactory.Observable(collection);

			var wasPropertyChanged = false;
			scanner.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Items", args.PropertyName);
			};

			var wasCollectionChanged = false;
			scanner.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			collection.Add("foo");

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventRemove()
		{
			var collection = new ObservableCollection<string> { "foo" };
			var scanner = ScannerFactory.Observable(collection);

			var wasPropertyChanged = false;
			scanner.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Items", args.PropertyName);
			};

			var wasCollectionChanged = false;
			scanner.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			var wasRemoved = collection.Remove("foo");
			Assert.IsTrue(wasRemoved, "Checking if the item was removed.");

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventClear()
		{
			var collection = new ObservableCollection<string> { "foo" };
			var scanner = ScannerFactory.Observable(collection);

			var wasPropertyChanged = false;
			scanner.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual("Items", args.PropertyName);
			};

			var wasCollectionChanged = false;
			scanner.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
			};

			collection.Clear();

			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

	}
}