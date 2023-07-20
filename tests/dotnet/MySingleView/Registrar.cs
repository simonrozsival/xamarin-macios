#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Foundation;
using ObjCRuntime;
using UIKit;

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

namespace MySingleView
{
	partial class AppDelegate : ObjCRuntime.IManagedRegistrarType
	{
		static AppDelegate()
		{
			// TOOD what's supposed to be the id?
			ManagedRegistrarSingleton.RegisterType<AppDelegate>();
		}

		// used instead of the [Export("init")] ctor from NSObject in an UCO
		// the base ctor doesn't take the native handle as a param, now we force it to do so...
		internal AppDelegate(ObjCRuntime.ManagedRegistrarHandleWrapper handle)
			: base(handle)
		{
		}

		public static new uint TypeId => 0;
	}
}

namespace ObjCRuntime
{
	public static class MySingleView_AppDelegate__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_FinishedLaunching")]
		public unsafe static byte callback_MySingleView_AppDelegate_FinishedLaunching(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(MySingleView.AppDelegate).GetMethod(nameof(MySingleView.AppDelegate.FinishedLaunching))!.MethodHandle; // ?? how to do thsi without reflection?
			try
			{
				MySingleView.AppDelegate nSObject = Runtime.GetNSObject<MySingleView.AppDelegate>(pobj, sel, methodHandle, true)!;
				UIApplication nSObject2 = Runtime.GetNSObject<UIApplication>(p0, sel, methodHandle, false)!;
				NSDictionary nSObject3 = Runtime.GetNSObject<NSDictionary>(p1, sel, methodHandle, false)!;
				return nSObject.FinishedLaunching(nSObject2, nSObject3) ? ((byte)1) : ((byte)0);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(byte);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate__ctor")]
		public unsafe static NativeHandle callback_MySingleView_AppDelegate__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// AppDelegate appDelegate = NSObject.AllocateNSObject<AppDelegate>(pobj);
				// appDelegate..ctor();
				MySingleView.AppDelegate appDelegate = new MySingleView.AppDelegate((ManagedRegistrarHandleWrapper)pobj);
				return NativeObjectExtensions.GetHandle(appDelegate);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}
	}

	internal sealed class AppKit_ActionDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_OnActivated")]
		public unsafe static void callback_AppKit_ActionDispatcher_OnActivated(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(AppKit.ActionDispatcher).GetMethod("OnActivated")!.MethodHandle;
			try
			{
				AppKit.ActionDispatcher nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, methodHandle, true)!;
				NSObject nSObject2 = Runtime.GetNSObject<NSObject>(p0, sel, methodHandle, false)!;
				nSObject.OnActivated(nSObject2);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_OnActivated2")]
		public unsafe static void callback_AppKit_ActionDispatcher_OnActivated2(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(AppKit.ActionDispatcher).GetMethod("OnActivated2")!.MethodHandle;
			try
			{
				AppKit.ActionDispatcher nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, methodHandle, true)!;
				NSObject nSObject2 = Runtime.GetNSObject<NSObject>(p0, sel, methodHandle, false)!;
				nSObject.OnActivated2(nSObject2);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_get_WorksWhenModal")]
		public unsafe static byte callback_AppKit_ActionDispatcher_get_WorksWhenModal(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(AppKit.ActionDispatcher).GetProperty("WorksWhenModal")!.GetMethod!.MethodHandle;
			try
			{
				AppKit.ActionDispatcher nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, methodHandle, true)!;
				return nSObject.WorksWhenModal ? ((byte)1) : ((byte)0);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(byte);
		}
	}

	internal sealed class UIKit_UIApplicationDelegate__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIApplicationDelegate__ctor")]
		public unsafe static NativeHandle callback_UIKit_UIApplicationDelegate__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// UIApplicationDelegate uIApplicationDelegate = NSObject.AllocateNSObject<UIApplicationDelegate>(pobj);
				// uIApplicationDelegate..ctor();
				UIKit.UIApplicationDelegate uIApplicationDelegate = new UIKit.UIApplicationDelegate((ManagedRegistrarHandleWrapper)pobj);
				return NativeObjectExtensions.GetHandle(uIApplicationDelegate);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}
	}

	internal sealed class Foundation_NSDispatcher__Registrar_Callbacks__
	{
		// !!!! NSDispatcher is abstract
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// Foundation.NSDispatcher nSDispatcher = NSObject.AllocateNSObject<Foundation.NSDispatcher>(pobj);
				// nSDispatcher..ctor();
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSDispatcher)}");
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(Foundation.NSDispatcher).GetMethod("Apply")!.MethodHandle;
			try
			{
				// TODO maybe we should pass the method name instead of the methodHandle?
				Foundation.NSDispatcher nSObject = Runtime.GetNSObject<Foundation.NSDispatcher>(pobj, sel, methodHandle, true)!;
				nSObject.Apply();
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}
	}

	internal sealed class Foundation_NSSynchronizationContextDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSSynchronizationContextDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSSynchronizationContextDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(Foundation.NSSynchronizationContextDispatcher).GetMethod("Apply")!.MethodHandle;
			try
			{
				Foundation.NSSynchronizationContextDispatcher nSObject = Runtime.GetNSObject<Foundation.NSSynchronizationContextDispatcher>(pobj, sel, methodHandle, true)!;
				nSObject.Apply();
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}
	}
	
	internal sealed class Foundation_NSObject_Disposer__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSObject_NSObject_Disposer__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSObject_NSObject_Disposer__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// Foundation.NSObject.NSObject_Disposer nSObject_Disposer = new Foundation.NSObject.NSObject_Disposer((ManagedRegistrarHandleWrapper)pobj);
				// return NativeObjectExtensions.GetHandle(nSObject_Disposer);
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSObject.NSObject_Disposer)}");
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSObject_NSObject_Disposer_Drain")]
		public unsafe static void callback_Foundation_NSObject_NSObject_Disposer_Drain(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			var methodHandle = typeof(Foundation.NSObject.NSObject_Disposer).GetMethod("Drain")!.MethodHandle;
			try
			{
				NSObject nSObject = Runtime.GetNSObject<NSObject>(p0, sel, methodHandle, false)!;
				Foundation.NSObject.NSObject_Disposer.Drain(nSObject);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}
	}
}
