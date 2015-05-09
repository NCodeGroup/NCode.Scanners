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
			var context = ScannerFactory.CreateContext();

			var input = ScannerFactory.Immutable("a", "b", "c");
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
			var context = ScannerFactory.CreateContext();

			var parentMock = new Mock<IScanner<string>>();
			parentMock.Setup(_ => _.Scan(context)).Returns((IEnumerable<string>)null);
			var parent = parentMock.Object;

			var useParentMock = new Mock<UseParentScanner<string>>(parent);
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