using System.Collections.Generic;

namespace NCode.Scanners
{
	public interface IScanOption
	{
		// nothing
	}

	public interface IScanContext
	{
		KeyedByTypeCollection<IScanOption> Options { get; }
	}

	public class ScanContext : IScanContext
	{
		public ScanContext()
		{
			Options = new KeyedByTypeCollection<IScanOption>();
		}

		#region IScanContext Members

		public virtual KeyedByTypeCollection<IScanOption> Options { get; private set; }

		#endregion
	}
}