using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xamarin.ManagedRegistrarGenerator
{

	internal sealed class NativeClass
	{
		private readonly INamedTypeSymbol _type;
		private readonly RegisterAttributeData? _registerAttribute;

		public NativeClass(INamedTypeSymbol type)
			: this(type, RegisterAttributeData.GetAttribute(type))
		{
		}

		public NativeClass(INamedTypeSymbol type, RegisterAttributeData? registerAttribute)
		{
			_type = type;
			_registerAttribute = registerAttribute;
		}

		public IEnumerable<ExportedMemberInfo> ExportedMembers
			=> ExportedMemberInfo.GetExportedMembers(_type);

		public string Name
			=> _registerAttribute switch
			{
				{ SkipRegistration: true } => GetSuperClass().Name,
				{ NativeClassName: string nativeClassName } => nativeClassName,
				_ => DeriveNativeClassName(),
			};

		public string? GetFramework()
			=> _type.ContainingNamespace.Name switch
			{
				"Foundation" => "Foundation",
				"UIKit" => "UIKit",
				_ => null,
			};

		public NativeClass GetSuperClass()
		{
			INamedTypeSymbol? super = _type.BaseType;
			while (super is not null && (IsModel(super) || IsFakeProtocol(super)))
			{
				super = super.BaseType;
			}

			if (super is null)
			{
				throw new Exception($"Cannot find the superclass of {_type}");
			}

			return new NativeClass(super);
		}

		public string GetSuperClassNameWithProtocols()
		{
			string[] protocols = GetImplementedProtocolNames();
			return protocols.Length == 0
				? GetSuperClass().Name
				: $"{GetSuperClass().Name}<{string.Join(", ", protocols)}>";
		}

		public bool IsFirstNonWrapper
			=> !(IsWrapper(_type) || IsModel(_type)) && (_type.BaseType is INamedTypeSymbol baseType && (IsWrapper(baseType) || IsModel(baseType)));

		private string DeriveNativeClassName()
		{
			if (IsWrapper(_type))
			{
				// TODO is this correct?
				return _type.Name;
			}

			var stack = new Stack<string>();
			INamedTypeSymbol? typeSymbol = _type;

			while (typeSymbol is not null)
			{
				stack.Push(typeSymbol.MetadataName.Replace("`", "_"));
				typeSymbol = typeSymbol.ContainingType;
			}

			if (_type.ContainingNamespace?.Name is string ns)
			{
				stack.Push(ns);
			}

			return string.Join("_", stack);
		}

		private string[] GetImplementedProtocolNames()
		{
			var protocolNames = new List<string>();

			INamedTypeSymbol? type = _type;
			while (type is not null)
			{
				if (IsWrapper(type))
				{
					// no need to declare protocols for wrapper types, they do it already in their headers.
					break;
				}

				protocolNames.AddRange(GetImplementedProtocolNames(type));
				protocolNames.AddRange(GetAdoptedProtocolNames(type));

				type = type.BaseType;
			}

			return protocolNames.OrderBy(name => name).Distinct().ToArray();
		}

		private static IEnumerable<string> GetImplementedProtocolNames(INamedTypeSymbol namedType)
		{
			// TODO type.Interfaces or type.AllInterfaces?
			// TODO skip UIAppearance - not a real protocol
			foreach (INamedTypeSymbol implementedInterface in namedType.Interfaces)
			{
				if (IsProtocol(implementedInterface, out ProtocolAttributeData? protocolData) && protocolData is not null)
				{
					// TODO FormalSince ??
					if (protocolData.IsInformal)
					{
						// all the interfaces are the implemented protocols
						// but the information protocol itself isn't
						foreach (INamedTypeSymbol informalProtocolInterface in implementedInterface.AllInterfaces)
						{
							yield return new NativeClass(informalProtocolInterface).Name;
						}
					}
					else
					{
						yield return protocolData.Name ?? new NativeClass(implementedInterface).Name;
					}
				}
			}
		}

		private static IEnumerable<string> GetAdoptedProtocolNames(INamedTypeSymbol namedType)
		{
			foreach (var attribute in namedType.GetAttributes())
			{
				if (IsAdoptsAttribute(attribute))
				{
					string? protocolTypeName = attribute.ConstructorArguments.Length > 0
						? attribute.ConstructorArguments[0].Value as string
						: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "ProtocolType").Value.Value as string;

					if (!string.IsNullOrEmpty(protocolTypeName))
					{
						yield return protocolTypeName!;
					}
				}
			}

			static bool IsAdoptsAttribute(AttributeData attribute)
				=> attribute.AttributeClass?.ToDisplayString() == "Foundation.ProtocolAttribute";
		}

		private static bool IsModel(INamedTypeSymbol namedType)
			=> namedType.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "Foundation.ModelAttribute");

		private static bool IsWrapper(INamedTypeSymbol namedType)
		{
			// TODO revisit this method

			if (namedType.Name == "NSObject"
				&& namedType.ContainingNamespace.Name == "Foundation"
				&& namedType.ContainingType is null)
			{
				return true;
			}

			return IsProtocol(namedType, out ProtocolAttributeData? protocolData)
				&& protocolData is not null
				&& protocolData.WrapperType is not null;
		}

		private sealed record ProtocolAttributeData(Type? WrapperType, string? Name, bool IsInformal, string? FormalSince);

		private static bool IsProtocol(INamedTypeSymbol namedType, out ProtocolAttributeData? protocolData)
		{
			if (namedType.TypeKind == TypeKind.Interface)
			{
				var attribute = namedType.GetAttributes().FirstOrDefault(IsProtocolAttribute);
				if (attribute is not null)
				{
					var wrapperType = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "WrapperType").Value.Value as Type;
					var name = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "Name").Value.Value as string;
					var isInformal = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "IsInformal").Value.Value as bool?;
					var formalSince = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "FormalSince").Value.Value as string;

					protocolData = new ProtocolAttributeData(wrapperType, name, isInformal ?? false, formalSince);
					return true;
				}
			}

			protocolData = null;
			return false;

			static bool IsProtocolAttribute(AttributeData attribute)
				=> attribute.AttributeClass?.ToDisplayString() == "Foundation.ProtocolAttribute";
		}

		private static bool IsFakeProtocol(INamedTypeSymbol namedType)
		{
			// TODO
			return false;
		}
	}
}
