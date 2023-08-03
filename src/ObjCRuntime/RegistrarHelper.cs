//
// RegistrarHelper.cs: Helper code for the managed static registra.
//
// Authors:
//   Rolf Bjarne Kvinge
//
// Copyright 2023 Microsoft Corp


// #define TRACE

#if NET

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using CoreFoundation;
using CoreGraphics;
using Foundation;

using Registrar;

using Xamarin.Bundler;

namespace ObjCRuntime {
	// This class contains helper methods for the managed static registrar.
	// The managed static registrar will make it public when needed.
	public static class RegistrarHelper {
		class MapInfo {
			public ManagedRegistrar Registrar;
			public HashSet<RuntimeTypeHandle> RegisteredWrapperTypes;

			public MapInfo (RuntimeTypeHandleEqualityComparer comparer)
			{
				Registrar = new (comparer);
				RegisteredWrapperTypes = new (comparer);
			}
		}

		// Ignore CS8618 for these two variables:
		//     Non-nullable variable must contain a non-null value when exiting constructor.
		// Because we won't use a static constructor to initialize them, instead we're using a module initializer,
		// it's safe to ignore this warning.
#pragma warning disable 8618
		static Dictionary<string, MapInfo> assembly_map;
		static Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapper_types;
		static StringEqualityComparer StringEqualityComparer;
		static RuntimeTypeHandleEqualityComparer RuntimeTypeHandleEqualityComparer;
#pragma warning restore 8618

		static RegistrarHelper ()
		{
			StringEqualityComparer = new StringEqualityComparer ();
			RuntimeTypeHandleEqualityComparer = new RuntimeTypeHandleEqualityComparer ();
			assembly_map = new Dictionary<string, MapInfo> (StringEqualityComparer);
			wrapper_types = new Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> (RuntimeTypeHandleEqualityComparer);
		}

		static NativeHandle CreateCFArray (params string[]? values)
		{
			if (values is null)
				return NativeHandle.Zero;
			return CFArray.Create (values);
		}

		unsafe static IntPtr GetBlockPointer (BlockLiteral block)
		{
			var rv = BlockLiteral._Block_copy (&block);
			block.Dispose ();
			return rv;
		}

		static IntPtr GetBlockForDelegate (object @delegate, RuntimeMethodHandle method_handle)
		{
			var method = (MethodInfo) MethodBase.GetMethodFromHandle (method_handle)!;
			return BlockLiteral.GetBlockForDelegate (method, @delegate, Runtime.INVALID_TOKEN_REF, null);
		}

		internal static Type? FindProtocolWrapperType (Type type)
		{
			var typeHandle = type.TypeHandle;

			// First check if the type is already in our dictionary.
			if (wrapper_types.TryGetValue (typeHandle, out var wrapperType))
				return Type.GetTypeFromHandle (wrapperType);

			// Not in our dictionary, get the map entry to see if we've already
			// called RegisterWrapperType for this assembly,
			var map = GetOrInitMapInfo (type.Assembly);
			if (!map.RegisteredWrapperTypes.Contains (typeHandle)) {
				map.Registrar.RegisterWrapperType (typeHandle, wrapper_types);
				map.RegisteredWrapperTypes.Add (typeHandle);
			}

			// Return whatever's in the dictionary now.
			if (wrapper_types.TryGetValue (typeHandle, out wrapperType))
				return Type.GetTypeFromHandle (wrapperType);

			return null;
		}

		public static void Register<T> ()
			where T : IManagedRegistrarType
		{
			if (!Runtime.IsManagedStaticRegistrar) {
				throw new InvalidOperationException ($"Cannot register type '{typeof (T)}' when the managed static registrar is not used.");
			}
#if TRACE
			Console.WriteLine($"RegistrarHelper: Register {typeof(T)}, {typeof (T).Assembly}");
#endif
			var map = GetOrInitMapInfo (typeof (T).Assembly);
			map.Registrar.Register<T> ();
		}

#if TRACE
		[ThreadStatic]
		static Stopwatch? lookupWatch;
#endif

		internal static IntPtr LookupUnmanagedFunction (IntPtr assembly, string? symbol, int id)
		{
			IntPtr rv;

#if TRACE
			if (lookupWatch is null)
				lookupWatch = new Stopwatch ();

			lookupWatch.Start ();
			Console.WriteLine ("LookupUnmanagedFunction (0x{0} = {1}, {2}, {3})", assembly.ToString ("x"), Marshal.PtrToStringAuto (assembly), symbol, id);
#endif

			if (id == -1) {
				rv = IntPtr.Zero;
			} else {
				rv = LookupUnmanagedFunctionInAssembly (assembly, symbol, id);
			}

#if TRACE
			lookupWatch.Stop ();

			Console.WriteLine ("LookupUnmanagedFunction (0x{0} = {1}, {2}, {3}) => 0x{4} ElapsedMilliseconds: {5}", assembly.ToString ("x"), Marshal.PtrToStringAuto (assembly), symbol, id, rv.ToString ("x"), lookupWatch.ElapsedMilliseconds);
#endif

			if (rv != IntPtr.Zero)
				return rv;

			throw ErrorHelper.CreateError (8001, "Unable to find the managed function with id {0} ({1})", id, symbol);
		}

