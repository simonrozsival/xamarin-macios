#define USE_STATIC_CTORS
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using ObjCRuntime;
using MySingleView;

namespace MySingleView
{
	static class RegistrarInit {
		[ModuleInitializer]
		public static void InitializeManagedStaticRegistrar () {
			initialize_managed_static_registrar ();
		}

		[DllImport ("__Internal")]
		public extern static void initialize_managed_static_registrar ();
	}

	partial class AppDelegate : ObjCRuntime.IManagedRegistrarType
	{
		// Important: If the type has its own static ctor, the `RegisterType` method must be called manually.
		// e.g. MessageSummaryView has a static ctor
		// BUT: - we need to make this specific to the ManagedStaticRegistrar

		// static ManagedStaticRegistrar() {
		// 	// ... existing code
		// 	// ...

		// 	if (Runtime.IsManagedStaticRegistrar) {
		// 		var instance = RegistrarHelper.Register<T> ();
		// 	}
		// }

#if USE_STATIC_CTORS
		static AppDelegate() => RegistrarHelper.Register<AppDelegate> ();
#endif
		public static new bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_AppDelegate");
		public static new Foundation.NSObject CreateNSObject (NativeHandle handle) => new AppDelegate (handle);

		// TODO: doesn't work with base classes that don't have _shadow_ constructors
		// // used instead of the [Export("init")] ctor from NSObject in an UCO
		// // the base ctor doesn't take the native handle as a param, now we force it to do so...
		// internal AppDelegate (ObjCRuntime.ManagedRegistrarHandleWrapper handle) : base (handle)
		// {
		// }

		// TODO where did this constructor come from in the original code?
		protected internal AppDelegate (NativeHandle handle) : base (handle)
		{
		}
	}

	// partial class CustomGenericNSObject<T1, T2> : ObjCRuntime.IManagedRegistrarType
	// {
	// 	static CustomGenericNSObject() => RegistrarHelper.Register<CustomGenericNSObject<T1, T2>> ();
	// 	public static NSObject CreateNSObject (NativeHandle handle) {
	// 		Console.WriteLine ($"creating a new instance via CustomGenericNSObject<{typeof(T1).Name}, {typeof(T2).Name}>.CreateNSObject({handle})");
	// 		return new CustomGenericNSObject<T1, T2> (handle);
	// 	}

	// 	public static NativeHandle GetNativeClass () => Class.GetHandle("MySingleView_CustomGenericNSObject");
	// }

	// interface IGetDotnetType {
	// 	Type GetDotnetType ();
	// }

	// partial class CustomGenericNSObject<T1, T2> : IGetDotnetType
	// {
	// 	public Type GetDotnetType () => typeof (CustomGenericNSObject<T1, T2>);
	// }
	
	partial class AnyT<T> : ObjCRuntime.IManagedRegistrarType
	{
#if USE_STATIC_CTORS
		static AnyT () => RegistrarHelper.Register<AnyT<T>> ();
#endif
		public static bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_AnyT_1");
	}

	public static class AnyT_1__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AnyT_1_GetDotnetType")]
		public unsafe static IntPtr callback_MySingleView_AnyT_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<MySingleView.AnyT<Foundation.NSObject>> (exception_gchandle);

