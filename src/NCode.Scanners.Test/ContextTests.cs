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

using NUnit.Framework;

namespace NCode.Scanners.Test
{
	[TestFixture]
	public class ContextTests
	{
		[Test]
		public void CreateContextIsNotNull()
		{
			var context = ScannerFactory.CreateContext();
			Assert.IsNotNull(context);
		}

		[Test]
		public void CreateContextTwiceIsDifferent()
		{
			var context1 = ScannerFactory.CreateContext();
			var context2 = ScannerFactory.CreateContext();
			Assert.AreNotSame(context1, context2);
		}

	}
}