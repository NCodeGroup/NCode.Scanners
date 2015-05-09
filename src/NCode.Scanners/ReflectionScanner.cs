using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NCode.Scanners
{
	public interface IReflectionScanner<out T> : IScanner<T>, IUseParentScanner<T>
	{
		ReflectionContext ReflectionContext { get; }
	}

	public abstract class ReflectionScanner<T> : UseParentScanner<T>, IReflectionScanner<T>
	{
		private readonly ReflectionContext _reflectionContext;

		protected ReflectionScanner(IScanner<T> parent, ReflectionContext reflectionContext)
			: base(parent)
		{
			if (reflectionContext == null)
				throw new ArgumentNullException("reflectionContext");

			_reflectionContext = reflectionContext;
		}

		protected abstract T MapItem(T item);

		#region IReflectionScanner<T> Members

		public virtual ReflectionContext ReflectionContext
		{
			get { return _reflectionContext; }
		}

		#endregion

		#region IScanner<T> Members

		public override IEnumerable<T> Scan(IScanContext context)
		{
			var items = GetParentItemsOrEmpty(context);
			var results = items.Select(MapItem);
			return results;
		}

		#endregion
	}

	public class ReflectionAssemblyScanner : ReflectionScanner<Assembly>
	{
		public ReflectionAssemblyScanner(IScanner<Assembly> parent, ReflectionContext reflectionContext)
			: base(parent, reflectionContext)
		{
			// nothing
		}

		protected override Assembly MapItem(Assembly item)
		{
			return ReflectionContext.MapAssembly(item);
		}
	}

	public class ReflectionTypeInfoScanner : ReflectionScanner<TypeInfo>
	{
		public ReflectionTypeInfoScanner(IScanner<TypeInfo> parent, ReflectionContext reflectionContext)
			: base(parent, reflectionContext)
		{
			// nothing
		}

		protected override TypeInfo MapItem(TypeInfo item)
		{
			return ReflectionContext.MapType(item);
		}
	}

}