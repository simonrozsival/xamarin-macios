using System;
using System.Collections.Generic;

namespace ObjCRuntime {
	internal static class ManagedRegistrarHelpers
	{
		// TODO how else can we create a deterministic ID for the type
		// that will be the same at build time and at runtime?
		//
		// - we could generate a random ID in a source generator and
		//   write it into the C# code (add a `uint TypeId` getter to `IManagedRegistrarType`)
		//   and write it also into the Objective-C code (they need to be generated together).

		internal static uint CalculateTypeId (Type type)
		{
			// we need to erase the generic type arguments so that all of these generic types map to the same Objective-C class
			if (type.IsGenericType) {
				type = type.GetGenericTypeDefinition ();
			}

			return CalculateTypeId (
				assemblyName: type.Assembly.GetName ().Name ?? "",
				typeName: type.FullName ?? "");
		}

		internal static uint CalculateTypeId (string assemblyName, string typeName) {
			// type names with square brackets contain the generic arguments types which are not acceptable
			if (typeName.IndexOf ('[') != -1) {
				Console.WriteLine ($"Calculating type ID for a type with a suspicious name: '{typeName}'");
			}

			var name = $"[{assemblyName}]{typeName}";
			return CalculateDeterministic24BitHash (name);
		}

		private static uint CalculateDeterministic24BitHash (string text)
		{
			uint hash = 23;
			foreach (char c in text) {
				hash = unchecked(hash * 31 + c);
			}

			return hash & 0xffffff;
		}
	}
}
