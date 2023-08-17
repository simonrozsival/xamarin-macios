using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal sealed record RegisteredClassInfo(INamedTypeSymbol Type, ClassDeclarationSyntax Syntax, RegisterAttributeData? Attribute, ExportedMemberInfo[] ExportedMembers)
	{
		internal string GetNativeClassName()
		{
			if (Attribute?.NativeClassName is string nativeClassName)
				return nativeClassName;

			var stack = new Stack<string>();
			INamedTypeSymbol? typeSymbol = Type;

			while (typeSymbol is not null)
			{
				stack.Push(typeSymbol.MetadataName.Replace("`", "_"));
				typeSymbol = typeSymbol.ContainingType;
			}

			if (Type.ContainingNamespace?.Name is string ns)
				stack.Push(ns);

			return string.Join("_", stack);
		}
	}

	internal sealed record RegisterAttributeData(string? NativeClassName, bool IsWrapper, bool SkipRegistration);
	internal sealed record ExportedMemberInfo(ISymbol Symbol, ExportAttributeData? Attribute);
	internal sealed record ExportAttributeData(string Selector, int ArgumentSemantic, bool IsVariadic);
}