		static IntPtr LookupUnmanagedFunctionInAssembly (IntPtr assembly_name, string? symbol, int id)
		{
			// return registrar.LookupUnmanagedFunction (symbol, id);
			return IntPtr.Zero;
		}

		internal static bool IsCustomType (Type type)
		{
			var map = GetOrInitMapInfo (type.Assembly);
			return map.Registrar.IsCustomType (type);
		}

		internal static IntPtr FindClass (Type type, out bool isCustomType)
		{
			var map = GetOrInitMapInfo (type.Assembly);
			return map.Registrar.FindClass (type, out isCustomType);
		}

		internal static Type? FindType (NativeHandle @class, out bool isCustomType)
		{
			return ManagedRegistrar.FindType (@class, out isCustomType);
		}

		internal static T? ConstructNSObject<T> (IntPtr ptr, Type type)
			where T : class, INativeObject
		{
			var map = GetOrInitMapInfo (type.Assembly);
			return (T?)(INativeObject?)map.Registrar.CreateNSObject (type.TypeHandle, ptr);
		}

		internal static T? ConstructINativeObject<T> (IntPtr ptr, bool owns, Type type)
			where T : class, INativeObject
		{
			var map = GetOrInitMapInfo (type.Assembly);
			return (T?)map.Registrar.CreateINativeObject (type.TypeHandle, ptr, owns);
		}

		static MapInfo GetOrInitMapInfo (Assembly assembly)
		{
			var assemblyName = assembly.FullName!;
			if (!assembly_map.TryGetValue (assemblyName, out var map))
			{
				map = assembly_map [assemblyName] = new MapInfo (RuntimeTypeHandleEqualityComparer);
			}

			return map;
		}

		// helper functions for converting between native and managed objects
		static NativeHandle ManagedArrayToNSArray (object array, bool retain)
		{
			if (array is null)
				return NativeHandle.Zero;

			NSObject rv;
			if (array is NSObject[] nsobjs) {
				rv = NSArray.FromNSObjects (nsobjs);
			} else if (array is INativeObject[] inativeobjs) {
				rv = NSArray.FromNSObjects (inativeobjs);
			} else {
				throw new InvalidOperationException ($"Can't convert {array.GetType ()} to an NSArray."); // FIXME: better error
			}

			if (retain)
				return Runtime.RetainNSObject (rv);
			return Runtime.RetainAndAutoreleaseNSObject (rv);
		}

		unsafe static void NSArray_string_native_to_managed (IntPtr* ptr, ref string[]? value, ref string[]? copy)
		{
			if (ptr is not null) {
				value = NSArray.StringArrayFromHandle (*ptr);
			} else {
				value = null;
			}
			copy = value;
		}

		unsafe static void NSArray_string_managed_to_native (IntPtr* ptr, string[] value, string[] copy, bool isOut)
		{
			if (ptr is null)
				return;

			// Note that we won't notice if individual array elements change, only if the array itself changes
			if (!isOut && (object) value == (object) copy) {
#if TRACE
				Runtime.NSLog ($"NSArray_string_managed_to_native (0x{(*ptr).ToString ("x")}, equal)");
#endif
				return;
			}
			if (value is null) {
#if TRACE
				Runtime.NSLog ($"NSArray_string_managed_to_native (0x{(*ptr).ToString ("x")}, null, !null)");
#endif
				*ptr = IntPtr.Zero;
				return;
			}
			IntPtr rv = Runtime.RetainAndAutoreleaseNSObject (NSArray.FromStrings (value));
#if TRACE
			Runtime.NSLog ($"NSArray_string_managed_to_native (0x{(*ptr).ToString ("x")}, value != copy: {value?.Length} != {copy?.Length}): 0x{rv.ToString ("x")} => {value?.GetType ()}");
#endif
			*ptr = rv;
		}

		unsafe static void NSArray_native_to_managed<T> (IntPtr* ptr, ref T[]? value, ref T[]? copy) where T: class, INativeObject
		{
			if (ptr is not null) {
				value = NSArray.ArrayFromHandle<T> (*ptr);
			} else {
				value = null;
			}
			copy = value;
		}

