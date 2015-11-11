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

namespace NCode.Scanners
{
	/// <summary>
	/// Represents a method that handles the <see cref="E:IScannerFactory.ContentCreated"/>
	/// event of an <see cref="IScannerFactory"/>.
	/// </summary>
	/// <param name="context">The newly created <see cref="IScanContext"/> instance.</param>
	public delegate void ContextCreatedHandler(IScanContext context);

	/// <summary>
	/// Provides an interface for extension methods that provide factory methods
	/// to instantiate various implementations of <see cref="IScanner{T}"/>.
	/// </summary>
	public interface IScannerFactory : ISupportFactory
	{
		/// <summary>
		/// Occurs when a new <see cref="IScanContext"/> is created from the <see cref="CreateContext"/> method.
		/// </summary>
		event ContextCreatedHandler ContextCreated;

		/// <summary>
		/// Creates and returns a new instance of <see cref="IScanContext"/>.
		/// </summary>
		IScanContext CreateContext();
	}

	/// <summary>
	/// Represents an interface that can be used to provide a <see cref="IScannerFactory"/> property.
	/// </summary>
	public interface ISupportFactory
	{
		IScannerFactory Factory { get; }
	}

	/// <summary>
	/// Contains the default implementation for <see cref="IScannerFactory"/>.
	/// </summary>
	public class DefaultScannerFactory : IScannerFactory
	{
		#region ISupportFactory Members

		public virtual IScannerFactory Factory
		{
			get { return this; }
		}

		#endregion

		#region ISupportFactory Members

		public virtual event ContextCreatedHandler ContextCreated;

		public virtual IScanContext CreateContext()
		{
			var context = new ScanContext(this);
			var handler = ContextCreated;
			handler?.Invoke(context);
			return context;
		}

		#endregion
	}

	/// <summary>
	/// Contains factory methods to instatiate various implementations of <see cref="IScanner{T}"/>.
	/// The various implementations are provided from extension methods to either the <see cref="IScanner{T}"/>
	/// or <see cref="IScannerFactory"/> interfaces.
	/// </summary>
	public static partial class ScannerFactory
	{
		/// <summary>
		/// Creates and returns a new instance of the default implementation for <see cref="IScannerFactory"/>.
		/// </summary>
		public static IScannerFactory Create()
		{
			return new DefaultScannerFactory();
		}

	}
}