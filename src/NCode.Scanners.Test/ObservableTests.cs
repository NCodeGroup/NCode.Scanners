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
			var factory = ScannerFactory.Create();
			var collection = new ObservableCollection<string>();
			var scanner = factory.Observable(collection);

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
			var factory = ScannerFactory.Create();
			var collection = new ObservableCollection<string> { "foo" };
			var scanner = factory.Observable(collection);

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
			var factory = ScannerFactory.Create();
			var collection = new ObservableCollection<string> { "foo" };
			var scanner = factory.Observable(collection);

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