		unsafe static void NSArray_managed_to_native<T> (IntPtr* ptr, T[] value, T[] copy, bool isOut) where T: class, INativeObject
		{
			if (ptr is null) {
#if TRACE
				Runtime.NSLog ($"NSArray_managed_to_native (NULL, ?, ?)");
#endif
				return;
			}
			// Note that we won't notice if individual array elements change, only if the array itself changes
			if (!isOut && (object) value == (object) copy) {
#if TRACE
				Runtime.NSLog ($"NSArray_managed_to_native (0x{(*ptr).ToString ("x")}, equal)");
#endif
				return;
			}
			if (value is null) {
#if TRACE
				Runtime.NSLog ($"NSArray_managed_to_native (0x{(*ptr).ToString ("x")}, null, !null)");
#endif
				*ptr = IntPtr.Zero;
				return;
			}
			IntPtr rv = Runtime.RetainAndAutoreleaseNSObject (NSArray.FromNSObjects<T> (value));
#if TRACE
			Runtime.NSLog ($"NSArray_managed_to_native (0x{(*ptr).ToString ("x")}, value != copy: {value?.Length} != {copy?.Length}): 0x{rv.ToString ("x")} => {value?.GetType ()}");
#endif
			*ptr = rv;
		}

		unsafe static void NSObject_native_to_managed<T> (IntPtr* ptr, ref T? value, ref T? copy) where T: NSObject
		{
			if (ptr is not null) {
				value = Runtime.GetNSObject<T> (*ptr, owns: false);
			} else {
				value = null;
			}
			copy = value;
		}

		unsafe static void NSObject_managed_to_native (IntPtr* ptr, NSObject value, NSObject copy, bool isOut)
		{
			if (ptr is null) {
#if TRACE
				Runtime.NSLog ($"NSObject_managed_to_native (NULL, ?, ?)");
#endif
				return;
			}
			if (!isOut && (object) value == (object) copy) {
#if TRACE
				Runtime.NSLog ($"NSObject_managed_to_native (0x{(*ptr).ToString ("x")}, equal)");
#endif
				return;
			}
			IntPtr rv = Runtime.RetainAndAutoreleaseNSObject (value);
#if TRACE
			Runtime.NSLog ($"NSObject_managed_to_native (0x{(*ptr).ToString ("x")}, ? != ?): 0x{rv.ToString ("x")} => {value?.GetType ()}");
#endif
			*ptr = rv;
		}

		unsafe static void string_native_to_managed (NativeHandle *ptr, ref string? value, ref string? copy)
		{
			if (ptr is not null) {
				value = CFString.FromHandle (*ptr);
			} else {
				value = null;
			}
			copy = value;
		}

		unsafe static void string_managed_to_native (NativeHandle *ptr, string value, string copy, bool isOut)
		{
			if (ptr is null) {
#if TRACE
				Runtime.NSLog ($"string_managed_to_native (NULL, ?, ?)");
#endif
				return;
			}
			if (!isOut && (object) value == (object) copy) {
#if TRACE
				Runtime.NSLog ($"string_managed_to_native (0x{(*ptr).ToString ()}, equal)");
#endif
				return;
			}
			var rv = CFString.CreateNative (value);
#if TRACE
			Runtime.NSLog ($"string_managed_to_native (0x{(*ptr).ToString ()}, ? != ?): 0x{rv.ToString ()} => {value}");
#endif
			*ptr = rv;
		}

		unsafe static void INativeObject_native_to_managed<T> (IntPtr* ptr, ref T? value, ref T? copy, RuntimeTypeHandle implementationType) where T: class, INativeObject
		{
			if (ptr is not null) {
				value = Runtime.GetINativeObject<T> (*ptr, implementation: Type.GetTypeFromHandle (implementationType), forced_type: false, owns: false);
			} else {
				value = null;
			}
			copy = value;
		}

		unsafe static void INativeObject_managed_to_native (IntPtr *ptr, INativeObject value, INativeObject copy, bool isOut)
		{
			if (ptr is null) {
#if TRACE
				Runtime.NSLog ($"INativeObject_managed_to_native (NULL, ?, ?)");
#endif
				return;
			}
			if (!isOut && (object) value == (object) copy) {
#if TRACE
				Runtime.NSLog ($"INativeObject_managed_to_native (0x{(*ptr).ToString ("x")}, equal)");
#endif
				return;
			}
			IntPtr rv = value.GetHandle ();
#if TRACE
			Runtime.NSLog ($"INativeObject_managed_to_native (0x{(*ptr).ToString ("x")}, ? != ?): 0x{rv.ToString ("x")} => {value?.GetType ()}");
#endif
			*ptr = rv;
		}

		public unsafe static IntPtr GetDotnetType<T> (IntPtr* exception_gchandle) {
			try {
				var type = typeof (T);
				if (type.IsGenericType)
					type = type.GetGenericTypeDefinition ();

				var rv = Runtime.AllocGCHandle (type);
#if TRACE
				Runtime.NSLog ($"GetDotnetType: {type} (GC handle: 0x{rv:x})");
#endif
				return rv;
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
				return IntPtr.Zero;
			}
		}
	}
}

#endif // NET
