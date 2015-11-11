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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NCode.Scanners
{
	/// <summary>
	/// Represents an <see cref="IScanner{T}"/> that retieves <see cref="Assembly"/>
	/// objects using <see cref="M:AppDomain.GetAssemblies"/>.
	/// </summary>
	/// <remarks>
	/// This scanner will raise it's <see cref="E:PropertyChanged"/> and <see cref="E:CollectionChanged"/>
	/// events when it detects the <see cref="E:AppDomain.AssemblyLoad"/> event.
	/// </remarks>
	public interface IAppDomainScanner : IScanner<Assembly>
	{
		/// <summary>
		/// Contains the <see cref="AppDomain"/> that this scanner uses to retieve
		/// it's <see cref="Assembly"/> objects from.
		/// </summary>
		/// <remarks>
		/// When additional assemblies load into this <see cref="AppDomain"/>, the
		/// <see cref="IAppDomainScanner"/> will raise it's <see cref="E:PropertyChanged"/>
		/// and <see cref="E:CollectionChanged"/> events.
		/// </remarks>
		AppDomain AppDomain { get; }
	}

	/// <summary>
	/// Provides the default implementation for the <see cref="IAppDomainScanner"/> interface.
	/// </summary>
	public class AppDomainScanner : Scanner<Assembly>, IAppDomainScanner
	{
		private readonly AppDomain _appDomain;

		/// <summary>
		/// Intializes a new instance of <see cref="AppDomainScanner"/> with the
		/// specified <see cref="AppDomain"/>.
		/// </summary>
		/// <param name="appDomain">The <see cref="AppDomain"/> that this scanner uses to retieves it's <see cref="Assembly"/> objects from.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="appDomain"/> argument is a null.</exception>
		public AppDomainScanner(IScannerFactory factory, AppDomain appDomain)
			: base(factory)
		{
			if (appDomain == null) throw new ArgumentNullException(nameof(appDomain));

			_appDomain = appDomain;
			appDomain.AssemblyLoad += HandleAssemblyLoad;
		}

		private void HandleAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			OnPropertyChanged("AppDomain");
			OnCollectionChanged();
		}

		#region IAppDomainScanner Members

		public virtual AppDomain AppDomain
		{
			get { return _appDomain; }
		}

		#endregion

		#region IScanner<T> Members

		public override IEnumerable<Assembly> Scan(IScanContext context)
		{
			return AppDomain.GetAssemblies();
		}

		#endregion
	}
}