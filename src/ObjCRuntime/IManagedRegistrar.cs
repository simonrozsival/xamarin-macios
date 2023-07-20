//
// IManagedRegistrar.cs
//
// Authors:
//   Rolf Bjarne Kvinge
//
// Copyright 2023 Microsoft Corp

// #define NET
#if NET

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Foundation;
using ObjCRuntime;
using UIKit;

namespace ObjCRuntime {
	// The ManagedRegistrarHandleWrapper needs to be unique in the codebase. If the developer uses it in their codebase
	// they could break things. It could be somehow randomized to make the name unpredictable (like ManagedRegistrarHandleWrapper1alskdhashaj)
	public struct ManagedRegistrarHandleWrapper
	{
		private readonly NativeHandle _handle;
		private ManagedRegistrarHandleWrapper(NativeHandle handle) => _handle = handle;
		public static implicit operator NativeHandle(ManagedRegistrarHandleWrapper wrapper) => wrapper._handle;
		public static implicit operator ManagedRegistrarHandleWrapper(NativeHandle handle) => new ManagedRegistrarHandleWrapper(handle);
		public static implicit operator ManagedRegistrarHandleWrapper(IntPtr handle) => new ManagedRegistrarHandleWrapper(new NativeHandle(handle));
	}

	public interface IManagedRegistrarType
	{
		static abstract uint TypeId { get; }
		static virtual bool HasNSObjectFactory => false;
		static virtual bool HasINativeObjectFactory => false;
		static virtual NSObject CreateNSObject(NativeHandle handle) => throw new InvalidOperationException();
		static virtual INativeObject CreateINativeObject(NativeHandle handle, bool owns) => throw new InvalidOperationException();
	}

	public sealed class ManagedRegistrar
	{
		private Dictionary<RuntimeTypeHandle, uint> _typeIds = new();
		private Dictionary<uint, RuntimeTypeHandle> _typeHandles = new();

		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _nsObjectFactories = new();
		private Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _nativeObjectFactories = new();

		private Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> _wrapperTypes = new();

		public void RegisterType<T>()
			where T : IManagedRegistrarType
		{
			Console.WriteLine($"Registering type {typeof(T).FullName} with id {T.TypeId} (has NSObject factory: {T.HasNSObjectFactory}, has INativeObject factory: {T.HasINativeObjectFactory})");

			_typeIds.Add(typeof(T).TypeHandle, T.TypeId);
			_typeHandles.Add(T.TypeId, typeof(T).TypeHandle);

			if (T.HasNSObjectFactory)
				_nsObjectFactories.Add(typeof(T).TypeHandle, T.CreateNSObject);

			if (T.HasINativeObjectFactory)
				_nativeObjectFactories.Add(typeof(T).TypeHandle, T.CreateINativeObject);
		}

		public NSObject? CreateNSObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle)
		{
			EnsureClassConstructorHasRun(runtimeTypeHandle);

			if (_nsObjectFactories.TryGetValue(runtimeTypeHandle, out var factory))
				return factory(handle);

			return null;
		}

		public INativeObject? CreateINativeObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle, bool owns)
		{
			EnsureClassConstructorHasRun(runtimeTypeHandle);

			if (_nativeObjectFactories.TryGetValue(runtimeTypeHandle, out var factory))
				return factory(handle, owns);

			return null;
		}

		public RuntimeTypeHandle LookupType(uint id) => _typeHandles[id];
		public uint LookupTypeId(RuntimeTypeHandle handle) => _typeIds[handle];

		public bool RegisterWrapperType(RuntimeTypeHandle runtimeTypeHandle, Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapperTypes)
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

		// TODO: is this necessary in our use case? it seems it's only used in debug mode with dynamic linking?
		// Should we register all the entrypoints with the type in the static constructor?
		public IntPtr LookupUnmanagedFunction(string? entryPoint, int id) => (IntPtr)(-1);
	}
}

internal static class ManagedRegistrarSingleton
{
	private static ManagedRegistrar s_instance = new();

	[ModuleInitializer]
	public static void Init()
	{
		RegistrarHelper.Register(s_instance);
	}

