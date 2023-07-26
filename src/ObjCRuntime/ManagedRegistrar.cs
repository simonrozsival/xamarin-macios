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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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
	}

	public sealed class ManagedRegistrar
	{
		private Dictionary<RuntimeTypeHandle, uint> _typeIds;
		private Dictionary<uint, RuntimeTypeHandle> _typeHandles;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _nsObjectFactories;
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _nativeObjectFactories;
		private Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> _wrapperTypes;

		internal ManagedRegistrar (RuntimeTypeHandleEqualityComparer runtimeTypeHandleEqualityComparer)
		{
			_typeIds = new (runtimeTypeHandleEqualityComparer);
			_typeHandles = new ();
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

		internal void AddType<T> ()
			where T : IManagedRegistrarType
		{
			// TODO lock-free synchronization using SemaphoreSlim?
			var type = typeof (T);

			// types that are generic but don't have any generic type arguments cannot be instantiated
			if (!type.IsGenericType || type != type.GetGenericTypeDefinition ()) {
				_nsObjectFactories.Add (type.TypeHandle, T.CreateNSObject);
				_nativeObjectFactories.Add (type.TypeHandle, T.CreateINativeObject);
			} else {
				Console.WriteLine ($"Registered a generic type {type} without any generic type arguments.");
			}

			// we need to erase the generic type arguments so that all of these generic types
			// map to the same Objective-C class ...
			if (type.IsGenericType) {
				var genericType = type.GetGenericTypeDefinition ();
				var typeId = ManagedRegistrarHelpers.CalculateTypeId (genericType);
				_typeHandles.Add (typeId, genericType.TypeHandle);
				Console.WriteLine($"[{type}] Registering mapping typeId={typeId} -> runtimeTypeHandle={genericType.TypeHandle.Value}");
				_typeIds.Add (type.TypeHandle, typeId); // we want to map all the generic types to the same type ID
				Console.WriteLine($"[{type}] Registering mapping runtimeTypeHandle={type.TypeHandle.Value} -> typeId={typeId}");
				_ = _typeIds.TryAdd (genericType.TypeHandle, typeId); // it's possible that this generic type mapping was added already
				Console.WriteLine($"[{type}] Registering mapping runtimeTypeHandle={genericType.TypeHandle.Value} -> typeId={typeId}");
				
			} else {
				var typeId = ManagedRegistrarHelpers.CalculateTypeId (type);
				_typeHandles.Add (typeId, type.TypeHandle);
				Console.WriteLine($"[{type}] Registering mapping typeId={typeId} -> runtimeTypeHandle={type.TypeHandle.Value}");
				_typeIds.Add (type.TypeHandle, typeId);
				Console.WriteLine($"[{type}] Registering mapping runtimeTypeHandle={type.TypeHandle.Value} -> typeId={typeId}");
			}
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

		internal RuntimeTypeHandle LookupType (uint id)
		{	
			// TODO how can I force the static constructor to run?
			// do we assume that this is not called before the static constructor has run?
			if (_typeHandles.TryGetValue (id, out var handle))
				return handle;

			// TODO what exception should we throw??
			throw new InvalidOperationException ($"No type with ID {id} has been registered.");
		}

		internal uint LookupTypeId(RuntimeTypeHandle runtimeTypeHandle)
		{
			EnsureClassConstructorHasRun(runtimeTypeHandle);
			return _typeIds[runtimeTypeHandle];
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
			if (type is not null && type.IsGenericType && type == type.GetGenericTypeDefinition ()) {
				// the type is a generic type without any type arguments
				// there's no way we can create an instance of such type
				// so let's just throw...
				throw new InvalidOperationException($"Cannot create an instance of generic class '{type}'.");
			}
		}

		// TODO: is this necessary in our use case? it seems it's only used in debug mode with dynamic linking?
		// Should we register all the entrypoints with the type in the static constructor?
		internal IntPtr LookupUnmanagedFunction(string? entryPoint, int id) => (IntPtr)(-1);
	}
}

#endif // NET
