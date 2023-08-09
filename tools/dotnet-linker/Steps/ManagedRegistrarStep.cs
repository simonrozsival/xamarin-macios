using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Xamarin.Bundler;
using Xamarin.Utils;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Tuner;

using ObjCRuntime;
using Registrar;
using System.Globalization;

#nullable enable

namespace Xamarin.Linker {
	// Class to contain (trampoline) info for every assembly in the app bundle
	public class AssemblyTrampolineInfos : Dictionary<AssemblyDefinition, AssemblyTrampolineInfo> {
		Dictionary<MethodDefinition, TrampolineInfo>? map;
		public bool TryFindInfo (MethodDefinition method, [NotNullWhen (true)] out TrampolineInfo? info)
		{
			if (map is null) {
				map = new Dictionary<MethodDefinition, TrampolineInfo> ();
				foreach (var kvp in this) {
					foreach (var ai in kvp.Value) {
						map.Add (ai.Target, ai);
					}
				}
			}
			return map.TryGetValue (method, out info);
		}
	}

	// Class to contain all the trampoline infos for an assembly
	// Also between a type and its ID.
	public class AssemblyTrampolineInfo : List<TrampolineInfo> {
		public TypeDefinition? RegistrarType;
	}

	// Class to contain info for each exported method, with its UCO trampoline.
	public class TrampolineInfo {
		public MethodDefinition Trampoline;
		public MethodDefinition Target;
		public string UnmanagedCallersOnlyEntryPoint;
		public int Id;

		public TrampolineInfo (MethodDefinition trampoline, MethodDefinition target, string entryPoint)
		{
			this.Trampoline = trampoline;
			this.Target = target;
			this.UnmanagedCallersOnlyEntryPoint = entryPoint;
			this.Id = -1;
		}
	}

	public class ManagedRegistrarStep : ConfigurationAwareStep {
		protected override string Name { get; } = "ManagedRegistrar";
		protected override int ErrorCode { get; } = 2430;

		AppBundleRewriter abr { get { return Configuration.AppBundleRewriter; } }

		protected override void TryProcess ()
		{
			base.TryProcess ();

			App.SelectRegistrar ();
			if (App.Registrar != RegistrarMode.ManagedStatic)
				return;

			Configuration.Target.StaticRegistrar.Register (Configuration.GetNonDeletedAssemblies (this));
		}

		protected override void TryEndProcess (out List<Exception>? exceptions)
		{
			base.TryEndProcess ();
			exceptions = null;
		}

		protected override void TryProcessAssembly (AssemblyDefinition assembly)
		{
			base.TryProcessAssembly (assembly);

			if (App.Registrar != RegistrarMode.ManagedStatic)
				return;

			if (Annotations.GetAction (assembly) == AssemblyAction.Delete)
				return;

			// No SDK assemblies will have anything we need to register
			if (Configuration.Profile.IsSdkAssembly (assembly))
				return;

			if (!assembly.MainModule.HasAssemblyReferences)
				return;

			// In fact, unless an assembly references our platform assembly, then it won't have anything we need to register
			if (!Configuration.Profile.IsProductAssembly (assembly) && !assembly.MainModule.AssemblyReferences.Any (v => Configuration.Profile.IsProductAssembly (v.Name)))
				return;

			if (!assembly.MainModule.HasTypes)
				return;

			abr.SetCurrentAssembly (assembly);

			var current_trampoline_lists = new AssemblyTrampolineInfo ();
			Configuration.AssemblyTrampolineInfos [assembly] = current_trampoline_lists;
			var proxyInterfaces = new List<TypeDefinition> ();

			var modified = false;
			foreach (var type in assembly.MainModule.Types)
				modified |= ProcessType (type, current_trampoline_lists, proxyInterfaces);

			foreach (var additionalType in proxyInterfaces)
				assembly.MainModule.Types.Add (additionalType);

			// Make sure the linker saves any changes in the assembly.
			DerivedLinkContext.Annotations.SetCustomAnnotation ("ManagedRegistrarStep", assembly, current_trampoline_lists);
			if (modified)
				abr.SaveCurrentAssembly ();

			if (App.XamarinRuntime == XamarinRuntime.MonoVM) {
				var md = abr.RegistrarHelper_RuntimeTypeHandleEquals.Resolve ();
				md.IsPublic = true;
				Annotations.Mark (md);
			}

			// TODO: Move this to a separate "MakeEverythingWorkWithNativeAOTStep" linker step
			if (App.XamarinRuntime == XamarinRuntime.NativeAOT && Configuration.Profile.IsProductAssembly (assembly)) {
				ImplementNSObjectRegisterToggleRefMethodStub ();
			}

			abr.ClearCurrentAssembly ();
		}