	public static void RegisterType<T>() where T : IManagedRegistrarType
		=> s_instance.RegisterType<T>();

	public static NSObject? CreateNSObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle)
		=> s_instance.CreateNSObject(runtimeTypeHandle, handle);

	public static INativeObject? CreateINativeObject(RuntimeTypeHandle runtimeTypeHandle, NativeHandle handle, bool owns)
		=> s_instance.CreateINativeObject(runtimeTypeHandle, handle, owns);

	public static RuntimeTypeHandle LookupType(uint id) => s_instance.LookupType(id);
	public static uint LookupTypeId(RuntimeTypeHandle handle) => s_instance.LookupTypeId(handle);

	public static bool RegisterWrapperType(RuntimeTypeHandle runtimeTypeHandle, Dictionary<RuntimeTypeHandle, RuntimeTypeHandle> wrapperTypes)
		=> s_instance.RegisterWrapperType(runtimeTypeHandle, wrapperTypes);
}

// public static class MySingleView_AppDelegate__Registrar_Callback__
// {
// 	[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_FinishedLaunching_2")]
// 	public unsafe static byte callback_MySingleView_AppDelegate_FinishedLaunching(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
// 	{
// 		var methodHandle = typeof(MySingleView.AppDelegate).GetMethod(nameof(MySingleView.AppDelegate.FinishedLaunching))!.MethodHandle; // ?? how to do thsi without reflection?
// 		try
// 		{
// 			MySingleView.AppDelegate nSObject = Runtime.GetNSObject<MySingleView.AppDelegate>(pobj, sel, methodHandle, true)!;
// 			UIApplication nSObject2 = Runtime.GetNSObject<UIApplication>(p0, sel, methodHandle, false)!;
// 			NSDictionary nSObject3 = Runtime.GetNSObject<NSDictionary>(p1, sel, methodHandle, false)!;
// 			return nSObject.FinishedLaunching(nSObject2, nSObject3) ? ((byte)1) : ((byte)0);
// 		}
// 		catch (Exception ex)
// 		{
// 			*exception_gchandle = Runtime.AllocGCHandle(ex);
// 		}
// 		return default(byte);
// 	}

// 	[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate__ctor_2")]
// 	public unsafe static NativeHandle callback_MySingleView_AppDelegate__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
// 	{
// 		try
// 		{
// 			if (Runtime.HasNSObject(pobj) != 0)
// 			{
// 				*call_super = 1;
// 				return pobj;
// 			}
// 			// AppDelegate appDelegate = NSObject.AllocateNSObject<AppDelegate>(pobj);
// 			// appDelegate..ctor();
// 			MySingleView.AppDelegate appDelegate = new MySingleView.AppDelegate((ManagedRegistrarHandleWrapper)pobj);
// 			return NativeObjectExtensions.GetHandle(appDelegate);
// 		}
// 		catch (Exception ex)
// 		{
// 			*exception_gchandle = Runtime.AllocGCHandle(ex);
// 		}
// 		return default(NativeHandle);
// 	}
// }

// 1u => typeof(UIApplicationDelegate).TypeHandle, 
// 2u => typeof(UIResponder).TypeHandle, 
// 3u => typeof(UIViewController).TypeHandle, 
// 4u => typeof(NSDispatcher).TypeHandle, 
// 5u => typeof(NSSynchronizationContextDispatcher).TypeHandle, 
// 6u => typeof(NSRunLoop).TypeHandle, 
// 7u => typeof(NSAutoreleasePool).TypeHandle, 
// 8u => typeof(NSException).TypeHandle, 
// 9u => typeof(ActionDispatcher).TypeHandle, 
// 10u => typeof(UIView).TypeHandle, 
// 11u => typeof(UIControl).TypeHandle, 
// 12u => typeof(UIButton).TypeHandle, 
// 13u => typeof(NSDictionary).TypeHandle, 
// 14u => typeof(NSObject.NSObject_Disposer).TypeHandle, 
// 15u => typeof(UIApplication).TypeHandle, 
// 16u => typeof(UIScreen).TypeHandle, 
// 17u => typeof(UIWindow).TypeHandle, 
// _ => typeof(NSObject).TypeHandle, 