		// [UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_AnyT")]
		// public static unsafe void _register_MySingleView_AnyT(IntPtr* exception_gchandle)
		// 	=> RegistrarHelper.Register<MySingleView.Any<Foundation.NSObject>> (exception_gchandle);
	}

	partial class ConformsToProtocolTestClass : ObjCRuntime.IManagedRegistrarType
	{
#if USE_STATIC_CTORS
		static ConformsToProtocolTestClass () => RegistrarHelper.Register<ConformsToProtocolTestClass> ();
#endif
		public static bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_ConformsToProtocolTestClass");
		public static class __Registrar_Callbacks__
		{
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_ConformsToProtocolTestClass_GetDotnetType")]
			public unsafe static IntPtr callback_MySingleView_ConformsToProtocolTestClass_GetDotnetType(IntPtr* exception_gchandle)
				=> RegistrarHelper.GetDotnetType<MySingleView.ConformsToProtocolTestClass> (exception_gchandle);

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_ConformsToProtocolTestClass__ctor")]
			public static unsafe IntPtr callback_MySingleView_ConformsToProtocolTestClass__ctor(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					if (Runtime.HasNSObject (pobj) != 0) {
						ConformsToProtocolTestClass obj = (ConformsToProtocolTestClass) Runtime.GetNSObject (pobj);
						return Runtime.AllocGCHandle (obj);
					}

					var inst = new ConformsToProtocolTestClass (pobj);
					return Runtime.AllocGCHandle (inst);
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			// [UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_AnyT")]
			// public static unsafe void _register_MySingleView_AnyT(IntPtr* exception_gchandle)
			// 	=> RegistrarHelper.Register<MySingleView.Any<Foundation.NSObject>> (exception_gchandle);
		}
	}

	partial class Test24970 : ObjCRuntime.IManagedRegistrarType
	{
#if USE_STATIC_CTORS
		static Test24970 () => RegistrarHelper.Register<Test24970> ();
#endif
		public static bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_Test24970");
		public static class __Registrar_Callbacks__
		{
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_Test24970_GetDotnetType")]
			public unsafe static IntPtr callback_MySingleView_Test24970_GetDotnetType(IntPtr* exception_gchandle)
				=> RegistrarHelper.GetDotnetType<MySingleView.Test24970> (exception_gchandle);

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_Test24970__ctor")]
			public static unsafe IntPtr callback_MySingleView_Test24970__ctor(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					// if (Runtime.HasNSObject (pobj) != 0) {
					// 	Test24970 obj = (Test24970) Runtime.GetNSObject (pobj);
					// 	return Runtime.AllocGCHandle (obj);
					// }
					throw new NotImplementedException ();
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_Test24970_GetSupportedInterfaceOrientations")]
			public static unsafe ulong callback_MySingleView_Test24970_GetSupportedInterfaceOrientations(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
			{
				try {
					var nSObject = Runtime.GetNSObject<Test24970>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
					var nSObject2 = Runtime.GetNSObject<UIKit.UIApplication>(p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
					var nSObject3 = Runtime.GetNSObject<UIKit.UIWindow>(p1, sel, method_handle: default, evenInFinalizerQueue: false)!;
					return (ulong) nSObject.GetSupportedInterfaceOrientations (nSObject2, nSObject3);
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			// [UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_AnyT")]
			// public static unsafe void _register_MySingleView_AnyT(IntPtr* exception_gchandle)
			// 	=> RegistrarHelper.Register<MySingleView.Any<Foundation.NSObject>> (exception_gchandle);
		}
	}

	partial class ConformsToProtocolTestClass<T> : ObjCRuntime.IManagedRegistrarType
	{
#if USE_STATIC_CTORS
		static ConformsToProtocolTestClass () => RegistrarHelper.Register<ConformsToProtocolTestClass<T>> ();
#endif
		public static bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_ConformsToProtocolTestClass_1");
	}

	public static class ConformsToProtocolTestClass_1__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_ConformsToProtocolTestClass_1_GetDotnetType")]
		public unsafe static IntPtr callback_MySingleView_ConformsToProtocolTestClass_1_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<MySingleView.ConformsToProtocolTestClass<Foundation.NSObject>> (exception_gchandle);

		// [UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_AnyT")]
		// public static unsafe void _register_MySingleView_AnyT(IntPtr* exception_gchandle)
		// 	=> RegistrarHelper.Register<MySingleView.Any<Foundation.NSObject>> (exception_gchandle);
	}

	partial class MyProtocolImplementation : ObjCRuntime.IManagedRegistrarType
	{
#if USE_STATIC_CTORS
		static MyProtocolImplementation () => RegistrarHelper.Register<MyProtocolImplementation> ();
#endif
		public static bool IsCustomType => true;
		public static new NativeHandle GetNativeClass () => Class.GetHandle ("MySingleView_MyProtocolImplementation");
	}

	public static class MyProtocolImplementation__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_GetDotnetType")]
		public unsafe static IntPtr callback_MySingleView_MyProtocolImplementation_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<MyProtocolImplementation> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_MyProtocolImplementation")]
		public static unsafe void _register_MySingleView_MyProtocolImplementation(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<MyProtocolImplementation> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation__ctor")]
		public static unsafe IntPtr callback_MySingleView_MyProtocolImplementation__ctor(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject (pobj) != 0) {
					MyProtocolImplementation obj = (MyProtocolImplementation) Runtime.GetNSObject (pobj);
					return Runtime.AllocGCHandle (obj);
				}

				// var inst = new MyProtocolImplementation (pobj);
				// return Runtime.AllocGCHandle (inst);
				throw new NotImplementedException();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
				return default;
			}
		}
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_Foo")]
		public static unsafe int callback_MySingleView_MyProtocolImplementation_Foo(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MyProtocolImplementation>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				return nSObject.Foo ();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
				return default;
			}
		}
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_get_Bar")]
		public static unsafe int callback_MySingleView_MyProtocolImplementation_get_Bar(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MyProtocolImplementation>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				return nSObject.Bar;
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
				return default;
			}
		}
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_set_Bar")]
		public static unsafe void callback_MySingleView_MyProtocolImplementation_set_Bar(IntPtr pobj, IntPtr sel, int p0, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MyProtocolImplementation>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Bar = p0;
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
			}
		}
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_get_Block")]
		public static unsafe IntPtr callback_MySingleView_MyProtocolImplementation_get_Block(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MyProtocolImplementation>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				return Runtime.AllocGCHandle (nSObject.Block);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
				return default;
			}
		}
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_MyProtocolImplementation_set_Block")]
		public static unsafe void callback_MySingleView_MyProtocolImplementation_set_Block(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MyProtocolImplementation>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Block = GCHandle.FromIntPtr (p0).Target as System.Action;
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle (ex);
			}
		}
	}

	partial class RegistrarTestClass : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static RegistrarTestClass () => RegistrarHelper.Register<RegistrarTestClass> ();