		bool ProcessType (TypeDefinition type, AssemblyTrampolineInfo infos, List<TypeDefinition> proxyInterfaces)
		{
			var modified = false;
			if (type.HasNestedTypes) {
				foreach (var nested in type.NestedTypes)
					modified |= ProcessType (nested, infos, proxyInterfaces);
			}

			// Figure out if there are any types we need to process
			var process = false;

			process |= IsNSObject (type);
			process |= StaticRegistrar.GetCategoryAttribute (type) is not null;

			var registerAttribute = StaticRegistrar.GetRegisterAttribute (type);
			if (registerAttribute is not null && registerAttribute.IsWrapper)
				return modified;

			if (!process)
				return modified;

			// Figure out if there are any methods we need to process
			var methods_to_wrap = new HashSet<MethodDefinition> ();
			if (type.HasMethods) {
				foreach (var method in type.Methods)
					ProcessMethod (method, methods_to_wrap);
			}

			if (type.HasProperties) {
				foreach (var prop in type.Properties) {
					ProcessProperty (prop, methods_to_wrap);
				}
			}

			// Create an UnmanagedCallersOnly method for each method we need to wrap
			foreach (var method in methods_to_wrap) {
				var name = $"callback_{Sanitize (method.DeclaringType.FullName)}_{Sanitize (method.Name)}";
				infos.Add (new TrampolineInfo (method, method, name));
			}

			return true;
		}

		void ProcessMethod (MethodDefinition method, HashSet<MethodDefinition> methods_to_wrap)
		{
			if (!(method.IsConstructor && !method.IsStatic)) {
				var ea = StaticRegistrar.GetExportAttribute (method);
				if (ea is null && !method.IsVirtual)
					return;
			}

			if (!StaticRegistrar.TryFindMethod (method, out _)) {
				// If the registrar doesn't know about a method, then we don't need to generate an UnmanagedCallersOnly trampoline for it
				return;
			}

			methods_to_wrap.Add (method);
		}

		void ProcessProperty (PropertyDefinition property, HashSet<MethodDefinition> methods_to_wrap)
		{
			var ea = StaticRegistrar.GetExportAttribute (property);
			if (ea is null)
				return;

			if (property.GetMethod is not null)
				methods_to_wrap.Add (property.GetMethod);

			if (property.SetMethod is not null)
				methods_to_wrap.Add (property.SetMethod);
		}

		static string Sanitize (string str)
		{
			str = str.Replace ('.', '_');
			str = str.Replace ('/', '_');
			str = str.Replace ('`', '_');
			str = str.Replace ('<', '_');
			str = str.Replace ('>', '_');
			str = str.Replace ('$', '_');
			str = str.Replace ('@', '_');
			str = StaticRegistrar.EncodeNonAsciiCharacters (str);
			str = str.Replace ('\\', '_');
			return str;
		}

		bool IsNSObject (TypeReference type)
		{
			if (type is ArrayType)
				return false;
			if (type is ByReferenceType)
				return false;
			if (type is PointerType)
				return false;
			if (type is GenericParameter)
				return false;
			return type.IsNSObject (DerivedLinkContext);
		}

		StaticRegistrar StaticRegistrar {
			get { return DerivedLinkContext.StaticRegistrar; }
		}

		CustomAttribute CreateUnmanagedCallersAttribute (string entryPoint)
		{
			var unmanagedCallersAttribute = new CustomAttribute (abr.UnmanagedCallersOnlyAttribute_Constructor);
			// Mono didn't prefix the entry point with an underscore until .NET 8: https://github.com/dotnet/runtime/issues/79491
			var entryPointPrefix = Driver.TargetFramework.Version.Major < 8 ? "_" : string.Empty;
			unmanagedCallersAttribute.Fields.Add (new CustomAttributeNamedArgument ("EntryPoint", new CustomAttributeArgument (abr.System_String, entryPointPrefix + entryPoint)));
			return unmanagedCallersAttribute;
		}

