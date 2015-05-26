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

namespace NCode.Scanners
{
	/// <summary>
	/// Defines a contract for custom options that can be used to alter the
	/// behavior of <see cref="IScanner{T}.Scan"/> at runtime.
	/// </summary>
	public interface IScanOption
	{
		// nothing
	}

	/// <summary>
	/// Represents a context that can provide additional options at runtime to
	/// the <see cref="IScanner{T}.Scan"/> method.
	/// </summary>
	public interface IScanContext
	{
		/// <summary>
		/// Gets a collection that allows storing and retrieving <see cref="IScanOption"/> objects by their type.
		/// </summary>
		KeyedByTypeCollection<IScanOption> Options { get; }
	}

	/// <summary>
	/// Provides the default implementation for <see cref="IScanContext"/>.
	/// </summary>
	public class ScanContext : IScanContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScanContext"/> class.
		/// </summary>
		public ScanContext()
		{
			Options = new KeyedByTypeCollection<IScanOption>();
		}

		#region IScanContext Members

		public virtual KeyedByTypeCollection<IScanOption> Options { get; private set; }

		#endregion
	}
}