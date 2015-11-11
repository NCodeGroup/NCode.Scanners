using System;

namespace NCode.Scanners
{
	public static class FactoryExtensions
	{
		public static IAppDomainScanner CurrentDomain(this IScannerFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			var appDomain = System.AppDomain.CurrentDomain;
			return new AppDomainScanner(factory, appDomain);
		}

		public static IAppDomainScanner AppDomain(this IScannerFactory factory, AppDomain appDomain)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (appDomain == null) throw new ArgumentNullException(nameof(appDomain));
			return new AppDomainScanner(factory, appDomain);
		}

		public static IAppDomainScanner AsScanner(this AppDomain appDomain, IScannerFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (appDomain == null) throw new ArgumentNullException(nameof(appDomain));
			return new AppDomainScanner(factory, appDomain);
		}

	}
}