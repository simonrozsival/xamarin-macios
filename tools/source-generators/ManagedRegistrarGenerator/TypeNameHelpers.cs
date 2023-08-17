using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal static class TypeNameHelpers
	{
		internal static string GetFullName(ITypeSymbol typeSymbol)
		{
			var stack = new Stack<string>();
			var ns = typeSymbol.ContainingNamespace?.Name;

			while (typeSymbol is not null)
			{
				stack.Push(GetName(typeSymbol));
				typeSymbol = typeSymbol.ContainingType;
			}

			if (ns is not null)
				stack.Push(ns);

			return string.Join(".", stack);
		}

		internal static string GetName(ITypeSymbol typeSymbol)
			=> typeSymbol is INamedTypeSymbol namedTypeSymbol
				? GetName(namedTypeSymbol)
				: typeSymbol.Name;

		internal static string GetName(INamedTypeSymbol namedTypeSymbol)
		{
			if (!namedTypeSymbol.IsGenericType)
				return namedTypeSymbol.Name;

			var sb = new StringBuilder();
			sb.Append(namedTypeSymbol.Name);
			sb.Append("<");
			for (int i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
			{
				if (i != 0) sb.Append(", ");
				sb.Append(namedTypeSymbol.TypeArguments[i].Name);
			}
			sb.Append(">");
			return sb.ToString();
		}
	}
}
