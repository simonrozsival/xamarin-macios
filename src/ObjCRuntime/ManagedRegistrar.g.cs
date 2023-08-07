#if NET

using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;

#if MONOMAC || __MACCATALYST__
namespace AppKit {
	partial class ActionDispatcher : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static ActionDispatcher() => RegistrarHelper.Register<ActionDispatcher> ();
#endif
	}
}
#endif

namespace UIKit {
	partial class UIApplicationDelegate : NSObject, IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIApplicationDelegate() => RegistrarHelper.Register<UIApplicationDelegate> ();
#endif
		public static bool IsCustomType => true;
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplicationDelegate(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__UIKit_UIApplicationDelegate");
	}
	
	partial class UIResponder : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIResponder() => RegistrarHelper.Register<UIResponder> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIResponder(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIResponder");
	}
	
	partial class UIViewController : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIViewController() => RegistrarHelper.Register<UIViewController> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIViewController(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIViewController");
	}

	partial class UIView : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIView() => RegistrarHelper.Register<UIView> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIView(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIView");
	}
	
	partial class UIControl : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIControl() => RegistrarHelper.Register<UIControl> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIControl(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIControl");
	}

	partial class UIControlEventProxy : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIControlEventProxy() => RegistrarHelper.Register<UIControlEventProxy> ();
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIKit_UIControlEventProxy");
	}
	
	partial class UIButton : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIButton() => RegistrarHelper.Register<UIButton> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIButton(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIButton");
	}

	partial class UIApplication : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIApplication() => RegistrarHelper.Register<UIApplication> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplication(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIApplication");
	}

	partial class UIScreen : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIScreen() => RegistrarHelper.Register<UIScreen> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIScreen(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIScreen");
	}
	
	partial class UIWindow : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static UIWindow() => RegistrarHelper.Register<UIWindow> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIWindow(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIWindow");
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSDispatcher() => RegistrarHelper.Register<NSDispatcher> ();
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSDispatcher");
	}

	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSSynchronizationContextDispatcher() => RegistrarHelper.Register<NSSynchronizationContextDispatcher> ();
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__MonoMac_NSSynchronizationContextDispatcher");
	}

	partial class NSAsyncDispatcher : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSAsyncDispatcher() => RegistrarHelper.Register<NSAsyncDispatcher> ();
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSAsyncDispatcher");
	}
	
	partial class NSAsyncSynchronizationContextDispatcher : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSAsyncSynchronizationContextDispatcher() => RegistrarHelper.Register<NSAsyncSynchronizationContextDispatcher> ();
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__MonoMac_NSAsyncSynchronizationContextDispatcher");
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSRunLoop() => RegistrarHelper.Register<NSRunLoop> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSRunLoop(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSRunLoop");
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSAutoreleasePool() => RegistrarHelper.Register<NSAutoreleasePool> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSAutoreleasePool(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSAutoreleasePool");
	}
	
	partial class NSException : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSException() => RegistrarHelper.Register<NSException> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSException(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSException");
	}

	partial class NSDictionary : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSDictionary() => RegistrarHelper.Register<NSDictionary> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSDictionary(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSDictionary");
	}

	partial class NSArray<TKey> : IManagedRegistrarType {
		// !!! We need to ALWAYS register all generic types in static constructors because we don't know
		// the generic arguments in the UCO called from the "module initializer".
		// Would it make sense to have separate strategies for non-generic and generic types?
		// - static ctors:
		//    - works also for generic types
		//    - shouldn't affect startup times that much
		// - module initializer:
		//    - solves (should solve) problems with static field initializers
		static NSArray () => RegistrarHelper.Register<NSArray<TKey>> ();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSArray<TKey>(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSArray");
		// TODO: could the generated Objective-C classes also hold generic arguments?
		// it might be possible to create instances of NSArray<TKey> from Objective-C 🤔?
	}

	static class NSArray_1 {
	}

	partial class NSString : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSString() => RegistrarHelper.Register<NSString> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSString(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSString");
	}

	partial class NSNumber : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSNumber() => RegistrarHelper.Register<NSNumber> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSNumber(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSNumber");
	}

	partial class NSNull : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static NSNull() => RegistrarHelper.Register<NSNull> ();
#endif
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSNull(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSNull");
	}

	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
	#if USE_STATIC_CTORS
		static NSObject_Disposer() => RegistrarHelper.Register<NSObject_Disposer> ();
#endif
			public static NativeHandle GetNativeClass () => Class.GetHandle ("__NSObject_Disposer");
		}
	}
}

namespace CoreGraphics {
	partial class CGPath : IManagedRegistrarType {
#if USE_STATIC_CTORS
		static CGPath() => RegistrarHelper.Register<CGPath> ();
#endif
		public static INativeObject CreateINativeObject (NativeHandle handle, bool owns)
			=> new CGPath (handle, owns);
		public static NativeHandle GetNativeClass () => Class.GetHandle ("CGPath");
	}
}

#endif // NET
