using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal sealed record RegisteredClassInfo(INamedTypeSymbol Type, RegisterAttributeData? Attribute, ExportedMemberInfo[] ExportedMembers)
	{
		public static RegisteredClassInfo GetRegisteredClassInfo(INamedTypeSymbol namedType, CancellationToken? cancellationToken = null)
		{
			cancellationToken ??= CancellationToken.None;
			
			// If the class has the [Register] attribute, we should skip registering that class if it is a wrapper
			// or if the "skip registration" flag is set. If there isn't any attribute, we will always register it.
			RegisterAttributeData? attributeData = RegisterAttributeData.GetAttribute(namedType);

			var isWrapper = attributeData is not null && (attributeData.IsWrapper || attributeData.SkipRegistration); // TODO I need to check again what SkipRegistration actually means and how it's different
			var exportedMembers = !isWrapper
				? ExportedMemberInfo.GetExportedMembers(namedType, cancellationToken.Value).ToArray()
				: Array.Empty<ExportedMemberInfo>();

			return new RegisteredClassInfo(namedType, attributeData, exportedMembers);
		}
	}

	internal sealed record RegisterAttributeData(string? NativeClassName, bool IsWrapper, bool SkipRegistration)
	{
		public static RegisterAttributeData? GetAttribute(INamedTypeSymbol type)
		{
			AttributeData? attribute = type.GetAttributes().FirstOrDefault(IsRegisterAttribute);
			if (attribute is null)
			{
				return null;
			}
			
			return GetRegisterAttributeData(attribute);

			static bool IsRegisterAttribute(AttributeData attribute)
				=> attribute.AttributeClass?.Name is "RegisterAttribute"
					&& attribute.AttributeClass?.ContainingNamespace.Name is "Foundation"
					&& attribute.AttributeClass?.ContainingType is null;

			static RegisterAttributeData GetRegisterAttributeData(AttributeData attribute)
			{
				var name = attribute.ConstructorArguments.Length > 0
					? attribute.ConstructorArguments[0].Value as string
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "Name").Value.Value as string;
				var isWrapper = attribute.ConstructorArguments.Length > 1
					? attribute.ConstructorArguments[1].Value as bool?
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "IsWrapper").Value.Value as bool?;
				var skipRegistration = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "SkipRegistration").Value.Value as bool?;

				return new RegisterAttributeData(name, isWrapper ?? false, skipRegistration ?? false);
			}
		}
	}

	internal sealed record ExportedMemberInfo(ISymbol Symbol, ExportAttributeData Attribute)
	{
		public static IEnumerable<ExportedMemberInfo> GetExportedMembers(INamedTypeSymbol type, CancellationToken? cancellationToken = null)
		{
			// TODO use it
			cancellationToken ??= CancellationToken.None;

			foreach (ISymbol member in type.GetMembers())
			{
				if (ExportAttributeData.TryGetExportAttribute(member, out ExportAttributeData? attributeData)
					&& attributeData is not null)
				{
					yield return new ExportedMemberInfo(member, attributeData);
				}
				else if (member is IMethodSymbol method
					&& method.MethodKind == MethodKind.Constructor
					&& !type.IsAbstract // TODO the Mono.Cecil code doesn't remove ctors of abstract classes
					&& !method.IsStatic
					&& method.Parameters.Length == 0)
				{
					// the parameterless constructor is special
					yield return new ExportedMemberInfo(
						member, new ExportAttributeData(Selector: "init", ArgumentSemantic: -1, IsVariadic: false));
				}
			}
		}
	}

	internal sealed record ExportAttributeData(string Selector, int ArgumentSemantic, bool IsVariadic)
	{
		public static bool TryGetExportAttribute(
			ISymbol member,
			out ExportAttributeData? attributeData)
		{
			AttributeData? attribute = member.GetAttributes().FirstOrDefault(IsExportAttribute);
			if (attribute is null
				&& member is IMethodSymbol method
				&& method.OverriddenMethod is IMethodSymbol overriddenMethod)
			{
				return TryGetExportAttribute(overriddenMethod, out attributeData);
			}

			attributeData = GetExportAttributeData(attribute);
			return attributeData is not null;

			static bool IsExportAttribute(AttributeData attribute)
				=> attribute.AttributeClass?.ToDisplayString() == "Foundation.ExportAttribute"; // TODO extract constant

			static ExportAttributeData? GetExportAttributeData(AttributeData? attribute)
			{
				if (attribute is null)
					return null;

				var selector = attribute.ConstructorArguments.Length > 0
					? attribute.ConstructorArguments[0].Value as string
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "Selector").Value.Value as string;
				var argumentSemantic = attribute.ConstructorArguments.Length > 1
					? attribute.ConstructorArguments[1].Value as int?
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "ArgumentSemantic").Value.Value as int?;
				var isVariadic = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "IsVariadic").Value.Value as bool?;

				if (selector is null)
				{
					// TODO report diagnostic instead
					throw new ArgumentException($"Export attribute is missing selector");
				}

				return new ExportAttributeData(
					selector,
					argumentSemantic ?? -1, // TODO ArgumentSemantic.None == -1
					isVariadic ?? false);
			}
		}
	}
}