		void GenerateConversionToManaged (MethodDefinition method, ILProcessor il, TypeReference inputType, TypeReference outputType, string descriptiveMethodName, int parameter, out TypeReference nativeCallerType)
		{
			// This is a mirror of the native method xamarin_generate_conversion_to_managed (for the dynamic registrar).
			// It's also a mirror of the method StaticRegistrar.GenerateConversionToManaged.
			// These methods must be kept in sync.
			var managedType = outputType;
			var nativeType = inputType;

			var isManagedNullable = StaticRegistrar.IsNullable (managedType);

			var underlyingManagedType = managedType;
			var underlyingNativeType = nativeType;

			var isManagedArray = StaticRegistrar.IsArray (managedType);
			var isNativeArray = StaticRegistrar.IsArray (nativeType);

			nativeCallerType = abr.System_IntPtr;

			if (isManagedArray != isNativeArray)
				throw ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}");

			if (isManagedArray) {
				if (isManagedNullable)
					throw ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}");
				underlyingNativeType = StaticRegistrar.GetElementType (nativeType);
				underlyingManagedType = StaticRegistrar.GetElementType (managedType);
			} else if (isManagedNullable) {
				underlyingManagedType = StaticRegistrar.GetNullableType (managedType);
			}

			string? func = null;
			MethodReference? conversionFunction = null;
			MethodReference? conversionFunction2 = null;
			if (underlyingNativeType.Is ("Foundation", "NSNumber")) {
				func = StaticRegistrar.GetNSNumberToManagedFunc (underlyingManagedType, inputType, outputType, descriptiveMethodName, out var _);
			} else if (underlyingNativeType.Is ("Foundation", "NSValue")) {
				func = StaticRegistrar.GetNSValueToManagedFunc (underlyingManagedType, inputType, outputType, descriptiveMethodName, out var _);
			} else if (underlyingNativeType.Is ("Foundation", "NSString")) {
				if (!StaticRegistrar.IsSmartEnum (underlyingManagedType, out var getConstantMethod, out var getValueMethod)) {
					// method linked away!? this should already be verified
					AddException (ErrorHelper.CreateError (99, Errors.MX0099, $"the smart enum {underlyingManagedType.FullName} doesn't seem to be a smart enum after all"));
					return;
				}

				var gim = new GenericInstanceMethod (abr.Runtime_GetNSObject_T___System_IntPtr);
				gim.GenericArguments.Add (abr.Foundation_NSString);
				conversionFunction = gim;

				conversionFunction2 = abr.CurrentAssembly.MainModule.ImportReference (getValueMethod);
			} else {
				throw ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}");
			}

			if (func is not null) {
				conversionFunction = abr.GetMethodReference (abr.PlatformAssembly, abr.ObjCRuntime_BindAs, func, func, (v) =>
						v.IsStatic, out MethodDefinition conversionFunctionDefinition);
				EnsureVisible (method, conversionFunctionDefinition.DeclaringType);
			}

			if (isManagedArray) {
				il.Emit (OpCodes.Ldftn, conversionFunction);
				if (conversionFunction2 is not null) {
					il.Emit (OpCodes.Ldftn, conversionFunction2);
					var gim = new GenericInstanceMethod (abr.BindAs_ConvertNSArrayToManagedArray2);
					gim.GenericArguments.Add (underlyingManagedType);
					gim.GenericArguments.Add (abr.Foundation_NSString);
					il.Emit (OpCodes.Call, gim);
				} else {
					var gim = new GenericInstanceMethod (abr.BindAs_ConvertNSArrayToManagedArray);
					gim.GenericArguments.Add (underlyingManagedType);
					il.Emit (OpCodes.Call, gim);
				}
				nativeCallerType = abr.System_IntPtr;
			} else {
				if (isManagedNullable) {
					il.Emit (OpCodes.Ldftn, conversionFunction);
					if (conversionFunction2 is not null) {
						il.Emit (OpCodes.Ldftn, conversionFunction2);
						var gim = new GenericInstanceMethod (abr.BindAs_CreateNullable2);
						gim.GenericArguments.Add (underlyingManagedType);
						gim.GenericArguments.Add (abr.Foundation_NSString);
						il.Emit (OpCodes.Call, gim);
					} else {
						var gim = new GenericInstanceMethod (abr.BindAs_CreateNullable);
						gim.GenericArguments.Add (underlyingManagedType);
						il.Emit (OpCodes.Call, gim);
					}
					nativeCallerType = abr.System_IntPtr;
				} else {
					il.Emit (OpCodes.Call, conversionFunction);
					if (conversionFunction2 is not null)
						il.Emit (OpCodes.Call, conversionFunction2);
					nativeCallerType = abr.System_IntPtr;
				}
			}
		}

		void GenerateConversionToNative (MethodDefinition method, ILProcessor il, TypeReference inputType, TypeReference outputType, string descriptiveMethodName, out TypeReference nativeCallerType)
		{
			// This is a mirror of the native method xamarin_generate_conversion_to_native (for the dynamic registrar).
			// It's also a mirror of the method StaticRegistrar.GenerateConversionToNative.
			// These methods must be kept in sync.
			var managedType = inputType;
			var nativeType = outputType;

			var isManagedNullable = StaticRegistrar.IsNullable (managedType);

			var underlyingManagedType = managedType;
			var underlyingNativeType = nativeType;

			var isManagedArray = StaticRegistrar.IsArray (managedType);
			var isNativeArray = StaticRegistrar.IsArray (nativeType);

			nativeCallerType = abr.System_IntPtr;

			if (isManagedArray != isNativeArray)
				throw ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}");

			if (isManagedArray) {
				if (isManagedNullable)
					throw ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}");
				underlyingNativeType = StaticRegistrar.GetElementType (nativeType);
				underlyingManagedType = StaticRegistrar.GetElementType (managedType);
			} else if (isManagedNullable) {
				underlyingManagedType = StaticRegistrar.GetNullableType (managedType);
			}

			string? func = null;
			MethodReference? conversionFunction = null;
			MethodReference? conversionFunction2 = null;
			MethodReference? conversionFunction3 = null;
			if (underlyingNativeType.Is ("Foundation", "NSNumber")) {
				func = StaticRegistrar.GetManagedToNSNumberFunc (underlyingManagedType, inputType, outputType, descriptiveMethodName);
			} else if (underlyingNativeType.Is ("Foundation", "NSValue")) {
				func = StaticRegistrar.GetManagedToNSValueFunc (underlyingManagedType, inputType, outputType, descriptiveMethodName);
			} else if (underlyingNativeType.Is ("Foundation", "NSString")) {
				if (!StaticRegistrar.IsSmartEnum (underlyingManagedType, out var getConstantMethod, out var getValueMethod)) {
					// method linked away!? this should already be verified
					ErrorHelper.Show (ErrorHelper.CreateError (99, Errors.MX0099, $"the smart enum {underlyingManagedType.FullName} doesn't seem to be a smart enum after all"));
					return;
				}

				conversionFunction = abr.CurrentAssembly.MainModule.ImportReference (getConstantMethod);
				conversionFunction2 = abr.NativeObjectExtensions_GetHandle;
				conversionFunction3 = abr.NativeObject_op_Implicit_IntPtr;
			} else {
				AddException (ErrorHelper.CreateError (99, Errors.MX0099, $"can't convert from '{inputType.FullName}' to '{outputType.FullName}' in {descriptiveMethodName}"));
				return;
			}

			if (func is not null) {
				conversionFunction = abr.GetMethodReference (abr.PlatformAssembly, abr.ObjCRuntime_BindAs, func, func, (v) =>
						v.IsStatic, out MethodDefinition conversionFunctionDefinition);
				EnsureVisible (method, conversionFunctionDefinition.DeclaringType);
			}

			if (isManagedArray) {
				il.Emit (OpCodes.Ldftn, conversionFunction);
				if (conversionFunction2 is not null) {
					il.Emit (OpCodes.Ldftn, conversionFunction2);
					var gim = new GenericInstanceMethod (abr.BindAs_ConvertManagedArrayToNSArray2);
					gim.GenericArguments.Add (underlyingManagedType);
					gim.GenericArguments.Add (abr.Foundation_NSString);
					il.Emit (OpCodes.Call, gim);
				} else {
					var gim = new GenericInstanceMethod (abr.BindAs_ConvertManagedArrayToNSArray);
					gim.GenericArguments.Add (underlyingManagedType);
					il.Emit (OpCodes.Call, gim);
				}
			} else {
				var tmpVariable = il.Body.AddVariable (managedType);

				var trueTarget = il.Create (OpCodes.Nop);
				var endTarget = il.Create (OpCodes.Nop);
				if (isManagedNullable) {
					il.Emit (OpCodes.Stloc, tmpVariable);
					il.Emit (OpCodes.Ldloca, tmpVariable);
					var mr = abr.System_Nullable_1.CreateMethodReferenceOnGenericType (abr.Nullable_HasValue, underlyingManagedType);
					il.Emit (OpCodes.Call, mr);
					il.Emit (OpCodes.Brtrue, trueTarget);
					il.Emit (OpCodes.Ldc_I4_0);
					il.Emit (OpCodes.Conv_I);
					il.Emit (OpCodes.Br, endTarget);
					il.Append (trueTarget);
					il.Emit (OpCodes.Ldloca, tmpVariable);
					il.Emit (OpCodes.Call, abr.System_Nullable_1.CreateMethodReferenceOnGenericType (abr.Nullable_Value, underlyingManagedType));
				}
				il.Emit (OpCodes.Call, conversionFunction);
				if (conversionFunction2 is not null) {
					il.Emit (OpCodes.Call, conversionFunction2);
					if (conversionFunction3 is not null)
						il.Emit (OpCodes.Call, conversionFunction3);
				}
				if (isManagedNullable)
					il.Append (endTarget);
			}
		}

		MethodDefinition CloneConstructorWithNativeHandle (MethodDefinition ctor)
		{
			var clonedCtor = new MethodDefinition (ctor.Name, ctor.Attributes, ctor.ReturnType);
			clonedCtor.IsPublic = false;

			// clone the original parameters firsts
			foreach (var parameter in ctor.Parameters) {
				clonedCtor.AddParameter (parameter.Name, parameter.ParameterType);
			}

			// add a native handle param + a dummy parameter that we know for a fact won't be used anywhere
			// to make the signature of the new constructor unique
			var handleParameter = clonedCtor.AddParameter ("nativeHandle", abr.System_IntPtr);
			var dummyParameter = clonedCtor.AddParameter ("dummy", abr.ObjCRuntime_IManagedRegistrar);

			var body = clonedCtor.CreateBody (out var il);

			// ensure visible
			abr.Foundation_NSObject_HandleField.Resolve ().IsFamily = true;
#if NET
			abr.Foundation_NSObject_FlagsSetterMethod.Resolve ().IsFamily = true;
#else
			abr.Foundation_NSObject_FlagsField.Resolve ().IsFamily = true;
#endif

			// store the handle and flags first
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldarg, handleParameter);
#if NET
			il.Emit (OpCodes.Call, abr.NativeObject_op_Implicit_NativeHandle);
