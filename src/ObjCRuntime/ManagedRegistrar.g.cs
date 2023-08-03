#if NET

using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;

#if MONOMAC || __MACCATALYST__
namespace AppKit {
	partial class ActionDispatcher : IManagedRegistrarType {
		static ActionDispatcher() => RegistrarHelper.Register<ActionDispatcher>();
	}
}
#endif

namespace UIKit {
	partial class UIApplicationDelegate : NSObject, IManagedRegistrarType {
		static UIApplicationDelegate() => RegistrarHelper.Register<UIApplicationDelegate>();
		public static bool IsCustomType => true;
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplicationDelegate(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__UIKit_UIApplicationDelegate");
	}
	
	partial class UIResponder : IManagedRegistrarType {
		static UIResponder() => RegistrarHelper.Register<UIResponder>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIResponder(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIResponder");
	}
	
	partial class UIViewController : IManagedRegistrarType {
		static UIViewController() => RegistrarHelper.Register<UIViewController>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIViewController(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIViewController");
	}

	partial class UIView : IManagedRegistrarType {
		static UIView() => RegistrarHelper.Register<UIView>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIView(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIView");
	}
	
	partial class UIControl : IManagedRegistrarType {
		static UIControl() => RegistrarHelper.Register<UIControl>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIControl(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIControl");
	}

	partial class UIControlEventProxy : IManagedRegistrarType {
		static UIControlEventProxy() => RegistrarHelper.Register<UIControlEventProxy>();
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIKit_UIControlEventProxy");
	}
	
	partial class UIButton : IManagedRegistrarType {
		static UIButton() => RegistrarHelper.Register<UIButton>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIButton(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIButton");
	}

	partial class UIApplication : IManagedRegistrarType {
		static UIApplication() => RegistrarHelper.Register<UIApplication>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplication(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIApplication");
	}

	partial class UIScreen : IManagedRegistrarType {
		static UIScreen() => RegistrarHelper.Register<UIScreen>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIScreen(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIScreen");
	}
	
	partial class UIWindow : IManagedRegistrarType {
		static UIWindow() => RegistrarHelper.Register<UIWindow>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIWindow(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("UIWindow");
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
		static NSDispatcher() => RegistrarHelper.Register<NSDispatcher>();
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSDispatcher");
	}

	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSSynchronizationContextDispatcher() => RegistrarHelper.Register<NSSynchronizationContextDispatcher>();
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__MonoMac_NSSynchronizationContextDispatcher");
	}

	partial class NSAsyncDispatcher : IManagedRegistrarType {
		static NSAsyncDispatcher() => RegistrarHelper.Register<NSAsyncDispatcher>();
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSAsyncDispatcher");
	}
	
	partial class NSAsyncSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSAsyncSynchronizationContextDispatcher() => RegistrarHelper.Register<NSAsyncSynchronizationContextDispatcher>();
		public static NativeHandle GetNativeClass () => Class.GetHandle ("__MonoMac_NSAsyncSynchronizationContextDispatcher");
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
		static NSRunLoop() => RegistrarHelper.Register<NSRunLoop>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSRunLoop(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSRunLoop");
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
		static NSAutoreleasePool() => RegistrarHelper.Register<NSAutoreleasePool>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSAutoreleasePool(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSAutoreleasePool");
	}
	
	partial class NSException : IManagedRegistrarType {
		static NSException() => RegistrarHelper.Register<NSException>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSException(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSException");
	}

	partial class NSDictionary : IManagedRegistrarType {
		static NSDictionary() => RegistrarHelper.Register<NSDictionary>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSDictionary(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSDictionary");
	}

	partial class NSArray<TKey> : IManagedRegistrarType {
		static NSArray() => RegistrarHelper.Register<NSArray<TKey>>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSArray<TKey>(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSArray");
	}

	static class NSArray_1 {
	}

	partial class NSString : IManagedRegistrarType {
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSString(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSString");
	}

	partial class NSNumber : IManagedRegistrarType {
		static NSNumber() => RegistrarHelper.Register<NSNumber>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSNumber(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSNumber");
	}

	partial class NSNull : IManagedRegistrarType {
		static NSNull() => RegistrarHelper.Register<NSNull>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSNull(handle);
#endif
		public static NativeHandle GetNativeClass () => Class.GetHandle ("NSNull");
	}

	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
			static NSObject_Disposer() => RegistrarHelper.Register<NSObject_Disposer>();
			public static NativeHandle GetNativeClass () => Class.GetHandle ("__NSObject_Disposer");
		}
	}
}

namespace CoreGraphics {
	partial class CGPath : IManagedRegistrarType {
		static CGPath() => RegistrarHelper.Register<CGPath>();
		public static INativeObject CreateINativeObject (NativeHandle handle, bool owns)
			=> new CGPath (handle, owns);
		public static NativeHandle GetNativeClass () => Class.GetHandle ("CGPath");
	}
}

#endif // NET
