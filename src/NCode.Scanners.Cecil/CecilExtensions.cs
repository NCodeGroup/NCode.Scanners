using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NCode.Scanners.Options;
using AssemblyHashAlgorithm = System.Configuration.Assemblies.AssemblyHashAlgorithm;

namespace NCode.Scanners
{
	internal static class CecilExtensions
	{
		public static AssemblyNameDefinition GetAssemblyNameDefinition(this AssemblyName assemblyName)
		{
			if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

			var assemblyNameDefinition = new AssemblyNameDefinition(assemblyName.Name, assemblyName.Version)
			{
				Culture = assemblyName.CultureName,
				PublicKey = assemblyName.GetPublicKey(),
				PublicKeyToken = assemblyName.GetPublicKeyToken()
			};

			if (assemblyName.HashAlgorithm == AssemblyHashAlgorithm.SHA1)
				assemblyNameDefinition.HashAlgorithm = Mono.Cecil.AssemblyHashAlgorithm.SHA1;

			if (assemblyName.Flags.HasFlag(AssemblyNameFlags.PublicKey))
				assemblyNameDefinition.Attributes |= AssemblyAttributes.PublicKey;

			if (assemblyName.Flags.HasFlag(AssemblyNameFlags.Retargetable))
				assemblyNameDefinition.Attributes |= AssemblyAttributes.Retargetable;

			// yes, the name is different Enable vs Disable, that is on purpose
			if (assemblyName.Flags.HasFlag(AssemblyNameFlags.EnableJITcompileOptimizer))
				assemblyNameDefinition.Attributes |= AssemblyAttributes.DisableJITCompileOptimizer;

			if (assemblyName.Flags.HasFlag(AssemblyNameFlags.EnableJITcompileTracking))
				assemblyNameDefinition.Attributes |= AssemblyAttributes.EnableJITCompileTracking;

			return assemblyNameDefinition;
		}

		public static TAttribute ResolveAttribute<TAttribute>(this CustomAttribute metadata)
			where TAttribute : Attribute
		{
			var type = typeof(TAttribute);
			var args = metadata.ConstructorArguments.Select(arg => arg.Value).ToArray();
			var attr = (TAttribute)Activator.CreateInstance(type, args);

			foreach (var prop in metadata.Properties)
			{
				type.GetProperty(prop.Name).SetValue(attr, prop.Argument.Value);
			}
			foreach (var field in metadata.Fields)
			{
				type.GetField(field.Name).SetValue(attr, field.Argument.Value);
			}

			return attr;
		}

		public static ReaderParameters GetReaderParameters(this IScanContext context)
		{
			var provide = context.Options.Find<IProvideReaderParameters>();
			if (provide == null)
			{
				provide = new ProvideReaderParameters();
				context.Options.Add(provide);
			}

			var parameters = provide.ReaderParameters;
			if (parameters == null)
			{
				parameters = new ReaderParameters();
				provide.ReaderParameters = parameters;
			}

			if (parameters.AssemblyResolver == null)
				parameters.AssemblyResolver = new DefaultAssemblyResolver();

			return parameters;
		}

		public static TypeDefinition ResolveTypeDefinition(this IScanContext context, Type type)
		{
			var cache = context.Options.Find<TypeDefinitionCache>();
			if (cache == null)
			{
				cache = new TypeDefinitionCache();
				context.Options.Add(cache);
			}
			return cache.ResolveTypeDefinition(context, type);
		}

		public static bool IsAssignableFrom(this Type me, IScanContext context, TypeDefinition other)
		{
			var typeDefinition = context.ResolveTypeDefinition(me);
			return IsAssignableFrom(typeDefinition, other);
		}

		public static bool IsAssignableFrom(this TypeDefinition me, TypeDefinition other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if (me.MetadataToken == other.MetadataToken)
				return true;

			if (other.IsSubclassOf(me))
				return true;

			if (me.IsInterface)
				return other.ImplementsInterface(me);

			return false;
		}

		public static bool IsSubclassOf(this TypeDefinition me, TypeDefinition other)
		{
			if (me.MetadataToken == other.MetadataToken)
				return false;

			var baseType = me.BaseType;
			while (baseType != null)
			{
				if (baseType.MetadataToken == other.MetadataToken)
					return true;

				baseType = baseType.Resolve().BaseType;
			}

			return false;
		}

		public static bool ImplementsInterface(this TypeDefinition me, TypeDefinition iFaceType)
		{
			for (var type = me; type != null; type = type.BaseType?.Resolve())
			{
				var interfaces = type.Interfaces.Select(iface => iface.Resolve());
				foreach (var iface in interfaces)
				{
					if (iface.MetadataToken == iFaceType.MetadataToken)
						return true;

					if (iface.ImplementsInterface(iFaceType))
						return true;
				}
			}
			return false;
		}

	}
}