using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal static class ConversionHelpers
	{
		// TODO
		internal static TypeSyntax SpecialTypeToSyntax(SpecialType specialType)
			=> specialType switch
			{
				SpecialType.System_Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
				SpecialType.System_Byte => PredefinedType(Token(SyntaxKind.ByteKeyword)),
				SpecialType.System_Char => PredefinedType(Token(SyntaxKind.CharKeyword)),
				SpecialType.System_Decimal => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
				SpecialType.System_Double => PredefinedType(Token(SyntaxKind.DoubleKeyword)),
				SpecialType.System_Int16 => PredefinedType(Token(SyntaxKind.ShortKeyword)),
				SpecialType.System_Int32 => PredefinedType(Token(SyntaxKind.IntKeyword)),
				SpecialType.System_Int64 => PredefinedType(Token(SyntaxKind.LongKeyword)),
				SpecialType.System_IntPtr => ParseTypeName(TypeNames.System_IntPtr),
				SpecialType.System_SByte => PredefinedType(Token(SyntaxKind.SByteKeyword)),
				SpecialType.System_Single => PredefinedType(Token(SyntaxKind.FloatKeyword)),
				SpecialType.System_String => PredefinedType(Token(SyntaxKind.StringKeyword)),
				SpecialType.System_UInt16 => PredefinedType(Token(SyntaxKind.UShortKeyword)),
				SpecialType.System_UInt32 => PredefinedType(Token(SyntaxKind.UIntKeyword)),
				SpecialType.System_UInt64 => PredefinedType(Token(SyntaxKind.ULongKeyword)),
				SpecialType.System_UIntPtr => ParseTypeName(TypeNames.System_UIntPtr),
				SpecialType.System_Void => PredefinedType(Token(SyntaxKind.VoidKeyword)),
				_ => throw new NotImplementedException($"Conversion of special type {specialType} hasn't been implemented"),
		};

		internal static TypeSyntax GetNativeTypeForManagedType(ITypeSymbol typeSymbol)
			=> typeSymbol switch
			{
				{ IsReferenceType: true } => ParseTypeName(TypeNames.System_IntPtr),
				{ SpecialType: SpecialType.System_Boolean } => SpecialTypeToSyntax(SpecialType.System_Byte),
				{ SpecialType: SpecialType specialType } => SpecialTypeToSyntax(specialType),
				_ => throw new NotImplementedException($"Conversion of type {typeSymbol} hasn't been implemented"),
			};

		internal static bool ParameterNeedsConversion(IParameterSymbol originalParameter)
			=> originalParameter switch
			{
				{ RefKind: not RefKind.None } => true,
				{ Type: { SpecialType: SpecialType.System_Boolean } } => true,
				{ Type: { IsReferenceType: true } } => true,
				{ Type: { IsValueType: true } } => false,
				_ => throw new NotImplementedException($"Parameter classification for {originalParameter} ({originalParameter.Type}) hasn't been implemented"),
			};

		internal static TypeSyntax UnmanagedCallersOnlyParameterType(IParameterSymbol originalParameter)
		{
			if (originalParameter.RefKind != RefKind.None)
				return PointerType(GetNativeTypeForManagedType(originalParameter.Type));

			// TODO is this enough?
			return GetNativeTypeForManagedType(originalParameter.Type);
		}
	}
}
