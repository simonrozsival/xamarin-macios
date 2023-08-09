using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using Registrar;
using System.Globalization;

#nullable enable

namespace Xamarin.Linker {
	public class ManagedRegistrarLookupTablesStep : ConfigurationAwareStep {
		protected override string Name { get; } = "ManagedRegistrarLookupTables";
		protected override int ErrorCode { get; } = 2440;

		protected override void TryProcessAssembly (AssemblyDefinition assembly)
		{
			base.TryProcessAssembly (assembly);
		}
	}
}

