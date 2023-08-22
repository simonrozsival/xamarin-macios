using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal sealed class NativeCodeGenerator
	{
		private readonly NativeClass _nativeClass;
		private readonly HashSet<NativeClass> _dependencies = new();

		public NativeCodeGenerator(RegisteredClassInfo classInfo)
		{
			_nativeClass = new NativeClass(classInfo.Type, classInfo.Attribute);
			_dependencies.Add(_nativeClass.GetSuperClass());
		}

		public string HeaderFileName => $"{_nativeClass.Name}.h";
		public string ImplementationFileName => $"{_nativeClass.Name}.mm";

		// TODO also generate a list of frameworks that we must link with

		public string GenerateHeader(CancellationToken cancellationToken)
		{
			// TODO generate the interface first and then generate the includes
			// based on the dependencies of the body

			var exportedMethodSignatures = _nativeClass.ExportedMembers.Select(GenerateExportedMethodSignature).Select(signature => $"{signature};");
			var exportedMemberSignaturesBlocks = string.Join("\n", exportedMethodSignatures);

			var xamarinTrampolines = _nativeClass.IsFirstNonWrapper
				? """
					-(void) release;
					-(id) retain;
					-(GCHandle) xamarinGetGCHandle;
					-(bool) xamarinSetGCHandle: (GCHandle) gchandle flags: (enum XamarinGCHandleFlags) flags;
					-(enum XamarinGCHandleFlags) xamarinGetFlags;
					-(void) xamarinSetFlags: (enum XamarinGCHandleFlags) flags;

					"""
				: string.Empty;

			return $$"""
				#ifndef {{HeaderConstantName}}
				#define {{HeaderConstantName}}
				
				#pragma clang diagnostic ignored "-Wdeprecated-declarations"
				#pragma clang diagnostic ignored "-Wtypedef-redefinition"
				#pragma clang diagnostic ignored "-Wobjc-designated-initializers"
				#pragma clang diagnostic ignored "-Wunguarded-availability-new"
				#define DEBUG 1 // TODO

				#include <stdarg.h>
				#include <objc/objc.h>
				#include <objc/runtime.h>
				#include <objc/message.h>

				{{GenerateIncludes()}}
				{{GenerateClassDeclarations()}}

				@interface {{_nativeClass.Name}} : {{_nativeClass.GetSuperClassNameWithProtocols()}} {}
					{{Indent(xamarinTrampolines, indents: 1)}}
					{{Indent(exportedMemberSignaturesBlocks, indents: 1)}}
				@end

				#endif // ifndef {{HeaderConstantName}}
				""";
		}

		public string GenerateImplementation(CancellationToken cancellationToken)
		{
			var exportedMethodImplementations = _nativeClass.ExportedMembers.Select(GenerateExportedMethodDefinition);
			var exportedMethodImplementationsBlocks = string.Join("\n", exportedMethodImplementations);

			var xamarinTrampolines = _nativeClass.IsFirstNonWrapper
				? """
					-(void) release { xamarin_release_trampoline (self, _cmd); }
					-(id) retain { return xamarin_retain_trampoline (self, _cmd); }
					-(GCHandle) xamarinGetGCHandle { return __monoObjectGCHandle.gc_handle; }

					-(bool) xamarinSetGCHandle: (GCHandle) gc_handle flags: (enum XamarinGCHandleFlags) flags
					{
						if (((flags & XamarinGCHandleFlags_InitialSet) == XamarinGCHandleFlags_InitialSet) && __monoObjectGCHandle.gc_handle != INVALID_GCHANDLE) {
							return false;
						}
						flags = (enum XamarinGCHandleFlags) (flags & ~XamarinGCHandleFlags_InitialSet);
						__monoObjectGCHandle.gc_handle = gc_handle;
						__monoObjectGCHandle.flags = flags;
						__monoObjectGCHandle.native_object = self;
						return true;
					}

					-(enum XamarinGCHandleFlags) xamarinGetFlags { return __monoObjectGCHandle.flags; }
					-(void) xamarinSetFlags: (enum XamarinGCHandleFlags) flags { __monoObjectGCHandle.flags = flags; }

					"""
				: string.Empty;

			return $$"""
				#include <xamarin/xamarin.h>
				#include "{{HeaderFileName}}"
				#define DEBUG 1 // TODO

				extern "C" {

				#pragma clang diagnostic push
				#pragma clang diagnostic ignored "-Wprotocol"
				#pragma clang diagnostic ignored "-Wobjc-protocol-property-synthesis"
				#pragma clang diagnostic ignored "-Wobjc-property-implementation"
				@implementation {{_nativeClass.Name}} {
					{{(_nativeClass.IsFirstNonWrapper ? "XamarinObject __monoObjectGCHandle" : "")}};
				}
					{{Indent(xamarinTrampolines, indents: 1)}}
					{{Indent(exportedMethodImplementationsBlocks, indents: 1)}}
				@end
				#pragma clang diagnostic pop

				} // extern "C"
				""";
		}

		private string GenerateIncludes()
		{
			var builder = new StringBuilder();

			foreach (var dependency in _dependencies)
			{
				if (dependency.GetFramework() is string framework)
				{
					builder.AppendLine($"#import <{framework}/{framework}.h>");
				}
				else
				{
					// TODO will this always be in the same folder?
					builder.AppendLine($"#include <{dependency.Name}.h>");
				}
			}

			return builder.ToString();
		}

		private string GenerateClassDeclarations()
		{
			return string.Empty;
		}

		private string GenerateExportedMethodSignature(ExportedMemberInfo exportedMember)
		{
			if (exportedMember.Symbol is IMethodSymbol method)
			{
				var selectorParts = exportedMember.Attribute.Selector.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (selectorParts.Length == 0)
				{
					throw new Exception($"Invalid selector {exportedMember.Attribute.Selector}");
				}

				var builder = new StringBuilder();
				builder.Append(method.IsStatic ? "+" : "-");
				builder.Append($"({GetNativeTypeName(method.ReturnType)})");

				for (int i = 0; i < selectorParts.Length; i++)
				{
					builder.Append(" ");
					builder.Append(selectorParts[i]);
					if (method.Parameters.Length > i)
					{
						builder.Append($":({GetNativeParameterName(method.Parameters[i])})p{i}");
					}
				}

				return builder.ToString();
			}
			else
			{
				throw new NotImplementedException($"we're missing implementation for {exportedMember.Symbol}");
			}
		}

		private string GetNativeParameterName(IParameterSymbol parameter)
		{
			var typeName = GetNativeTypeName(parameter.Type);
			return parameter.RefKind == RefKind.None
				? typeName
				: $"{typeName}*";
		}

		private string GetNativeTypeName(ITypeSymbol type)
		{
			if (!type.IsReferenceType)
			{
				return type.SpecialType switch
				{
					SpecialType.System_Boolean => $"BOOL",
					SpecialType.System_Byte => $"char",
					SpecialType.System_Char => $"char",
					SpecialType.System_Decimal => $"long", // ??
					SpecialType.System_Double => $"double",
					SpecialType.System_Int16 => $"short",
					SpecialType.System_Int32 => $"int",
					SpecialType.System_Int64 => $"long long",
					SpecialType.System_IntPtr => $"id", // ??
					SpecialType.System_SByte => $"sbyte", // ??
					SpecialType.System_Single => $"float",
					SpecialType.System_UInt16 => $"unsigned short",
					SpecialType.System_UInt32 => $"unsigned int",
					SpecialType.System_UInt64 => $"unsigned long long",
					SpecialType.System_UIntPtr => $"id", // ??
					SpecialType.System_Void => $"void",
					_ => throw new Exception($"we're missing special type conversion for {type.SpecialType}"),
				};
			}
			else if (type is INamedTypeSymbol namedType)
			{
				var nativeClass = new NativeClass(namedType);
				if (nativeClass.Name != _nativeClass.Name)
				{
					_dependencies.Add(nativeClass);
				}

				return $"{nativeClass.Name}*";
			}

			throw new NotImplementedException($"Cannot get native type for {type}");
		}
		
		private string GenerateExportedMethodDeclaration(ExportedMemberInfo exportedMember)
		{
			return string.Empty;
		}

		private string GenerateExportedMethodDefinition(ExportedMemberInfo exportedMember)
		{
			if (exportedMember.Symbol is IMethodSymbol method)
			{
				var callbackName = NameMangler.GetRegistrarCallbackIdentifier(_nativeClass.Name, exportedMember);

				var builder = new StringBuilder();
				builder.AppendLine($"{GenerateCallbackMethodSignature(exportedMember)};");
				builder.AppendLine($"{GenerateExportedMethodSignature(exportedMember)} {{");
				builder.AppendLine($"\tGCHandle exception_gchandle = INVALID_GCHANDLE;");
				if (method.ReturnsVoid && method.MethodKind != MethodKind.Constructor)
				{
					builder.Append("\t");
					AppendCallToCallback(builder, callbackName, method);
					builder.AppendLine(";");
				}
				else
				{
					builder.Append("\t");
					if (method.MethodKind == MethodKind.Constructor)
					{
						builder.Append("id");
					}
					else
					{
						builder.Append(GetNativeTypeName(method.ReturnType));
					}
					builder.AppendLine(" rv = { 0 };");

					builder.Append("\trv = ");
					AppendCallToCallback(builder, callbackName, method);
					builder.AppendLine(";");
				}
				builder.AppendLine("\txamarin_process_managed_exception_gchandle(exception_gchandle);");

				if (method.MethodKind == MethodKind.Constructor)
				{
					builder.AppendLine("\tif (call_super && rv) {");
					builder.AppendLine($"\t\tstruct objc_super super = {{ rv, [{_nativeClass.GetSuperClass().Name} class] }};");
					builder.Append($"\t\trv = ((id (*)(objc_super*, SEL");
					foreach (var parameter in method.Parameters)
					{
						builder.Append($", {GetNativeParameterName(parameter)}");
					}
					builder.Append($")) objc_msgSendSuper) (&super, @selector ({exportedMember.Attribute.Selector})");
					for (int i = 0; i < method.Parameters.Length; i++)
					{
						builder.Append($", p{i}");
					}
					builder.AppendLine(");");
					builder.AppendLine("\t}");
				}

				if (!method.ReturnsVoid || method.MethodKind == MethodKind.Constructor)
				{
					builder.AppendLine($"\treturn rv;");
				}

				builder.AppendLine("}");
				return builder.ToString();
			}
			else
			{
				throw new NotImplementedException($"missing method definition for {exportedMember.Symbol}");
			}

			static void AppendCallToCallback(StringBuilder builder, string callbackName, IMethodSymbol method)
			{
				builder.Append(callbackName);
				builder.Append("(self, _cmd");

				for (int i = 0; i < method.Parameters.Length; i++)
				{
					builder.Append($", p{i}");
				}

				if (method.MethodKind == MethodKind.Constructor)
				{
					builder.Append(", &call_super");
				}

				builder.Append(", &exception_gchandle)");
			}
		}

		private string GenerateCallbackMethodSignature(ExportedMemberInfo exportedMember)
		{
			if (exportedMember.Symbol is IMethodSymbol method)
			{
				var builder = new StringBuilder();

				if (method.MethodKind == MethodKind.Constructor)
				{
					builder.Append("id");
				}
				else
				{
					builder.Append(GetNativeTypeName(method.ReturnType));
				}

				builder.Append(" ");
				builder.Append(NameMangler.GetRegistrarCallbackIdentifier(_nativeClass.Name, exportedMember));
				builder.Append("(id self, SEL sel");

				for (int i = 0; i < method.Parameters.Length; i++)
				{
					builder.Append($", {GetNativeParameterName(method.Parameters[i])} p{i}");
				}

				if (method.MethodKind == MethodKind.Constructor)
				{
					builder.Append(", bool* call_super");
				}

				builder.Append(", GCHandle* exception_gchandle)");
				return builder.ToString();
			}
			else
			{
				throw new NotImplementedException($"we're missing implementation for {exportedMember.Symbol}");
			}
		}

		private static string Indent(string block, int indents)
			=> string.Join($"\n{new string('\t', indents)}", block.Split('\n'));

		private string HeaderConstantName => $"__{_nativeClass.Name.ToUpper()}__";
	}
}
