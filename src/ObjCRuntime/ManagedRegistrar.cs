//
// ManagedRegistrar.cs
//
// Authors:
//   Rolf Bjarne Kvinge
//
// Copyright 2023 Microsoft Corp

#if NET

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;

using Foundation;
using ObjCRuntime;

namespace ObjCRuntime {
	// The ManagedRegistrarHandleWrapper needs to be unique in the codebase. If the developer uses it in their codebase
	// they could break things. Maybe there could be an analzyer that'll check if it is being used somewhere 
	public struct ManagedRegistrarHandleWrapper
	{
		private readonly NativeHandle _handle;
		public ManagedRegistrarHandleWrapper (NativeHandle handle) => _handle = handle;
		public static explicit operator NativeHandle (ManagedRegistrarHandleWrapper wrapper) => wrapper._handle;
	}

	public interface IManagedRegistrarType
	{
		static virtual NSObject CreateNSObject(NativeHandle handle) => throw new InvalidOperationException ();
		static virtual INativeObject CreateINativeObject(NativeHandle handle, bool owns) => throw new InvalidOperationException ();
		static virtual IntPtr GetObjCClass (out bool is_custom_type) => throw new InvalidOperationException ();
	}

	public sealed class ManagedRegistrar
	{
		delegate IntPtr GetObjCClassFunc (out bool is_custom_type);

		private Dictionary<RuntimeTypeHandle, GetObjCClassFunc> _getObjCClasses;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _nsObjectFactories;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _nativeObjectFactories;
		private Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> _wrapperTypes;

		internal ManagedRegistrar (RuntimeTypeHandleEqualityComparer runtimeTypeHandleEqualityComparer)
		{
			_getObjCClasses = new (runtimeTypeHandleEqualityComparer);
			_nsObjectFactories = new (runtimeTypeHandleEqualityComparer);
			_nativeObjectFactories = new (runtimeTypeHandleEqualityComparer);
			_wrapperTypes = new (runtimeTypeHandleEqualityComparer);
		}

		public static void Register<T> ()
			where T : IManagedRegistrarType
		{
			if (!Runtime.IsManagedStaticRegistrar) {
				throw new InvalidOperationException ($"Cannot register type '{typeof (T)}' when the managed static registrar is not used.");
			}

			RegistrarHelper.RegisterManagedRegistrarType<T> ();
		}

		internal static void ThrowWhenUsingManagedStaticRegistrar ([CallerMemberName] string? caller = null)
		{
			if (Runtime.IsManagedStaticRegistrar)
				throw new UnreachableException (
					$"The method '{caller ?? "<unknown>"}' cannot be called when using the managed static registrar.");
		}

		internal void AddType<T> ()
			where T : IManagedRegistrarType
		{
			// TODO lock-free synchronization using SemaphoreSlim?
			var type = typeof (T);

			// types that are generic but don't have any generic type arguments cannot be instantiated
			if (!type.IsGenericType || type != type.GetGenericTypeDefinition ()) {
				_nsObjectFactories.Add (type.TypeHandle, T.CreateNSObject);
				_nativeObjectFactories.Add (type.TypeHandle, T.CreateINativeObject);
				_getObjCClasses.Add (type.TypeHandle, T.GetObjCClass);
			} else {
				Console.WriteLine ($"Registered a generic type {type} without any generic type arguments.");
			}
		}

		internal IntPtr FindClass (Type type, out bool is_custom_type)
		{
			is_custom_type = false;
			EnsureClassConstructorHasRun (type.TypeHandle);

			if (_getObjCClasses.TryGetValue (type.TypeHandle, out var getObjCClass))
				return getObjCClass (out is_custom_type);

			ThrowIfGenericType (type);
			return IntPtr.Zero;
		}

		internal static Type? FindType (NativeHandle @class, out bool is_custom_type)
		{
			is_custom_type = false;
			var ptr = IntPtr_objc_msgSend_ref_bool (@class, Selector.GetHandle ("getDotnetType:"), ref is_custom_type);
			return GCHandle.FromIntPtr (ptr).Target as Type;
		}

		internal NSObject? CreateNSObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle)
		{
			EnsureClassConstructorHasRun (runtimeTypeHandle);

			if (_nsObjectFactories.TryGetValue (runtimeTypeHandle, out var factory))
				return factory (handle);

			ThrowIfGenericType (runtimeTypeHandle);
			return null;
		}

		internal INativeObject? CreateINativeObject (RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle, bool owns)
		{
			EnsureClassConstructorHasRun (runtimeTypeHandle);

			if (_nativeObjectFactories.TryGetValue (runtimeTypeHandle, out var factory))
				return factory (handle, owns);

			ThrowIfGenericType (runtimeTypeHandle);
			return null;
		}

		internal bool RegisterWrapperType(RuntimeTypeHandle runtimeTypeHandle, Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapperTypes)
		{
			EnsureClassConstructorHasRun(runtimeTypeHandle);

			if (_wrapperTypes.TryGetValue(runtimeTypeHandle, out var wrapperType))
			{
				// it shouldn't be there but there's no reason to throw if it's already there (race condition?)
				_ = wrapperTypes.TryAdd(runtimeTypeHandle, wrapperType);
				return true;
			}

			return false;
		}

		[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2059:UnrecognizedReflectionPattern", Justification =
			"The static constructor of the type is preserved if and only if the whole type is preserved. " +
			"If the type was trimmed, we won't be able to create an instance of the type anyway.")]
		private void EnsureClassConstructorHasRun(RuntimeTypeHandle typeHandle)
		{
			RuntimeHelpers.RunClassConstructor(typeHandle);
		}

		private void ThrowIfGenericType (RuntimeTypeHandle runtimeTypeHandle)
		{
			var type = Type.GetTypeFromHandle (runtimeTypeHandle);
			if (type is not null) {
				ThrowIfGenericType (type);
			}
		}

		private void ThrowIfGenericType (Type type)
		{
			if (type.IsGenericType && type == type.GetGenericTypeDefinition ()) {
				// the type is a generic type without any type arguments
				// there's no way we can create an instance of such type
				// so let's just throw...
				throw new InvalidOperationException($"Cannot create an instance of generic class '{type}'.");
			}
		}

		// TODO: is this necessary in our use case? it seems it's only used in debug mode with dynamic linking?
		// Should we register all the entrypoints with the type in the static constructor?
		internal IntPtr LookupUnmanagedFunction(string? entryPoint, int id) => (IntPtr)(-1);

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public extern static IntPtr IntPtr_objc_msgSend_ref_bool (IntPtr receiver, IntPtr selector, ref bool p1);
	}
}

#endif // NET
