//
// ManagedRegistrar.cs
//
// Authors:
//   Rolf Bjarne Kvinge
//
// Copyright 2023 Microsoft Corp

#if NET

// #define TRACE

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
		static virtual bool IsCustomType => false;
		static virtual NSObject CreateNSObject(NativeHandle handle) => throw new InvalidOperationException ();
		static virtual INativeObject CreateINativeObject(NativeHandle handle, bool owns) => throw new InvalidOperationException ();
		static virtual NativeHandle GetNativeClass () => throw new InvalidOperationException ();
	}

	internal sealed class ManagedRegistrar
	{
		private readonly static IntPtr s_getDotnetTypeSelector = Selector.GetHandle ("getDotnetType");

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim (1, 1);

		private Dictionary<RuntimeTypeHandle, Func<NativeHandle>> _getNativeClasses;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _createNSObjects;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _createINativeObjects;
		private Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> _protocolWrapperTypes;
		private HashSet<RuntimeTypeHandle> _customTypes;

		internal ManagedRegistrar (RuntimeTypeHandleEqualityComparer runtimeTypeHandleEqualityComparer)
		{
			_getNativeClasses = new (runtimeTypeHandleEqualityComparer);
			_createNSObjects = new (runtimeTypeHandleEqualityComparer);
			_createINativeObjects = new (runtimeTypeHandleEqualityComparer);
			_protocolWrapperTypes = new (runtimeTypeHandleEqualityComparer);
			_customTypes = new (runtimeTypeHandleEqualityComparer);
		}

		internal void Register<T> ()
			where T : IManagedRegistrarType
		{
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: Registering {typeof (T)}");
#endif
			var type = typeof (T);
			var handle = type.TypeHandle;

			// types that are generic but don't have any generic type arguments cannot be registered
			// this is a bug that the developer must fix
			if (type.IsGenericType && type == type.GetGenericTypeDefinition ()) {
#if TRACE
				Runtime.NSLog ($"ManagedRegistrar: {type} is generic but doesn't have any generic type arguments.");
#endif
				return;
			}

			_semaphore.Wait ();
			try {
				if (_createNSObjects.ContainsKey (handle)) {
#if TRACE
					Runtime.NSLog ($"ManagedRegistrar: {type} has already been registered");
#endif
					return;
				}

				_createNSObjects.Add (handle, T.CreateNSObject);
				_createINativeObjects.Add (handle, T.CreateINativeObject);
				_getNativeClasses.Add (handle, T.GetNativeClass);
				if (T.IsCustomType)
					_customTypes.Add (handle);
				// TODO protocol wrapper types
			} finally {
				_semaphore.Release ();
			}
		}

		internal bool IsCustomType (Type type)
		{
			EnsureRegistered (type.TypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: IsCustomType(type: {type})");
#endif
			return _customTypes.Contains (type.TypeHandle);
		}

		internal static Type? FindType (NativeHandle @class)
		{
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: FindType(@class: {@class})");
#endif
			var ptr = Messaging.IntPtr_objc_msgSend (@class, s_getDotnetTypeSelector);
			return Type.GetTypeFromHandle (RuntimeTypeHandle.FromIntPtr (ptr));
		}

		internal IntPtr FindClass (Type type)
		{
			EnsureRegistered (type.TypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: FindClass(type: {type})");
#endif
			if (_getNativeClasses.TryGetValue (type.TypeHandle, out var getNativeClass)) {
				return getNativeClass ();
			}

			ThrowNotRegisteredType (type.TypeHandle);
			return default;
		}

		internal NSObject? CreateNSObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle)
		{
			EnsureRegistered (runtimeTypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: CreateNSObject(type: {Type.GetTypeFromHandle (runtimeTypeHandle)}, handle: {handle})");
#endif
			if (_createNSObjects.TryGetValue (runtimeTypeHandle, out var factory)) {
				return factory (handle);
			}

			ThrowNotRegisteredType (runtimeTypeHandle);
			return default;
		}

		internal INativeObject? CreateINativeObject (RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle, bool owns)
		{
			EnsureRegistered (runtimeTypeHandle);
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: CreateINativeObject(type: {Type.GetTypeFromHandle (runtimeTypeHandle)}, handle: {handle}, owns: {owns})");
#endif
			if (_createINativeObjects.TryGetValue (runtimeTypeHandle, out var factory)) {
				return factory (handle, owns);
			}

			ThrowNotRegisteredType (runtimeTypeHandle);
			return default;
		}

		internal bool RegisterWrapperType(RuntimeTypeHandle runtimeTypeHandle, Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapperTypes)
		{
			EnsureRegistered(runtimeTypeHandle);
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
			"We do not need the RuntimeHelpers.RunClassConstructor method to contribute to dependency analysis. " +
			"We only need to run the class constructors for types that are used elsewhere in code. " +
			"The static constructor of the type is preserved when its parent type is preserved.")]
		internal void EnsureRegistered (RuntimeTypeHandle runtimeTypeHandle)
		{
#if TRACE
			Runtime.NSLog ($"ManagedRegistrar: EnsureRegistered(type: {Type.GetTypeFromHandle (runtimeTypeHandle)})");
#endif
			RuntimeHelpers.RunClassConstructor(runtimeTypeHandle);
		}

		[DoesNotReturn] 
		private void ThrowNotRegisteredType (RuntimeTypeHandle runtimeTypeHandle)
		{
			var type = Type.GetTypeFromHandle (runtimeTypeHandle);
			if (type is not null) {
				ThrowNotRegisteredType (type);
			} else {
				throw new InvalidOperationException ($"Type with handle 0x{runtimeTypeHandle.Value:x} has not been registered.");
			}
		}

		[DoesNotReturn]
		private void ThrowNotRegisteredType (Type type)
		{
			throw new InvalidOperationException ($"Type '{type}' has not been registered.");
		}
	}
}

#endif // NET
