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
using System.ComponentModel;
using Moq;
using NCode.Scanners.Test.Dummy;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class UseParentTests
	{
		[Test]
		public void ParentIsNull()
		{
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();

			var input = factory.Immutable("a", "b", "c");
			var useParentMock = new Mock<UseParentScanner<string>>(input);
			useParentMock.SetupGet(_ => _.Parent).Returns((IScanner<string>)null);
			var useParent = useParentMock.Object;

			var items = useParent.Scan(context);
			Assert.IsNotNull(items);
			CollectionAssert.IsEmpty(items);
		}

		[Test]
		public void ParentScanIsNull()
		{
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();

			var parentMock = new Mock<IScanner<string>>(MockBehavior.Strict);
			parentMock.SetupGet(_ => _.Factory).Returns(factory);
			parentMock.Setup(_ => _.Scan(context)).Returns((IEnumerable<string>)null);
			var parent = parentMock.Object;

			var useParentMock = new Mock<UseParentScanner<string>>(parent) { CallBase = true };
			var useParent = useParentMock.Object;

			var items = useParent.Scan(context);
			Assert.IsNotNull(items);
			CollectionAssert.IsEmpty(items);
		}

		[Test]
		public void EventPropertyChanged()
		{
			var parent = new DummyScanner<string>();
			var useParent = new DummyUseParentScanner<string>(parent);

			var wasPropertyChanged = false;
			useParent.PropertyChanged += (sender, args) =>
			{
				wasPropertyChanged = true;
				Assert.AreEqual(args.PropertyName, "Parent");
			};

			var wasCollectionChanged = false;
			useParent.CollectionChanged += (sender, args) => wasCollectionChanged = true;

			parent.FireOnPropertyChanged(new PropertyChangedEventArgs("Foo"));
			Assert.IsTrue(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsFalse(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

		[Test]
		public void EventCollectionChanged()
		{
			var parent = new DummyScanner<string>();
			var useParent = new DummyUseParentScanner<string>(parent);

			var wasPropertyChanged = false;
			useParent.PropertyChanged += (sender, args) => wasPropertyChanged = true;

			var wasCollectionChanged = false;
			useParent.CollectionChanged += (sender, args) =>
			{
				wasCollectionChanged = true;
				Assert.AreEqual(args.Action, NotifyCollectionChangedAction.Reset);
			};

			parent.FireOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, "Foo"));
			Assert.IsFalse(wasPropertyChanged, "Checking if the PropertyChanged event was raised.");
			Assert.IsTrue(wasCollectionChanged, "Checking if the CollectionChanged event was raised.");
		}

	}
}