using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Xamarin.ManagedRegistrarGenerator.SyntaxFactoryHelpers;
using static Xamarin.ManagedRegistrarGenerator.ConversionHelpers;
using static Xamarin.ManagedRegistrarGenerator.TypeNameHelpers;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal class ManagedCodeGenerator
	{
		private readonly RegisteredClassInfo _classInfo;
		private const string RegistrarCallbacksName = "__Registrar_Callbacks__";
		
		public ManagedCodeGenerator(RegisteredClassInfo classInfo)
		{
			_classInfo = classInfo;
		}

		public MemberDeclarationSyntax Generate(CancellationToken cancellationToken)
		{
			var classDeclaration = ClassDeclaration(GetName(_classInfo.Type))
				.AddModifiers(Token(SyntaxKind.PartialKeyword));

			if (ShouldGenerateCreateNSObjectFactoryMethod())
			{
				// TODO add `new` if any of the base types already has `CreateNSObject`
				classDeclaration = classDeclaration.AddMembers(
					ParseMemberDeclaration($"public static {GetFullName(_classInfo.Type)} CreateNSObject(global::ObjCRuntime.NativeHandle nativeHandle) => new {GetFullName(_classInfo.Type)}(nativeHandle);\n\n")!);
			}

			if (ShouldGenerateCreateINativeObjectFactoryMethod())
			{
				// TODO add `new` if any of the base types already has `CreateINativeObject`
				classDeclaration = classDeclaration.AddMembers(
					ParseMemberDeclaration($"public static {GetFullName(_classInfo.Type)} CreateINativeObject(global::ObjCRuntime.NativeHandle nativeHandle, bool owns) => new {GetFullName(_classInfo.Type)}(nativeHandle, owns);\n\n")!);
			}

			if (IsCustomType())
			{
				// TODO add `new` if any of the base types already has `IsCustomType`
				classDeclaration = classDeclaration.AddMembers(
					ParseMemberDeclaration($"public static bool IsCustomType => true;\n\n")!);
			}

			// TODO the "nesting" mechanism with an array is ugly, but it works. refactor it if there's time
			ClassDeclarationSyntax[] classesToNest = Array.Empty<ClassDeclarationSyntax>();

			if (_classInfo.Type.IsGenericType)
			{
				// TODO implement the proxy for the generic class and remove the temporary impl
				// var registrarCallbacks = GenerateRegistrarCallbacks(className: GetFullName(_classInfo.Type, separator: "_", withGenericParameterNames: false) + RegistrarCallbacksName);
				// classesToNest = new[] { classDeclaration, registrarCallbacks };
				classesToNest = new[] { classDeclaration };
			}
			else
			{
				var registrarCallbacks = GenerateRegistrarCallbacks();
				classDeclaration = classDeclaration.AddMembers(registrarCallbacks);
				classesToNest = new[] { classDeclaration };
			}

			var topLevelClass = _classInfo.Type;
			while (topLevelClass.ContainingType is not null) {
				topLevelClass = topLevelClass.ContainingType;
				classDeclaration = ClassDeclaration(topLevelClass.Name)
					.AddModifiers(Token(SyntaxKind.PartialKeyword))
					.AddMembers(classesToNest);

				classesToNest = new[] { classDeclaration };
			}

			return NamespaceDeclaration(IdentifierName(_classInfo.Type.ContainingNamespace.Name))
				.AddMembers(classDeclaration);
		}

		private bool HasNativeHandleConstructor()
			=> _classInfo.Type.GetMembers()
				.Where(member => member is IMethodSymbol)
				.Cast<IMethodSymbol>()
				.Any(method =>
					method.MethodKind == MethodKind.Constructor
					&& !method.IsStatic
					&& method.Parameters.Length == 1
					&& method.Parameters[0].Type.Name == "NativeHandle"
					&& method.Parameters[0].Type.ContainingNamespace.Name == "ObjCRuntime"
					&& method.Parameters[0].Type.ContainingType is null);

		private bool HasINativeObjectConstructor()
			=> _classInfo.Type.GetMembers()
				.Where(member => member is IMethodSymbol)
				.Cast<IMethodSymbol>()
				.Any(method =>
					method.MethodKind == MethodKind.Constructor
					&& !method.IsStatic
					&& method.Parameters.Length == 2
					&& method.Parameters[0].Type.Name == "NativeHandle"
					&& method.Parameters[0].Type.ContainingNamespace.Name == "ObjCRuntime"
					&& method.Parameters[0].Type.ContainingType is null
					&& method.Parameters[1].Type.Name == "Boolean"
					&& method.Parameters[1].Type.ContainingNamespace.Name == "System"
					&& method.Parameters[1].Type.ContainingType is null);

		private bool IsCustomType()
			=> _classInfo.Attribute is null || (!_classInfo.Attribute.IsWrapper && !_classInfo.Attribute.SkipRegistration);

		// TODO: dynamic dependency attribute
		private ClassDeclarationSyntax GenerateRegistrarCallbacks(string className = RegistrarCallbacksName)
			=> ClassDeclaration(className)
				.AddModifiers(
					Token(SyntaxKind.PrivateKeyword),
					Token(SyntaxKind.StaticKeyword))
				.AddMembers(GenerateGetDotnetType())
				.AddMembers(GenerateCreateManagedInstance())
				.AddMembers(GenerateExportedMethodCallbacks());

		private MethodDeclarationSyntax GenerateGetDotnetType()
			=> CreateUnmanagedCallersOnlyMethod(
				returnType: SpecialTypeToSyntax(SpecialType.System_IntPtr),
				isVoid: false,
				name: "GetDotnetType",
				// TODO for generic types we need to change `typeof(T)` to:
				//    typeof(T<NSObject, ..., NSObject>).GetGenericTypeDefinition()
				createBody: () =>
					Block(
						ReturnStatement(
							RuntimeAllocGCHandleExpression(
								TypeOfExpression(IdentifierName(GetFullName(_classInfo.Type)))))));

		private MethodDeclarationSyntax GenerateGetDotnetType()
			=> CreateUnmanagedCallersOnlyMethod(
				returnType: SpecialTypeToSyntax(SpecialType.System_IntPtr),
				isVoid: false,
				name: "CreateManagedInstance",
				// TODO for generic types we need to change `typeof(T)` to `typeof(T<,,,>)`
				createBody: () =>
				{
					if (_classInfo.Type.IsGenericType) {
						return ThrowStatement(
							ObjectCreationExpression(
								IdentifierName("global::System.NotSupportedException"))
							.WithArgumentList(
								ArgumentList(
									SingletonSeparatedList<ArgumentSyntax>(
										Argument(
											LiteralExpression(
												SyntaxKind.StringLiteralExpression,
												Literal("Generic types cannot be instantiated from native code")))))));
					}

					if (HasNativeHandleConstructor()) {
						return Block(
							ReturnStatement(
								RuntimeAllocGCHandleExpression(
									ObjectCreationExpression(
										IdentifierName(GetFullName(_classInfo.Type)))
									.WithArgumentList(
										ArgumentList(
											SingletonSeparatedList<ArgumentSyntax>(
												Argument(
													IdentifierName("nativeHandle"))))))));
					}

					if (HasINativeObjectConstructor()) {
						return Block(
							ReturnStatement(
								RuntimeAllocGCHandleExpression(
									ObjectCreationExpression(
										IdentifierName(GetFullName(_classInfo.Type)))
									.WithArgumentList(
										ArgumentList(
											SeparatedList<ArgumentSyntax>(
												new SyntaxNodeOrToken[]{
													Argument(
														IdentifierName("nativeHandle")),
													Token(SyntaxKind.CommaToken),
													Argument(
														IdentifierName("owns"))}))))));
					}

					return ThrowStatement(
						ObjectCreationExpression(
							IdentifierName("global::System.NotSupportedException"))
						.WithArgumentList(
							ArgumentList(
								SingletonSeparatedList<ArgumentSyntax>(
									Argument(
										LiteralExpression(
											SyntaxKind.StringLiteralExpression,
											Literal($"Type {_classInfo.Type.Namespace}.{_classInfo.Type.Name} does not have a suitable constructor to be instantiated from native code")))))));
				});

		private static string GenerateTypeOfExpression(INamedTypeSymbol type)
			=> TypeOfExpression(
				IdentifierName(
					_classInfo.Type.IsGenericType
						? GetFullName(_classInfo.Type)
						: $"{GetFullName(_classInfo.Type)}<{new string(',', _classInfo.Type.TypeArguments.Count - 1)}>"));

		private MemberDeclarationSyntax[] GenerateExportedMethodCallbacks()
			=> _classInfo.ExportedMembers.Select(GenerateExportedMethodCallback).ToArray();

		private MemberDeclarationSyntax GenerateExportedMethodCallback(ExportedMemberInfo memberInfo)
		{
			var method = (IMethodSymbol)memberInfo.Symbol;
			if (method.MethodKind == MethodKind.Constructor)
			{
				return GenerateExportedConstructor(method, memberInfo);
			}

			var nativeClassName = new NativeClass(_classInfo.Type, _classInfo.Attribute).Name;
			return CreateUnmanagedCallersOnlyMethod(
				returnType: GetNativeTypeForManagedType(method.ReturnType),
				isVoid: method.ReturnsVoid,
				name: NameMangler.GetRegistrarCallbackIdentifier(nativeClassName, memberInfo),
				createParams: () => CloneParameters(method).ToArray(),
				createBody: () => {
					// TODO check AssociatedSymbol if it's a property getter/setter and change the impl accordingly
					// TODO what about generic methods? are those supported?
					return method.IsStatic
						? GenerateStaticMethodCall(method, memberInfo)
						: GenerateInstanceMethodCall(method, memberInfo);
				});
		}
		
		private BlockSyntax GenerateInstanceMethodCall(IMethodSymbol method, ExportedMemberInfo memberInfo)
			// var obj = Runtime.GetNSObject<T>(pobj);
			=> Block(
				VariableInitializationStatement(
					variableType: _classInfo.Type,
					variableName: "obj",
					GetNSObjectExpression(IdentifierName("pobj"), _classInfo.Type)))
				.AddStatements(GenerateCallMethod(IdentifierName("obj"), method).ToArray());

		private BlockSyntax GenerateStaticMethodCall(IMethodSymbol method, ExportedMemberInfo memberInfo)
			=> Block(GenerateCallMethod(IdentifierName(GetFullName(_classInfo.Type)), method));

		private MethodDeclarationSyntax GenerateExportedConstructor(IMethodSymbol method, ExportedMemberInfo memberInfo)
		{
			var nativeClassName = new NativeClass(_classInfo.Type, _classInfo.Attribute).Name;
			return CreateUnmanagedCallersOnlyMethod(
					returnType: SpecialTypeToSyntax(SpecialType.System_IntPtr),
					isVoid: false,
					isConstructor: true,
					name: NameMangler.GetRegistrarCallbackIdentifier(nativeClassName, memberInfo),
					createParams: () => CloneParameters(method).ToArray(),
					// if (Runtime.HasNSObject(pobj) != 0)
					// {
					//     *call_super = 1;
					//     return pobj;
					// }
					//
					// T obj = NSObject.CreateNSObject<T>(pobj);
					// // TODO convert params
					// ctor(obj, _p0, ...);
					// return NativeObjectExtensions.GetHandle(obj);
					createBody: () =>
						Block(
							// if (Runtime.HasNSObject(pobj) != 0)
							IfStatement(
								ParseExpression("global::ObjCRuntime.Runtime.HasNSObject(pobj) != 0"),
								Block(
									ExpressionStatement(
										AssignmentExpression(
											SyntaxKind.SimpleAssignmentExpression,
											PrefixUnaryExpression(
												SyntaxKind.PointerIndirectionExpression,
												IdentifierName("call_super")),
											LiteralExpression(
												SyntaxKind.NumericLiteralExpression,
												Literal(1)))),
									ReturnStatement(
										IdentifierName("pobj")))),

								// T obj = NSObject.CreateNSObject<T> (pobj);
								VariableInitializationStatement(
									_classInfo.Type,
									"obj",
									CreateNSObjectExpression(
										IdentifierName("pobj"),
										_classInfo.Type)),

								// ctor(obj, p0, p1, ...)
								// TODO convert all params
								// TODO pass all params
								ExpressionStatement(
									InvocationExpression(
										IdentifierName("ctor"))
									.WithArgumentList(
										ArgumentList(
											SingletonSeparatedList<ArgumentSyntax>(
													Argument(
														IdentifierName("obj")))))),

								// return NativeObjectExtensions.GetHandle(obj);
								ParseStatement($"return global::ObjCRuntime.NativeObjectExtensions.GetHandle(obj);"),

								// [UnsafeAccessor(UnsafeAccessorKind.Method, Name = ".ctor")]
								// extern static void ctor(T @this, T0 p0, ...);
								LocalFunctionStatement(
									PredefinedType(
										Token(SyntaxKind.VoidKeyword)),
									Identifier("ctor"))
										.WithAttributeLists(
											SingletonList<AttributeListSyntax>(
												AttributeList(
													SingletonSeparatedList<AttributeSyntax>(
														Attribute(
															IdentifierName("global::System.Runtime.CompilerServices.UnsafeAccessor"))
														.WithArgumentList(
															AttributeArgumentList(
																SeparatedList<AttributeArgumentSyntax>(
																	new SyntaxNodeOrToken[]{
																		AttributeArgument(
																			MemberAccessExpression(
																				SyntaxKind.SimpleMemberAccessExpression,
																				IdentifierName("global::System.Runtime.CompilerServices.UnsafeAccessorKind"),
																				IdentifierName("Method"))),
																		Token(SyntaxKind.CommaToken),
																		AttributeArgument(
																			LiteralExpression(
																				SyntaxKind.StringLiteralExpression,
																				Literal(".ctor")))
																		.WithNameEquals(
																			NameEquals(
																				IdentifierName("Name")))})))))))
										.AddModifiers(
											Token(SyntaxKind.ExternKeyword),
											Token(SyntaxKind.StaticKeyword))
										.WithParameterList(
											ParameterList(
												SeparatedList<ParameterSyntax>(
													new SyntaxNodeOrToken[]{
														Parameter(
															Identifier("@this"))
														.WithType(
															IdentifierName(GetFullName(_classInfo.Type))),})))
										.WithSemicolonToken(
											Token(SyntaxKind.SemicolonToken))));
		}

		private MethodDeclarationSyntax CreateUnmanagedCallersOnlyMethod(
			TypeSyntax returnType,
			bool isVoid,
			string name,
			Func<BlockSyntax> createBody,
			bool isConstructor = false,
			Func<ParameterSyntax[]>? createParams = null)
			=> MethodDeclaration(returnType, name)
				// [UnmanagedCallersOnly(EntryPoint = "_<name>")]
				.AddAttributeLists(
					AttributeList(
						SingletonSeparatedList<AttributeSyntax>(
								Attribute(IdentifierName(TypeNames.System_Runtime_InteropServices_UnmanagedCallersOnlyAttribute))
									.AddArgumentListArguments(
										AttributeArgument(
											nameEquals: NameEquals("EntryPoint"),
											nameColon: null,
											LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"_{name}")))))))
				// private static unsafe <name>(IntPtr pobj, IntPtr sel, <createParams>, IntPtr* exception_gchandle) {
				.AddModifiers(
					Token(SyntaxKind.PrivateKeyword),
					Token(SyntaxKind.StaticKeyword),
					Token(SyntaxKind.UnsafeKeyword))
				.AddParameterListParameters(
					Parameter(Identifier("pobj")).WithType(ParseTypeName(TypeNames.System_IntPtr)), // TODO skip pobj for static methods?
					Parameter(Identifier("sel")).WithType(ParseTypeName(TypeNames.System_IntPtr)))
				.AddParameterListParameters(createParams?.Invoke() ?? Array.Empty<ParameterSyntax>())
				.AddParameterListParameters(isConstructor
					? new ParameterSyntax[] { Parameter(Identifier("call_super")).WithType(PointerType(ParseTypeName(TypeNames.System_Byte))) }
					: Array.Empty<ParameterSyntax>())
				.AddParameterListParameters(
					Parameter(Identifier("exception_gchandle")).WithType(PointerType(ParseTypeName(TypeNames.System_IntPtr))))
				.WithBody(
					Block(
						// try {
						TryStatement()
							.WithBlock(createBody())
							.AddCatches(
								// } catch (Exception ex) {
								CatchClause()
									.WithDeclaration(
										CatchDeclaration(
											ParseName(TypeNames.System_Exception),
											Identifier("ex")))
									.WithBlock(
										Block(
											// *exception_gchandle = Runtime.AllocGCHandle(ex);
											ParseStatement("*exception_gchandle = global::ObjCRuntime.Runtime.AllocGCHandle(ex);"),
											// return default;
											ReturnStatement(isVoid ? null : ParseExpression("default")))))));

		private static IEnumerable<ParameterSyntax> CloneParameters(IMethodSymbol method)
			=> method.Parameters.Select(parameter =>
				Parameter(Identifier(ParameterName(parameter)))
					.WithType(UnmanagedCallersOnlyParameterType(parameter)));

		private static string ParameterName(IParameterSymbol parameter)
			=> $"p{parameter.Ordinal}_{parameter.Name}";

		// The idea is to use this for both the "regular" callback and "proxy" callback:
		// - the Runtime.GetNSObject<T>(pobj, ...) will be fetched elsewhere
		// - all remaining conversions will be performed here
		// - for reguralr callbacks the obj identifier will be "obj"
		// - for proxy callbacks the obj identifier will be "this"
		private static IEnumerable<StatementSyntax> GenerateCallMethod(IdentifierNameSyntax targetIdentifier, IMethodSymbol method)
		{
			// TODO what if the method is generic? is that even supported?
			var invocation = InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					targetIdentifier,
					IdentifierName(method.Name)));

			foreach (var parameter in method.Parameters)
			{
				var parameterName = ParameterName(parameter);
				if (ParameterNeedsConversion(parameter))
				{
					var localVariableName = $"_{parameterName}";
					yield return VariableInitializationStatement(
						parameter.Type,
						localVariableName,
						PrepareUnamangedCallersOnlyParameterForCallingOriginalMethodExpression(parameter, parameterName));
					parameterName = localVariableName;
				}

				// add the param to the invocation list
				var argument = Argument(IdentifierName(parameterName));
				if (parameter.RefKind != RefKind.None)
					argument = argument.WithRefKindKeyword(RefKindKeyword(parameter.RefKind));

				invocation = invocation.AddArgumentListArguments(argument);
			}

			if (method.ReturnsVoid)
			{
				// ExpotedMethod(_p0, ...);
				yield return ExpressionStatement(invocation);
			}
			else
			{
				// TReturn rv = ExportedMethod(_p0, ...);
				yield return LocalDeclarationStatement(
					VariableDeclaration(
						TypeName(method.ReturnType))
					.WithVariables(
						SingletonSeparatedList<VariableDeclaratorSyntax>(
							VariableDeclarator(
								Identifier("rv"))
							.WithInitializer(
								EqualsValueClause(invocation)))));
			}

			foreach (var parameter in method.Parameters)
			{
				if (parameter.RefKind != RefKind.None && parameter.RefKind != RefKind.In)
				{
					// *pN_X = _pN_X;
					yield return ExpressionStatement(
						AssignmentExpression(
							SyntaxKind.SimpleAssignmentExpression,
							PrefixUnaryExpression(
								SyntaxKind.PointerIndirectionExpression,
								IdentifierName(ParameterName(parameter))),
							IdentifierName($"_{ParameterName(parameter)}")));
				}
			}

			if (!method.ReturnsVoid)
			{
				yield return ReturnStatement(CastReturnValueIfNeeded(method.ReturnType, "rv"));
			}
		}

		private static BlockSyntax GenerateStaticMethodCall(IMethodSymbol method)
		{
			// TODO
			return Block();
		}

		private static BlockSyntax GeneratePropertyGetter(IPropertySymbol property, SyntaxToken? objectIdentifier)
		{
			// TODO
			return Block();
		}
		
		private static BlockSyntax GeneratePropertySetter(IPropertySymbol property, SyntaxToken? objectIdentifier)
		{
			// TODO
			return Block();
		}

		// TODO can I unify instance and static property setters/getters?
		private static BlockSyntax GenerateStaticPropertyGetter(IPropertySymbol property)
		{
			// TODO
			return Block();
		}
		
		private static BlockSyntax GenerateStaticPropertySetter(IPropertySymbol property)
		{
			// TODO
			return Block();
		}
	}
}