#endif
		public static bool IsCustomType => true;
		public static Foundation.NSObject CreateNSObject (NativeHandle handle) => new RegistrarTestClass (handle);
		public RegistrarTestClass () : base () {}
		protected internal RegistrarTestClass (NativeHandle handle) : base (handle) {}
		public static NativeHandle GetNativeClass () => Class.GetHandle ("RegistrarTestClass");

		public static class __Registrar_Callbacks__
		{
			// [UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_FinishedLaunching")]
			// public unsafe static byte callback_MySingleView_RegistrarTestClass_FinishedLaunching(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
			// {
			// 	try {
			// 		var nSObject = Runtime.GetNSObject<MySingleView.AppDelegate>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
			// 		var nSObject2 = Runtime.GetNSObject<UIKit.UIApplication>(p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
			// 		var nSObject3 = Runtime.GetNSObject<Foundation.NSDictionary>(p1, sel, method_handle: default, evenInFinalizerQueue: false)!;
			// 		return nSObject.FinishedLaunching(nSObject2, nSObject3) ? ((byte)1) : ((byte)0);
			// 	} catch (Exception ex) {
			// 		*exception_gchandle = Runtime.AllocGCHandle(ex);
			// 	}
			// 	return default(byte);
			// }

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_FetchComplexGenericType")]
			public static unsafe IntPtr callback_MySingleView_RegistrarTestClass_FetchComplexGenericType(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
			{
				try {
					var nSObject = Runtime.GetNSObject<MySingleView.RegistrarTestClass>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
					var nSObject2 = Runtime.GetNSObject<Foundation.NSArray<Foundation.NSDictionary<Foundation.NSString, Foundation.NSArray<Foundation.NSNumber>>>> (p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
					return nSObject.FetchComplexGenericType(nSObject2).Handle;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_FetchNSArrayOfNSString")]
			public static unsafe IntPtr callback_MySingleView_RegistrarTestClass_FetchNSArrayOfNSString(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
			{
				try {
					var nSObject = Runtime.GetNSObject<MySingleView.RegistrarTestClass>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
					var nSObject2 = Runtime.GetNSObject<Foundation.NSArray<Foundation.NSNumber>> (p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
					// var arr = Foundation.NSArray.ArrayFromHandle<Foundation.NSNumber> (p0);
					// var nSObject2 = arr is not null ? Foundation.NSArray<Foundation.NSNumber>.FromNSObjects (arr) : null;
					// var nSObject2 = arr is not null ? Foundation.NSArray<Foundation.NSNumber>.FromNSObjects (arr) : null;
					// return Runtime.AllocGCHandle (nSObject.FetchNSArrayOfNSString(nSObject2));
					return nSObject.FetchNSArrayOfNSString(nSObject2).Handle; // ???!!
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return IntPtr.Zero;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_INativeObject1")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_INativeObject1(IntPtr ptr, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
			{
				try {
					var o0 = Runtime.GetINativeObject<CoreGraphics.CGPath> (p0, false);
					return MySingleView.RegistrarTestClass.INativeObject1 (o0) ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_INativeObject2")]
			public static unsafe IntPtr callback_MySingleView_RegistrarTestClass_INativeObject2(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.INativeObject2 (p0 == (byte)1)?.Handle ?? IntPtr.Zero;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_INativeObject3")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_INativeObject3(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_INativeObject4")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_INativeObject4(IntPtr ptr, IntPtr sel, IntPtr* p0, IntPtr* exception_gchandle)
			{
				try {
					var o0 = Runtime.GetINativeObject<CoreGraphics.CGPath> (*p0, false);
					var rv = MySingleView.RegistrarTestClass.INativeObject4 (ref o0);
					*p0 = o0?.Handle ?? IntPtr.Zero;
					return rv ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_INativeObject5")]
			public static unsafe IntPtr callback_MySingleView_RegistrarTestClass_INativeObject5(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.INativeObject5 (p0 == (byte)1)?.Handle ?? IntPtr.Zero;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestBug23289")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestBug23289(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestCGPoint")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestCGPoint(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestNSAction")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestNSAction(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestNativeEnum1")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestNativeEnum1(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestOutNSString")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestOutNSString(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestOutParameters")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestOutParameters(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestOverriddenRetainNSObject")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestOverriddenRetainNSObject(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestRetainArray")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestRetainArray(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestRetainINativeObject")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestRetainINativeObject(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestRetainNSObject")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestRetainNSObject(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_TestRetainString")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_TestRetainString(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_VirtualMethod")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_VirtualMethod(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass__ctor")]
			public static unsafe void callback_MySingleView_RegistrarTestClass__ctor(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_B1")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_B1 (IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.B1 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_NSArrayOfNSString")]
			public static unsafe IntPtr callback_MySingleView_RegistrarTestClass_get_NSArrayOfNSString(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.NSArrayOfNSString.Handle;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_NativeObjects")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_get_NativeObjects(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_Property1")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_Property1(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.Property1 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_Property2")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_Property2(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.Property2 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_Property4")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_Property4(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.Property4 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_Property5")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_Property5(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return obj.Property5 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_StaticProperty1")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_StaticProperty1(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.StaticProperty1 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_StaticProperty2")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_StaticProperty2(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.StaticProperty2 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_StaticProperty4")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_StaticProperty4(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.StaticProperty4 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_StaticProperty5")]
			public static unsafe byte callback_MySingleView_RegistrarTestClass_get_StaticProperty5(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					return MySingleView.RegistrarTestClass.StaticProperty5 ? (byte)1 : (byte)0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_get_TestNativeEnum2")]
			public static unsafe int callback_MySingleView_RegistrarTestClass_get_TestNativeEnum2(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					return (int)obj.TestNativeEnum2;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
					return default;
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_NSArrayOfNSString")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_NSArrayOfNSString(IntPtr ptr, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					var o0 = Runtime.GetNSObject<Foundation.NSArray<Foundation.NSString>> (p0, sel, method_handle: default, evenInFinalizerQueue: false);
					obj.NSArrayOfNSString = o0;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_Property2")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_Property2(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					obj.Property2 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_Property3")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_Property3(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					obj.Property3 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_Property5")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_Property5(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					obj.Property5 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_Property6")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_Property6(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					var obj = Runtime.GetNSObject<MySingleView.RegistrarTestClass> (ptr, sel, method_handle: default, evenInFinalizerQueue: true);
					obj.Property6 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}
			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_StaticProperty2")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_StaticProperty2(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					MySingleView.RegistrarTestClass.StaticProperty2 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_StaticProperty3")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_StaticProperty3(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					MySingleView.RegistrarTestClass.StaticProperty3 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_StaticProperty5")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_StaticProperty5(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					MySingleView.RegistrarTestClass.StaticProperty5 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_StaticProperty6")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_StaticProperty6(IntPtr ptr, IntPtr sel, byte p0, IntPtr* exception_gchandle)
			{
				try {
					MySingleView.RegistrarTestClass.StaticProperty6 = p0 == (byte)1;
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_RegistrarTestClass_set_TestNativeEnum2")]
			public static unsafe void callback_MySingleView_RegistrarTestClass_set_TestNativeEnum2(IntPtr ptr, IntPtr sel, IntPtr* exception_gchandle)
			{
				try {
					
				} catch (Exception ex) {
					*exception_gchandle = Runtime.AllocGCHandle (ex);
				}
			}

			[UnmanagedCallersOnly(EntryPoint = "_callback_RegistrarTestClass_GetDotnetType")]
			public static unsafe IntPtr callback_RegistrarTestClass_GetDotnetType(IntPtr* exception_gchandle)
				=> RegistrarHelper.GetDotnetType<MySingleView.RegistrarTestClass> (exception_gchandle);

			[UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_RegistrarTestClass")]
			public static unsafe IntPtr register_MySingleView_RegistrarTestClass(IntPtr* exception_gchandle)
				=> RegistrarHelper.GetDotnetType<MySingleView.RegistrarTestClass> (exception_gchandle);
		}
	}
}

namespace ObjCRuntime
{
	public static class MySingleView_AppDelegate__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_FinishedLaunching")]
		public unsafe static byte callback_MySingleView_AppDelegate_FinishedLaunching(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr p1, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<MySingleView.AppDelegate>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				var nSObject2 = Runtime.GetNSObject<UIKit.UIApplication>(p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
				var nSObject3 = Runtime.GetNSObject<Foundation.NSDictionary>(p1, sel, method_handle: default, evenInFinalizerQueue: false)!;
				return nSObject.FinishedLaunching(nSObject2, nSObject3) ? ((byte)1) : ((byte)0);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(byte);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate__ctor")]
		public unsafe static NativeHandle callback_MySingleView_AppDelegate__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				Console.WriteLine($"Creating {nameof(MySingleView.AppDelegate)} through the MSR...");
				// AppDelegate appDelegate = NSObject.AllocateNSObject<AppDelegate>(pobj);
				// appDelegate..ctor();
				// var appDelegate = new MySingleView.AppDelegate(new ObjCRuntime.ManagedRegistrarHandleWrapper (pobj));
				var appDelegate = new MySingleView.AppDelegate(pobj);
				Console.WriteLine($"Got an instance.");
				return NativeObjectExtensions.GetHandle(appDelegate);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_AppDelegate_GetDotnetType")]
		public unsafe static IntPtr callback_MySingleView_AppDelegate_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<MySingleView.AppDelegate> (exception_gchandle);
			
		[UnmanagedCallersOnly(EntryPoint = "_register_MySingleView_AppDelegate")]
		public static unsafe void _register_MySingleView_AppDelegate(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<MySingleView.AppDelegate> (exception_gchandle);
	}

	// public static class MySingleView_CustomGenericNSObject_2__Registrar_Callbacks__
	// {
	// 	[UnmanagedCallersOnly(EntryPoint = "_callback_MySingleView_CustomGenericNSObject_2_GetDotnetType")]
	// 	public unsafe static IntPtr callback_MySingleView_CustomGenericNSObject_2_GetDotnetType(IntPtr* exception_gchandle)
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
			try {
				var nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				var nSObject2 = Runtime.GetNSObject<Foundation.NSObject>(p0,  sel, method_handle: default, evenInFinalizerQueue: false)!;
				nSObject.OnActivated(nSObject2);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_OnActivated2")]
		public unsafe static void callback_AppKit_ActionDispatcher_OnActivated2(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				var nSObject2 = Runtime.GetNSObject<Foundation.NSObject>(p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
				nSObject.OnActivated2(nSObject2);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_AppKit_ActionDispatcher_get_WorksWhenModal")]
		public unsafe static byte callback_AppKit_ActionDispatcher_get_WorksWhenModal(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<AppKit.ActionDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				return nSObject.WorksWhenModal ? ((byte)1) : ((byte)0);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(byte);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback___monomac_internal_ActionDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_AppKit_ActionDispatcher_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<AppKit.ActionDispatcher> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register___monomac_internal_ActionDispatcher")]
		public unsafe static void register_AppKit_ActionDispatcher(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<AppKit.ActionDispatcher> (exception_gchandle);
	}
#endif

	internal sealed class UIKit_UIApplicationDelegate__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIApplicationDelegate__ctor")]
		public unsafe static NativeHandle callback_UIKit_UIApplicationDelegate__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				// UIApplicationDelegate uIApplicationDelegate = NSObject.AllocateNSObject<UIApplicationDelegate>(pobj);
				// uIApplicationDelegate..ctor();
				// var uIApplicationDelegate = new UIKit.UIApplicationDelegate(new ManagedRegistrarHandleWrapper (pobj));
				// var uIApplicationDelegate = new UIKit.UIApplicationDelegate(pobj);
				// return NativeObjectExtensions.GetHandle(uIApplicationDelegate);
				throw new InvalidOperationException ($"Unable to activate instance of type {nameof(UIKit.UIApplicationDelegate)}");
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIApplicationDelegate_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIApplicationDelegate> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate")]
		public unsafe static void register_UIKit_UIApplicationDelegate(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIApplicationDelegate> (exception_gchandle);
	}

	internal sealed class UIKit_UIControlEventProxy__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControlEventProxy_Activated")]
		public unsafe static void callback_UIKit_UIControlEventProxy_Activated(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<UIKit.UIControlEventProxy>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Activated();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControlEventProxy_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIControlEventProxy_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIControlEventProxy> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIControlEventProxy")]
		public unsafe static void register_UIKit_UIControlEventProxy(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIControlEventProxy> (exception_gchandle);
	}

	internal sealed class Foundation_NSDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				// TODO: NSDispatcher is abstract -> why are we generating this code in the MSR:?
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
			try {
				var nSObject = Runtime.GetNSObject<Foundation.NSDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Apply();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSDispatcher_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSDispatcher> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSDispatcher")]
		public unsafe static void register_Foundation_NSDispatcher(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSDispatcher> (exception_gchandle);
	}

	internal sealed class Foundation_NSAsyncDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSAsyncDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				// TODO: NSAsyncDispatcher is abstract -> why are we generating this code in the MSR:?
				// Foundation.NSAsyncDispatcher nSDispatcher = NSObject.AllocateNSObject<Foundation.NSAsyncDispatcher>(pobj);
				// nSDispatcher..ctor();
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSDispatcher)}");
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSAsyncDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {

				var nSObject = Runtime.GetNSObject<Foundation.NSAsyncDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Apply();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAsyncDispatcher_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSAsyncDispatcher> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSAsyncDispatcher")]
		public unsafe static void register_Foundation_NSAsyncDispatcher(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSAsyncDispatcher> (exception_gchandle);
	}

	internal sealed class Foundation_NSAsyncSynchronizationContextDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncSynchronizationContextDispatcher__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSAsyncSynchronizationContextDispatcher__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				// TODO: NSAsyncSynchronizationContextDispatcher is abstract -> why are we generating this code in the MSR:?
				// Foundation.NSAsyncSynchronizationContextDispatcher nSDispatcher = NSObject.AllocateNSObject<Foundation.NSAsyncSynchronizationContextDispatcher>(pobj);
				// nSDispatcher..ctor();
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSDispatcher)}");
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAsyncSynchronizationContextDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSAsyncSynchronizationContextDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {

				var nSObject = Runtime.GetNSObject<Foundation.NSAsyncSynchronizationContextDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Apply();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback___MonoMac_NSAsyncSynchronizationContextDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAsyncSynchronizationContextDispatcher_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSAsyncSynchronizationContextDispatcher> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSAsyncSynchronizationContextDispatcher")]
		public unsafe static void register_Foundation_NSAsyncSynchronizationContextDispatcher(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSAsyncSynchronizationContextDispatcher> (exception_gchandle);
	}

	internal sealed class Foundation_NSSynchronizationContextDispatcher__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSSynchronizationContextDispatcher_Apply")]
		public unsafe static void callback_Foundation_NSSynchronizationContextDispatcher_Apply(IntPtr pobj, IntPtr sel, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<Foundation.NSSynchronizationContextDispatcher>(pobj, sel, method_handle: default, evenInFinalizerQueue: true)!;
				nSObject.Apply();
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback___MonoMac_NSSynchronizationContextDispatcher_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSSynchronizationContextDispatcher_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSSynchronizationContextDispatcher> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSSynchronizationContextDispatcher")]
		public unsafe static void register_Foundation_NSSynchronizationContextDispatcher(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSSynchronizationContextDispatcher> (exception_gchandle);
	}
	
	internal sealed class Foundation_NSObject_Disposer__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSObject_NSObject_Disposer__ctor")]
		public unsafe static NativeHandle callback_Foundation_NSObject_NSObject_Disposer__ctor(IntPtr pobj, IntPtr sel, byte* call_super, IntPtr* exception_gchandle)
		{
			try {
				if (Runtime.HasNSObject(pobj) != 0) {
					*call_super = 1;
					return pobj;
				}
				// Foundation.NSObject.NSObject_Disposer nSObject_Disposer = new Foundation.NSObject.NSObject_Disposer(pobj);
				// return NativeObjectExtensions.GetHandle(nSObject_Disposer);
				throw new InvalidOperationException($"Cannot create an instance of an abstract class {nameof(Foundation.NSObject.NSObject_Disposer)}");
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
			return default(NativeHandle);
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSObject_NSObject_Disposer_Drain")]
		public unsafe static void callback_Foundation_NSObject_NSObject_Disposer_Drain(IntPtr pobj, IntPtr sel, IntPtr p0, IntPtr* exception_gchandle)
		{
			try {
				var nSObject = Runtime.GetNSObject<Foundation.NSObject.NSObject_Disposer>(p0, sel, method_handle: default, evenInFinalizerQueue: false)!;
				Foundation.NSObject.NSObject_Disposer.Drain(nSObject);
			} catch (Exception ex) {
				*exception_gchandle = Runtime.AllocGCHandle(ex);
			}
		}

		[UnmanagedCallersOnly(EntryPoint = "_callback___NSObject_Disposer_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSObject_NSObject_Disposer_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSObject.NSObject_Disposer> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSObject_Disposer")]
		public unsafe static void register_Foundation_NSObject_NSObject_Disposer(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSObject.NSObject_Disposer> (exception_gchandle);
	}
	
	internal sealed class Foundation_NSException__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSException_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSException_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSException> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSException")]
		public unsafe static void register_Foundation_NSException(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSException> (exception_gchandle);
	}

	internal sealed class UIKit_UIApplication__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIApplication_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIApplication_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIApplication> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIApplication")]
		public unsafe static void register_UIKit_UIApplication(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIApplication> (exception_gchandle);
	}

	internal sealed class UIKit_UIResponder__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIResponder_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIResponder_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIResponder> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIResponder")]
		public unsafe static void register_UIKit_UIResponder(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIResponder> (exception_gchandle);
	}

	internal sealed class UIKit_UIViewController__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIViewController_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIViewController_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIViewController> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIViewController")]
		public unsafe static void register_UIKit_UIViewController(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIViewController> (exception_gchandle);
	}

	internal sealed class UIKit_UIView__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIView_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIView_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIView> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIView")]
		public unsafe static void register_UIKit_UIView(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIView> (exception_gchandle);
	}

	internal sealed class UIKit_UIControl__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIControl_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIControl_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIControl> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIControl")]
		public unsafe static void register_UIKit_UIControl(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIControl> (exception_gchandle);
	}

	internal sealed class UIKit_UIButton__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIButton_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIButton_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIButton> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIButton")]
		public unsafe static void register_UIKit_UIButton(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIButton> (exception_gchandle);
	}

	internal sealed class UIKit_UIScreen__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIScreen_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIScreen_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIScreen> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIScreen")]
		public unsafe static void register_UIKit_UIScreen(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIScreen> (exception_gchandle);
	}

	internal sealed class UIKit_UIWindow__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_UIKit_UIWindow_GetDotnetType")]
		public unsafe static IntPtr callback_UIKit_UIWindow_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<UIKit.UIWindow> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_UIKit_UIWindow")]
		public unsafe static void register_UIKit_UIWindow(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<UIKit.UIWindow> (exception_gchandle);
	}

	internal sealed class Foundation_NSRunLoop__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSRunLoop_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSRunLoop_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSRunLoop> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSRunLoop")]
		public unsafe static void register_Foundation_NSRunLoop(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSRunLoop> (exception_gchandle);
	}

	internal sealed class Foundation_NSAutoreleasePool__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSAutoreleasePool_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSAutoreleasePool_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSAutoreleasePool> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSAutoreleasePool")]
		public unsafe static void register_Foundation_NSAutoreleasePool(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSAutoreleasePool> (exception_gchandle);
	}

	internal sealed class Foundation_NSDictionary__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSDictionary_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSDictionary_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSDictionary> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSDictionary")]
		public unsafe static void register_Foundation_NSDictionary(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSDictionary> (exception_gchandle);
	}

	internal sealed class Foundation_NSString__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSString_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSString_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSString> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSString")]
		public unsafe static void register_Foundation_NSString(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSString> (exception_gchandle);
	}

	internal sealed class Foundation_NSNumber__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSNumber_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSNumber_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSNumber> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSNumber")]
		public unsafe static void register_Foundation_NSNumber(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSNumber> (exception_gchandle);
	}

	internal sealed class Foundation_NSArray__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSArray_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSNumber_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSArray<Foundation.NSObject>> (exception_gchandle);

		// TODO: This doesn't solve the problem with generic types
		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSArray")]
		public unsafe static void register_Foundation_NSNumber(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSArray<Foundation.NSObject>> (exception_gchandle);
	}

	internal sealed class Foundation_NSNull__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_callback_Foundation_NSNull_GetDotnetType")]
		public unsafe static IntPtr callback_Foundation_NSNumber_GetDotnetType(IntPtr* exception_gchandle)
			=> RegistrarHelper.GetDotnetType<Foundation.NSNull> (exception_gchandle);

		[UnmanagedCallersOnly(EntryPoint = "_register_Foundation_NSNull")]
		public unsafe static void register_Foundation_NSNumber(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<Foundation.NSNull> (exception_gchandle);
	}

	internal sealed class CoreGraphics_CGPath__Registrar_Callbacks__
	{
		[UnmanagedCallersOnly(EntryPoint = "_register_CoreGraphics_CGPath")]
		public unsafe static void register_Foundation_NSNumber(IntPtr* exception_gchandle)
			=> RegistrarHelper.Register<CoreGraphics.CGPath> (exception_gchandle);
	}
}
