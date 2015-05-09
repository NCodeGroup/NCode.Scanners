﻿using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class CacheTests
	{
		[Test]
		public void ScanCacheOnce()
		{
			var context = ScannerFactory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Loose);
			innerMock.Setup(_ => _.Scan(context)).Returns(expected);
			var inner = innerMock.Object;

			var cache = inner.Cache();
			var items1 = cache.Scan(context);
			var items2 = cache.Scan(context);
			CollectionAssert.AreEqual(expected, items1);
			CollectionAssert.AreEqual(expected, items2);

			innerMock.Verify(_ => _.Scan(context), Times.Once);
		}

		[Test]
		public void ScanAgainAfterInvalidate()
		{
			var context = ScannerFactory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Loose);
			innerMock.Setup(_ => _.Scan(context)).Returns(expected);
			var inner = innerMock.Object;

			var cache = inner.Cache();
			var items1 = cache.Scan(context);
			var items2 = cache.Scan(context);
			CollectionAssert.AreEqual(expected, items1);
			CollectionAssert.AreEqual(expected, items2);

			innerMock.Verify(_ => _.Scan(context), Times.Once);
			cache.Invalidate();

			var items3 = cache.Scan(context);
			var items4 = cache.Scan(context);
			CollectionAssert.AreEqual(expected, items3);
			CollectionAssert.AreEqual(expected, items4);

			innerMock.Verify(_ => _.Scan(context), Times.Exactly(2));
		}

		[Test]
		public void ScanAgainAfterEvent()
		{
			var context = ScannerFactory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Loose);
			innerMock.Setup(_ => _.Scan(context)).Returns(expected);
			var inner = innerMock.Object;

			var cache = inner.Cache();
			var items1 = cache.Scan(context);
			var items2 = cache.Scan(context);
			CollectionAssert.AreEqual(expected, items1);
			CollectionAssert.AreEqual(expected, items2);

			innerMock.Verify(_ => _.Scan(context), Times.Once);
			innerMock.Raise(_ => _.CollectionChanged += null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			var items3 = cache.Scan(context);
			var items4 = cache.Scan(context);
			CollectionAssert.AreEqual(expected, items3);
			CollectionAssert.AreEqual(expected, items4);

			innerMock.Verify(_ => _.Scan(context), Times.Exactly(2));
		}

	}
}