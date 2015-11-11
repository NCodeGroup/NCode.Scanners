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

using System.Collections.Specialized;
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
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Strict);
			innerMock.SetupGet(_ => _.Factory).Returns(factory);
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
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Strict);
			innerMock.SetupGet(_ => _.Factory).Returns(factory);
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
			var factory = ScannerFactory.Create();
			var context = factory.CreateContext();
			var expected = Enumerable.Range(1, 5).ToArray();

			var innerMock = new Mock<IScanner<int>>(MockBehavior.Strict);
			innerMock.SetupGet(_ => _.Factory).Returns(factory);
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