#endif
			il.Emit (OpCodes.Stfld, abr.CurrentAssembly.MainModule.ImportReference (abr.Foundation_NSObject_HandleField));

			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldc_I4_2); // Flags.NativeRef == 2
#if NET
			il.Emit (OpCodes.Call, abr.Foundation_NSObject_FlagsSetterMethod);
#else
			il.Emit (OpCodes.Stfld, abr.Foundation_NSObject_FlagsField);
#endif

			// call the original constructor with all of the original parameters
			il.Emit (OpCodes.Ldarg_0);
			foreach (var parameter in clonedCtor.Parameters.SkipLast (2)) {
				il.Emit (OpCodes.Ldarg, parameter);
			}

			il.Emit (OpCodes.Call, ctor);
			il.Emit (OpCodes.Ret);

			return clonedCtor;
		}

		void ImplementNSObjectRegisterToggleRefMethodStub ()
		{
			// The NSObject.RegisterToggleRef method is a Mono icall that is unused in NativeAOT.
			// The method isn't included on all platforms but when it is present, we need to modify it
			// so that ILC can trim it and it doesn't report the following warning:
			// 
			//    ILC: Method '[Microsoft.iOS]Foundation.NSObject.RegisterToggleRef(NSObject,native int,bool)' will always throw because:
			//         Invalid IL or CLR metadata in 'Void Foundation.NSObject.RegisterToggleRef(Foundation.NSObject, IntPtr, Boolean)'
			//
			if (abr.TryGet_NSObject_RegisterToggleRef (out var registerToggleRef)) {
				registerToggleRef!.IsPublic = false;
				registerToggleRef!.IsInternalCall = false;

				registerToggleRef!.CreateBody (out var il);
				il.Emit (OpCodes.Ret);
			}
		}
	}
}