#if MONOMAC || __MACCATALYST__
namespace AppKit {
	partial class ActionDispatcher : IManagedRegistrarType {
		static ActionDispatcher()
		{
			ManagedRegistrarSingleton.RegisterType<ActionDispatcher>();
		}
		public static uint TypeId => 9u;
	}
}
#endif

namespace UIKit {
	public partial class UIApplicationDelegate : NSObject, IManagedRegistrarType { // TODO why is the NSObject necessary? the base class is already NSObject..
		static UIApplicationDelegate()
		{
			ManagedRegistrarSingleton.RegisterType<UIApplicationDelegate>();
		}
		public static uint TypeId => 1u;
		public UIApplicationDelegate(ManagedRegistrarHandleWrapper handle) : base(handle, 0) { }
	}
	
	partial class UIResponder : IManagedRegistrarType {
		static UIResponder()
		{
			ManagedRegistrarSingleton.RegisterType<UIResponder>();
		}
		public static uint TypeId => 2u;
	}
	
	partial class UIViewController : IManagedRegistrarType {
		static UIViewController()
		{
			ManagedRegistrarSingleton.RegisterType<UIViewController>();
		}
		public static uint TypeId => 3u;
	}

	partial class UIView : IManagedRegistrarType {
		static UIView()
		{
			ManagedRegistrarSingleton.RegisterType<UIView>();
		}
		public static uint TypeId => 10u;
	}
	
	partial class UIControl : IManagedRegistrarType {
		static UIControl()
		{
			ManagedRegistrarSingleton.RegisterType<UIControl>();
		}
		public static uint TypeId => 11u;
	}
	
	partial class UIButton : IManagedRegistrarType {
		static UIButton()
		{
			ManagedRegistrarSingleton.RegisterType<UIButton>();
		}
		public static uint TypeId => 12u;
	}
	partial class UIApplication : IManagedRegistrarType {
		static UIApplication()
		{
			ManagedRegistrarSingleton.RegisterType<UIApplication>();
		}
		public static uint TypeId => 15u;
	}

	partial class UIScreen : IManagedRegistrarType {
		static UIScreen()
		{
			ManagedRegistrarSingleton.RegisterType<UIScreen>();
		}
		public static uint TypeId => 16u;
	}
	
	partial class UIWindow : IManagedRegistrarType {
		static UIWindow()
		{
			ManagedRegistrarSingleton.RegisterType<UIWindow>();
		}
		public static uint TypeId => 17u;
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
		static NSDispatcher()
		{
			ManagedRegistrarSingleton.RegisterType<NSDispatcher>();
		}
		public static uint TypeId => 4u;
	}
	
	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSSynchronizationContextDispatcher()
		{
			ManagedRegistrarSingleton.RegisterType<NSSynchronizationContextDispatcher>();
		}
		public static uint TypeId => 5u;
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
		static NSRunLoop()
		{
			ManagedRegistrarSingleton.RegisterType<NSRunLoop>();
		}
		public static uint TypeId => 6u;
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
		static NSAutoreleasePool()
		{
			ManagedRegistrarSingleton.RegisterType<NSAutoreleasePool>();
		}
		public static uint TypeId => 7u;
	}
	
	partial class NSException : IManagedRegistrarType {
		static NSException()
		{
			ManagedRegistrarSingleton.RegisterType<NSException>();
		}
		public static uint TypeId => 8u;
	}

	partial class NSDictionary : IManagedRegistrarType {
		static NSDictionary()
		{
			ManagedRegistrarSingleton.RegisterType<NSDictionary>();
		}
		public static uint TypeId => 13u;
	}
	
	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
			static NSObject_Disposer()
			{
				ManagedRegistrarSingleton.RegisterType<NSObject_Disposer>();
			}
			public static uint TypeId => 14u;
		}
	}
}

// TODO: THE REGISTRAR CALLBACKS COULDN'T BE GENERATED HERE BECAUSE OF LINKING???

#endif // NET
