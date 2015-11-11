using Mono.Cecil;

namespace NCode.Scanners.Options
{
	public interface IProvideReaderParameters : IScanOption
	{
		ReaderParameters ReaderParameters { get; set; }
	}

	public class ProvideReaderParameters : IProvideReaderParameters
	{
		private ReaderParameters _parameters;

		public ProvideReaderParameters()
		{
			_parameters = new ReaderParameters
			{
				AssemblyResolver = new DefaultAssemblyResolver()
			};
		}

		public virtual ReaderParameters ReaderParameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}

	}
}