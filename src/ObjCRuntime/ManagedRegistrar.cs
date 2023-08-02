//
// ManagedRegistrar.cs
//
// Authors:
//   Rolf Bjarne Kvinge
//
// Copyright 2023 Microsoft Corp

#if NET

#define TRACE

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
	public interface IManagedRegistrarType
	{
		static virtual NSObject CreateNSObject(NativeHandle handle) => throw new InvalidOperationException ();
		static virtual INativeObject CreateINativeObject(NativeHandle handle, bool owns) => throw new InvalidOperationException ();
		static virtual IntPtr GetNativeClass (out bool is_custom_type) => throw new InvalidOperationException ();
	}

	internal sealed class ManagedRegistrar
	{
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim (1, 1);

		delegate IntPtr GetNativeClassFunc (out bool is_custom_type);

		private Dictionary<RuntimeTypeHandle, GetNativeClassFunc> _getNativeClasses;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _createNSObjects;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _createINativeObjects;
		private Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> _protocolWrapperTypes;

		internal ManagedRegistrar (RuntimeTypeHandleEqualityComparer runtimeTypeHandleEqualityComparer)
		{
			_getNativeClasses = new (runtimeTypeHandleEqualityComparer);
			_createNSObjects = new (runtimeTypeHandleEqualityComparer);
			_createINativeObjects = new (runtimeTypeHandleEqualityComparer);
			_protocolWrapperTypes = new (runtimeTypeHandleEqualityComparer);
		}

		internal void Register<T> ()
			where T : IManagedRegistrarType
		{
			var type = typeof (T);
			var handle = type.TypeHandle;

			// types that are generic but don't have any generic type arguments cannot be registered
			// this is a bug that the developer must fix
			if (type.IsGenericType && type == type.GetGenericTypeDefinition ()) {
				throw new InvalidOperationException (
					$"Cannot register a generic type {type} without generic type arguments.");
			}

			_semaphore.Wait ();
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: Registering {type}");
#endif
			try {
				_createNSObjects.Add (handle, T.CreateNSObject);
				_createINativeObjects.Add (handle, T.CreateINativeObject);
				_getNativeClasses.Add (handle, T.GetNativeClass);
				// TODO protocol wrapper types
			} finally {
				_semaphore.Release ();
			}
		}

		internal IntPtr FindClass (Type type, out bool is_custom_type)
		{
			is_custom_type = false;
			EnsureClassConstructorHasRun (type.TypeHandle);

			if (_getNativeClasses.TryGetValue (type.TypeHandle, out var getObjCClass))
				return getObjCClass (out is_custom_type);

			ThrowIfGenericType (type);
			return IntPtr.Zero;
		}

		internal static Type? FindType (NativeHandle @class, out bool is_custom_type)
		{
			is_custom_type = false;

			var ptr = IntPtr_objc_msgSend_ref_bool (@class, Selector.GetHandle ("getDotnetType:"), ref is_custom_type);
			if (ptr == IntPtr.Zero)
				return null;

			return GCHandle.FromIntPtr (ptr).Target as Type;
		}

		internal NSObject? CreateNSObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle)
		{
			EnsureClassConstructorHasRun (runtimeTypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: CreateNSObject(type: {Type.GetTypeFromHandle (runtimeTypeHandle)}, handle: {handle})");
#endif
			if (_createNSObjects.TryGetValue (runtimeTypeHandle, out var factory))
				return factory (handle);

			ThrowIfGenericType (runtimeTypeHandle);
			return null;
		}

		internal INativeObject? CreateINativeObject (RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle, bool owns)
		{
			EnsureClassConstructorHasRun (runtimeTypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: CreateINativeObject(type: {Type.GetTypeFromHandle (runtimeTypeHandle)}, handle: {handle}, owns: {owns})");
#endif
			if (_createINativeObjects.TryGetValue (runtimeTypeHandle, out var factory))
				return factory (handle, owns);

			ThrowIfGenericType (runtimeTypeHandle);
			return null;
		}

		internal bool RegisterWrapperType(RuntimeTypeHandle runtimeTypeHandle, Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapperTypes)
		{
			EnsureClassConstructorHasRun(runtimeTypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: RegisterWrapperType(type: {Type.GetTypeFromHandle (runtimeTypeHandle)})");
#endif
			if (_protocolWrapperTypes.TryGetValue(runtimeTypeHandle, out var wrapperType))
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

		// TODO: this is necessary whenever we don't statically link the final app (e.g., in debug mode)
		internal IntPtr LookupUnmanagedFunction(string? entryPoint, int id) => (IntPtr)(-1);

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public extern static IntPtr IntPtr_objc_msgSend_ref_bool (IntPtr receiver, IntPtr selector, ref bool p1);
	}
}

#endif // NET
