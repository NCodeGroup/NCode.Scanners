using System;
using System.Collections.Generic;
using System.Reflection;

namespace NCode.Scanners
{
	public interface IAppDomainScanner : IScanner<Assembly>
	{
		AppDomain AppDomain { get; }
	}

	public class AppDomainScanner : Scanner<Assembly>, IAppDomainScanner
	{
		private readonly AppDomain _appDomain;

		public AppDomainScanner(AppDomain appDomain)
		{
			if (appDomain == null)
				throw new ArgumentNullException("appDomain");

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