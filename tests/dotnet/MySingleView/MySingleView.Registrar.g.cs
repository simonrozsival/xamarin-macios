#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Foundation;
using ObjCRuntime;
using UIKit;
using MySingleView;

namespace MySingleView
{
	partial class AppDelegate : ObjCRuntime.IManagedRegistrarType
	{
		// Important: If the type has its own static ctor, the `RegisterType` method must be called manually.
		// e.g. MessageSummaryView has a static ctor
		// BUT: - we need to make this specific to the ManagedStaticRegistrar

		// static ManagedStaticRegistrar() {
		// 	// ... existing code
		// 	// ...

		// 	if (Runtime.IsManagedStaticRegistrar) {
		// 		var instance = ManagedRegistrar.Register<T> ();
		// 	}
		// }

		static AppDelegate () => ManagedRegistrar.Register<AppDelegate> ();
		public static new NSObject CreateNSObject (NativeHandle handle) => new AppDelegate (handle);

		// used instead of the [Export("init")] ctor from NSObject in an UCO
		// the base ctor doesn't take the native handle as a param, now we force it to do so...
		internal AppDelegate (ObjCRuntime.ManagedRegistrarHandleWrapper handle) : base ((NativeHandle)handle)
		{
		}

		// TODO where did this constructor come from in the original code?
		protected internal AppDelegate (NativeHandle handle) : base (handle)
		{
		}

		public static new IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_MySingleView_AppDelegate (out is_custom_type);

		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_MySingleView_AppDelegate (out bool is_custom_type);
	}

	// partial class CustomGenericNSObject<T1, T2> : ObjCRuntime.IManagedRegistrarType
	// {
	// 	static CustomGenericNSObject() => ManagedRegistrar.Register<CustomGenericNSObject<T1, T2>> ();
	// 	public static NSObject CreateNSObject (NativeHandle handle) {
	// 		Console.WriteLine ($"creating a new instance via CustomGenericNSObject<{typeof(T1).Name}, {typeof(T2).Name}>.CreateNSObject({handle})");
	// 		return new CustomGenericNSObject<T1, T2> (handle);
	// 	}

	// 	public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_MySingleView_CustomGenericNSObject_2 (out is_custom_type);

	// 	[DllImport("__Internal")]
	// 	private static extern IntPtr get_objc_class_MySingleView_CustomGenericNSObject_2 (out bool is_custom_type);
	// }

	interface IGetDotnetType {
		Type GetDotnetType ();
	}

	// partial class CustomGenericNSObject<T1, T2> : IGetDotnetType
	// {
	// 	public Type GetDotnetType () => typeof (CustomGenericNSObject<T1, T2>);
	// }
}

