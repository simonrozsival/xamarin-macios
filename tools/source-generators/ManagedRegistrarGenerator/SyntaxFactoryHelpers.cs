using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Xamarin.ManagedRegistrarGenerator.ConversionHelpers;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal static class SyntaxFactoryHelpers
	{
		internal static ExpressionSyntax GetNSObjectExpression(IdentifierNameSyntax variable, ITypeSymbol managedType)
			=> InvocationExpression(
				MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
					ParseTypeName(TypeNames.ObjCRuntime_Runtime),
					GenericName("GetNSObject")
						.AddTypeArgumentListArguments(IdentifierName(TypeNameHelpers.GetFullName(managedType)))),
				ArgumentList(
					SingletonSeparatedList(
						Argument(variable))));

		internal static ExpressionSyntax CreateNSObjectExpression(IdentifierNameSyntax variable, ITypeSymbol managedType)
			=> InvocationExpression(
				MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
					ParseTypeName(TypeNames.Foundation_NSObject),
					GenericName("CreateNSObject")
						.AddTypeArgumentListArguments(IdentifierName(TypeNameHelpers.GetFullName(managedType)))),
				ArgumentList(
					SingletonSeparatedList(
						Argument(variable))));

		internal static ExpressionSyntax RuntimeAllocGCHandleExpression(ExpressionSyntax expression)
			=> InvocationExpression(
					MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						IdentifierName("global::ObjCRuntime.Runtime"),
						IdentifierName("AllocGCHandle")))
				.WithArgumentList(
					ArgumentList(
						SingletonSeparatedList<ArgumentSyntax>(
							Argument(expression))));

		internal static StatementSyntax VariableInitializationStatement(ITypeSymbol variableType, string variableName, ExpressionSyntax expression)
			=> LocalDeclarationStatement(
				VariableDeclaration(
					TypeName(variableType),
					SingletonSeparatedList(
						VariableDeclarator(variableName)
							.WithInitializer(EqualsValueClause(expression)))));

		internal static ExpressionSyntax PrepareUnamangedCallersOnlyParameterForCallingOriginalMethodExpression(IParameterSymbol originalParameter, string parameterName)
		{
			if (originalParameter.Type.IsReferenceType) // TODO check it's a NSObject
			{
				// TODO strings
				// TODO arrays
				// TODO check it's a NSObject first
				return GetNSObjectExpression(IdentifierName(parameterName), originalParameter.Type);
			}

			if (originalParameter.RefKind == RefKind.Out)
			{
				return ParseExpression("default");
			}
			else if (originalParameter.RefKind != RefKind.None)
			{
				// the parameter is a value type and we'll be passing it to the UCO as a pointer
				// we need to dereference the pointer
				return PrefixUnaryExpression(
					SyntaxKind.PointerIndirectionExpression,
					IdentifierName(parameterName));
			}

			if (originalParameter.Type.SpecialType == SpecialType.System_Boolean)
			{
				// cast to byte
				return BinaryExpression(
					SyntaxKind.EqualsExpression,
					IdentifierName(parameterName),
					LiteralExpression(
						SyntaxKind.NumericLiteralExpression,
						Literal(1)));
			}

			throw new NotImplementedException($"We're missing a conversion for UCO parameter {originalParameter}");
		}

		internal static ExpressionSyntax CastReturnValueIfNeeded(ITypeSymbol returnType, string variableName)
		{
			if (returnType.SpecialType == SpecialType.System_Boolean)
			{
				return ConditionalExpression(
					IdentifierName(variableName),
					CastExpression(
						PredefinedType(
							Token(SyntaxKind.ByteKeyword)),
						LiteralExpression(
							SyntaxKind.NumericLiteralExpression,
							Literal(1))),
					CastExpression(
						PredefinedType(
							Token(SyntaxKind.ByteKeyword)),
						LiteralExpression(
							SyntaxKind.NumericLiteralExpression,
							Literal(0))));
			}

			if (returnType.IsReferenceType)
			{
				// TODO NSArray
				// TODO NSString
				// TODO NSObject -> return handle
				return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName("global::ObjCRuntime.NativeObjectExtensions"),
							IdentifierName("GetHandle")))
					.WithArgumentList(
						ArgumentList(
							SingletonSeparatedList<ArgumentSyntax>(
								Argument(
									IdentifierName(variableName)))));
	
				// TODO ...
			}

			if (returnType.IsValueType)
			{
				// return 
				return IdentifierName(variableName);
			}

			throw new NotImplementedException($"We're missing cast for the return value for {returnType}");
		}

		internal static ExpressionSyntax ManagedTypeConversionExpression(IdentifierNameSyntax variable, SpecialType nativeType, ITypeSymbol managedType)
			=> (nativeType, managedType) switch
			{
				(SpecialType x, { SpecialType: SpecialType y }) when x == y => variable,
				(SpecialType.System_IntPtr, { IsReferenceType: true }) => GetNSObjectExpression(variable, managedType), // TODO when NSObject
				(SpecialType.System_IntPtr, { SpecialType: SpecialType.System_Boolean }) => CastExpression(IdentifierName("bool"), variable),
				_ => throw new NotImplementedException($"We're missing a cast from {nativeType} to {managedType.SpecialType}"),
			};

		
		internal static TypeSyntax TypeName(ITypeSymbol managedType)
			=> managedType switch 
			{
				{ IsNativeIntegerType: true } => ParseTypeName(TypeNames.System_IntPtr),
				{ IsReferenceType: true } => ParseTypeName(TypeNameHelpers.GetFullName(managedType)),
				{ SpecialType: SpecialType specialType } => SpecialTypeToSyntax(specialType),
				_ => throw new NotImplementedException($"We're missing a managed -> native mapping for {managedType}"),
			};

		internal static SyntaxToken RefKindKeyword(RefKind refKind)
			=> refKind switch
			{
				RefKind.In => Token(SyntaxKind.InKeyword),
				RefKind.Out => Token(SyntaxKind.OutKeyword),
				RefKind.Ref => Token(SyntaxKind.RefKeyword),
				// RefKind.RefReadOnly => Token(SyntaxKind.RefKeyword), // TODO == 3 - same as `In` !!??
				_ => throw new NotImplementedException($"We're missing a ref kind keyword for {refKind}"),
			};
	}
}
