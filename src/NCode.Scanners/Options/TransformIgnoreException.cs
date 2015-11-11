using System;
using System.Collections.Generic;
using System.Linq;

namespace NCode.Scanners.Options
{
	/// <summary>
	/// Represents an <see cref="IScanOption"/> that can be used to ignore
	/// exceptions when various <see cref="Transforms"/> methods are called.
	/// </summary>
	public interface ITransformIgnoreException : IScanOption
	{
		/// <summary>
		/// Used to determine whether the <paramref name="exception"/>
		/// can be ignored when the <paramref name="transform"/> is invoked.
		/// </summary>
		/// <param name="transform">Contains the name of transform.</param>
		/// <param name="exception">Contains the <see cref="Exception"/> that occured during transform.</param>
		/// <returns><c>true</c> if the exception should be ignored; otherwise <c>false</c> to propagate back to the caller.</returns>
		bool IgnoreException(string transform, Exception exception);
	}

	/// <summary>
	/// Provides an implementation of <see cref="ITransformIgnoreException"/>
	/// that will ignore exceptions based on their <see cref="Type"/>.
	/// </summary>
	public class TransformIgnoreExceptionByType : ITransformIgnoreException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransformIgnoreExceptionByType"/> class.
		/// </summary>
		public TransformIgnoreExceptionByType()
		{
			Types = new HashSet<Type>();
		}

		/// <summary>
		/// Contains the list of exception types that should be ignored.
		/// </summary>
		public virtual ISet<Type> Types { get; }

		public virtual bool IgnoreException(string transform, Exception exception)
		{
			if (exception == null) return true;
			var exceptionType = exception.GetType();
			return Types.Any(type => type.IsAssignableFrom(exceptionType));
		}

	}
}