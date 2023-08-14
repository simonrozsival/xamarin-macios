using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xamarin.ManagedRegistrarGenerator
{
	[Generator]
	public class ManagedRegistrarGenerator : IIncrementalGenerator
	{
		private sealed record RegisteredClassInfo(INamedTypeSymbol Type, ClassDeclarationSyntax Syntax, string? ExplicitName);
		private sealed record RegisterAttributeData(string Name, bool IsWrapper, bool SkipRegistration);

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			var unsafeCodeIsEnabled = context.CompilationProvider.Select((comp, ct) => comp.Options is CSharpCompilationOptions { AllowUnsafe: true }); // Unsafe code enabled

			var registeredClassesOrDiagnostics = context.SyntaxProvider
				.CreateSyntaxProvider(
					predicate: IsClassDeclarationWithBaseList,
					transform: Transform)
				.Where(static m => m is not null)
				.Combine(unsafeCodeIsEnabled)
				.Select((data, _) =>
					{
						RegisteredClassInfo registeredClassInfo = data.Left!;
						bool unsafeCodeIsEnabled = data.Right;

						if (!unsafeCodeIsEnabled)
						{
							return DiagnosticOr<RegisteredClassInfo>.FromDiagnostic(
								Diagnostic.Create(
									GeneratorDiagnostics.RequiresAllowUnsafeBlocks,
									registeredClassInfo.Syntax.Identifier.GetLocation()));
						}

						// TODO if it's a nested class all of the parent classes must be partial
						if (!registeredClassInfo.Syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
						{
							return DiagnosticOr<RegisteredClassInfo>.FromDiagnostic(
								Diagnostic.Create(
									GeneratorDiagnostics.InvalidAttributedClassMissingPartialModifier,
									registeredClassInfo.Syntax.Identifier.GetLocation(),
									registeredClassInfo.Type.ToDisplayString()));
						}

						return DiagnosticOr<RegisteredClassInfo>.FromValue(registeredClassInfo);
					});

			var (diagnostics, registeredClasses) = DiagnosticOr<RegisteredClassInfo>.Split(registeredClassesOrDiagnostics);
			context.RegisterSourceOutput(diagnostics, (context, diagnostic) => context.ReportDiagnostic(diagnostic));

			IncrementalValueProvider<(Compilation, ImmutableArray<RegisteredClassInfo>)> compilationAndClasses
				= context.CompilationProvider.Combine(registeredClasses.Collect());

			context.RegisterSourceOutput(compilationAndClasses,
				static (spc, source) => Execute(source.Item1, source.Item2, spc));
		}

		private static bool IsClassDeclarationWithBaseList(SyntaxNode node, CancellationToken cancellationToken)
			=> node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };

		private static RegisteredClassInfo? Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
		{
			var classDeclaration = (ClassDeclarationSyntax)context.Node;
			var classSymbol = context.SemanticModel.GetSymbolInfo(classDeclaration).Symbol as INamedTypeSymbol;
			if (classSymbol is null)
			{
				return null;
			}
			
			// TODO is this correct?
			if (!IsNSObjectSubclass(classSymbol, cancellationToken))
			{
				return null;
			}

			// If the class has the [Register] attribute, we should skip registering that class if it is a wrapper
			// or if the "skip registration" flag is set.
			if (TryGetRegisterAttribute(classSymbol, out var attributeData) && attributeData is not null)
			{
				if (attributeData.IsWrapper || attributeData.SkipRegistration)
				{
					return null;
				}
			}

			return new RegisteredClassInfo(classSymbol, classDeclaration, attributeData?.Name);
		}

		private static bool IsNSObjectSubclass(INamedTypeSymbol? type, CancellationToken cancellationToken)
		{
			while (type is not null)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (type.ToDisplayString() == "Foundation.NSObject") // TODO extract constant
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		private static bool TryGetRegisterAttribute(
			INamedTypeSymbol type,
			out RegisterAttributeData? attributeData)
		{
			AttributeData? attribute = type.GetAttributes().FirstOrDefault(IsRegisterAttribute);
			attributeData = GetRegisterAttributeData(attribute);
			return attributeData is not null;

			static bool IsRegisterAttribute(AttributeData attribute)
				=> attribute.AttributeClass?.ToDisplayString() == "Foundation.RegisterAttribute"; // TODO extract constant

			static RegisterAttributeData? GetRegisterAttributeData(AttributeData? attribute)
			{
				if (attribute is null)
					return null;

				var name = attribute.ConstructorArguments.Length > 0
					? attribute.ConstructorArguments[0].Value as string
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "Name").Value.Value as string;
				var isWrapper = attribute.ConstructorArguments.Length > 1
					? attribute.ConstructorArguments[1].Value as bool?
					: attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "IsWrapper").Value.Value as bool?;
				var skipRegistration = attribute.NamedArguments.FirstOrDefault(static arg => arg.Key == "SkipRegistration").Value.Value as bool?;

				return new RegisterAttributeData(
					name ?? string.Empty,
					isWrapper ?? false,
					skipRegistration ?? false);
			}
		}

		private static void Execute(
			Compilation compilation,
			ImmutableArray<RegisteredClassInfo> registeredClasses,
			SourceProductionContext context)
		{
			// TODO
			var source = "// <autogenerated />\n";
			foreach (var registeredClass in registeredClasses) {
				source += $"// {registeredClass.Type.ToDisplayString()} (explicit name='{registeredClass.ExplicitName}')\n";
			}
			context.AddSource("ManagedRegistrar.g.cs", SourceText.From(source, Encoding.UTF8));
		}
	}
}
