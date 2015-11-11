using System;
using System.Collections;
using System.Linq;
using Mono.Cecil;

namespace NCode.Scanners.Options
{
	internal class TypeDefinitionCache : IScanOption
	{
		private readonly Hashtable _cache = new Hashtable();

		public TypeDefinition ResolveTypeDefinition(IScanContext context, Type type)
		{
			var token = new MetadataToken((uint)type.MetadataToken);
			var typeDefinition = _cache[token] as TypeDefinition;

			if (typeDefinition == null)
			{
				var parameters = context.GetReaderParameters();
				var assembly = AssemblyDefinition.ReadAssembly(type.Assembly.Location, parameters);
				var typeReference = assembly.MainModule.GetTypes().Single(_ => _.MetadataToken == token);
				typeDefinition = typeReference.Resolve();
				_cache[token] = typeDefinition;
			}

			return typeDefinition;
		}

	}
}