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
			if (modified) {
				DerivedLinkContext.Annotations.SetCustomAnnotation ("ManagedRegistrarStep", assembly, current_trampoline_lists);
				abr.SaveCurrentAssembly ();
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
	}
}