namespace ObjCRuntime
{
	public static class MySingleView_AppDelegate__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_FinishedLaunching")]
		public unsafe static byte callback_MySingleView_AppDelegate_FinishedLaunching(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
		{
			// var methodHandle = MySingleView.AppDelegate.FinishedLaunching.GetMethodInfo().MethodHandle; // ?? how to do thsi without reflection?
			RuntimeMethodHandle methodHandle = default;
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
				MySingleView.AppDelegate appDelegate = new MySingleView.AppDelegate(new ObjCRuntime.ManagedRegistrarHandleWrapper (new NativeHandle (pobj)));
				return NativeObjectExtensions.GetHandle(appDelegate);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_GetDotnetType")]
		public unsafe static IntPtr callback_MySingleView_AppDelegate_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (MySingleView.AppDelegate));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	// public static class MySingleView_CustomGenericNSObject_2__Registrar_Callbacks__
	// {
	// 	[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_CustomGenericNSObject_2_GetDotnetType")]
	// 	public unsafe static IntPtr callback_MySingleView_CustomGenericNSObject_2_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
	// 	{
	// 		try
	// 		{
	// 			if (Runtime.HasNSObject (pobj) != 0) {
	// 				IGetDotnetType obj = (IGetDotnetType) Runtime.GetNSObject (pobj);
	// 				return Runtime.AllocGCHandle (obj.GetDotnetType ());
	// 			}

	// 			// does this even make sense?
	// 			// all of the generic params should be constrained to NSObjects if I'm not wrong...
	// 			return Runtime.AllocGCHandle (typeof (MySingleView.CustomGenericNSObject<Foundation.NSObject, Foundation.NSObject>));
	// 		}
	// 		catch (Exception ex)
	// 		{
	// 			*exception_gchandle = Runtime.AllocGCHandle(ex);
	// 		}
	// 		return IntPtr.Zero;
	// 	}
	// }

#if __MACCATALYST__
	internal sealed class AppKit_ActionDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_OnActivated")]
		public unsafe static void callback_AppKit_ActionDispatcher_OnActivated(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			// var methodHandle = typeof(AppKit.ActionDispatcher).GetMethod("OnActivated")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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
			// var methodHandle = typeof(AppKit.ActionDispatcher).GetMethod("OnActivated2")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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
			// var methodHandle = typeof(AppKit.ActionDispatcher).GetProperty("WorksWhenModal")!.GetMethod!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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

		[UnmanagedCallersOnly(EntryPoint = "_callback___monomac_internal_ActionDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_AppKit_ActionDispatcher_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (AppKit.ActionDispatcher));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}
#endif

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
				UIKit.UIApplicationDelegate uIApplicationDelegate = new UIKit.UIApplicationDelegate(new ManagedRegistrarHandleWrapper (new NativeHandle (pobj)));
				return NativeObjectExtensions.GetHandle(uIApplicationDelegate);
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIApplicationDelegate_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIApplicationDelegate));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIControlEventProxy__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControlEventProxy_Activated")]
		public unsafe static void callback_UIKit_UIControlEventProxy_Activated(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			RuntimeMethodHandle methodHandle = default;
			try
			{
				// TODO maybe we should pass the method name instead of the methodHandle?
				UIKit.UIControlEventProxy nSObject = Runtime.GetNSObject<UIKit.UIControlEventProxy>(pobj, sel, methodHandle, true)!;
				nSObject.Activated();
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControlEventProxy_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIControlEventProxy_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIControlEventProxy));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
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
			// var methodHandle = typeof(Foundation.NSDispatcher).GetMethod("Apply")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSDispatcher_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSDispatcher));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSAsyncDispatcher__Registrar_Callbacks__
	{
		// !!!! NSAsyncDispatcher is abstract
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSAsyncDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// Foundation.NSAsyncDispatcher nSDispatcher = NSObject.AllocateNSObject<Foundation.NSAsyncDispatcher>(pobj);
				// nSDispatcher..ctor();
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSDispatcher)}");
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSAsyncDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			// var methodHandle = typeof(Foundation.NSAsyncDispatcher).GetMethod("Apply")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
			try
			{
				// TODO maybe we should pass the method name instead of the methodHandle?
				Foundation.NSAsyncDispatcher nSObject = Runtime.GetNSObject<Foundation.NSAsyncDispatcher>(pobj, sel, methodHandle, true)!;
				nSObject.Apply();
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAsyncDispatcher_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSAsyncDispatcher));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSAsyncSynchronizationContextDispatcher__Registrar_Callbacks__
	{
		// !!!! NSAsyncSynchronizationContextDispatcher is abstract
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncSynchronizationContextDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSAsyncSynchronizationContextDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try
			{
				if (Runtime.HasNSObject(pobj) != 0)
				{
					*call_super = 1;
					return pobj;
				}
				// Foundation.NSAsyncSynchronizationContextDispatcher nSDispatcher = NSObject.AllocateNSObject<Foundation.NSAsyncSynchronizationContextDispatcher>(pobj);
				// nSDispatcher..ctor();
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSDispatcher)}");
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncSynchronizationContextDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSAsyncSynchronizationContextDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			// var methodHandle = typeof(Foundation.NSAsyncSynchronizationContextDispatcher).GetMethod("Apply")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
			try
			{
				// TODO maybe we should pass the method name instead of the methodHandle?
				Foundation.NSAsyncSynchronizationContextDispatcher nSObject = Runtime.GetNSObject<Foundation.NSAsyncSynchronizationContextDispatcher>(pobj, sel, methodHandle, true)!;
				nSObject.Apply();
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback___MonoMac_NSAsyncSynchronizationContextDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAsyncSynchronizationContextDispatcher_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSAsyncSynchronizationContextDispatcher));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSSynchronizationContextDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSSynchronizationContextDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSSynchronizationContextDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			// var methodHandle = typeof(Foundation.NSSynchronizationContextDispatcher).GetMethod("Apply")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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

		[UnmanagedCallersOnly(EntryPoint = "_callback___MonoMac_NSSynchronizationContextDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSSynchronizationContextDispatcher_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSSynchronizationContextDispatcher));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
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
			// var methodHandle = typeof(Foundation.NSObject.NSObject_Disposer).GetMethod("Drain")!.MethodHandle;
			RuntimeMethodHandle methodHandle = default;
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

		[UnmanagedCallersOnly(EntryPoint = "_callback___NSObject_Disposer_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSObject_NSObject_Disposer_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSObject.NSObject_Disposer));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}
	
	internal sealed class Foundation_NSException__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSException_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSException_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSException));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIApplication__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIApplication_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIApplication_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIApplication));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIResponder__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIResponder_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIResponder_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIResponder));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIViewController__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIViewController_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIViewController_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIViewController));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIView__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIView_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIView_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIView));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIControl__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControl_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIControl_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIControl));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIButton__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIButton_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIButton_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIButton));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIScreen__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIScreen_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIScreen_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIScreen));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class UIKit_UIWindow__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIWindow_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIWindow_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (UIKit.UIWindow));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSRunLoop__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSRunLoop_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSRunLoop_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSRunLoop));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSAutoreleasePool__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAutoreleasePool_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAutoreleasePool_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSAutoreleasePool));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}

	internal sealed class Foundation_NSDictionary__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDictionary_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSDictionary_GetDotnetType(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try
			{
				return Runtime.AllocGCHandle (typeof (Foundation.NSDictionary));
			}
			catch (Exception ex)
			{
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return IntPtr.Zero;
		}
	}
}
