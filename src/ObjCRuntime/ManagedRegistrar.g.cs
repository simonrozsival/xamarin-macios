#if NET

using Foundation;
using ObjCRuntime;

#if MONOMAC || __MACCATALYST__
namespace AppKit {
	partial class ActionDispatcher : IManagedRegistrarType {
		static ActionDispatcher() => ManagedRegistrar.Register<ActionDispatcher>();
	}
}
#endif

namespace UIKit {
	// 0 -> NSObject - skipped
	partial class UIApplicationDelegate : NSObject, IManagedRegistrarType {
		static UIApplicationDelegate() => ManagedRegistrar.Register<UIApplicationDelegate>();
		public UIApplicationDelegate(ManagedRegistrarHandleWrapper handle) : base((NativeHandle)handle) { }
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplicationDelegate(handle);
#endif
	}
	
	partial class UIResponder : IManagedRegistrarType {
		static UIResponder() => ManagedRegistrar.Register<UIResponder>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIResponder(handle);
#endif
	}
	
	partial class UIViewController : IManagedRegistrarType {
		static UIViewController() => ManagedRegistrar.Register<UIViewController>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIViewController(handle);
#endif
	}

	partial class UIView : IManagedRegistrarType {
		static UIView() => ManagedRegistrar.Register<UIView>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIView(handle);
#endif
	}
	
	partial class UIControl : IManagedRegistrarType {
		static UIControl() => ManagedRegistrar.Register<UIControl>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIControl(handle);
#endif
	}

	partial class UIControlEventProxy : IManagedRegistrarType {
		static UIControlEventProxy() => ManagedRegistrar.Register<UIControlEventProxy>();
	}
	
	partial class UIButton : IManagedRegistrarType {
		static UIButton() => ManagedRegistrar.Register<UIButton>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIButton(handle);
#endif
	}

	partial class UIApplication : IManagedRegistrarType {
		static UIApplication() => ManagedRegistrar.Register<UIApplication>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplication(handle);
#endif
	}

	partial class UIScreen : IManagedRegistrarType {
		static UIScreen() => ManagedRegistrar.Register<UIScreen>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIScreen(handle);
#endif
	}
	
	partial class UIWindow : IManagedRegistrarType {
		static UIWindow() => ManagedRegistrar.Register<UIWindow>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIWindow(handle);
#endif
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
		static NSDispatcher() => ManagedRegistrar.Register<NSDispatcher>();
	}

	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSSynchronizationContextDispatcher() => ManagedRegistrar.Register<NSSynchronizationContextDispatcher>();
	}

	partial class NSAsyncDispatcher : IManagedRegistrarType {
		static NSAsyncDispatcher() => ManagedRegistrar.Register<NSAsyncDispatcher>();
	}
	
	partial class NSAsyncSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSAsyncSynchronizationContextDispatcher() => ManagedRegistrar.Register<NSAsyncSynchronizationContextDispatcher>();
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
		static NSRunLoop() => ManagedRegistrar.Register<NSRunLoop>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSRunLoop(handle);
#endif
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
		static NSAutoreleasePool() => ManagedRegistrar.Register<NSAutoreleasePool>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSAutoreleasePool(handle);
#endif
	}
	
	partial class NSException : IManagedRegistrarType {
		static NSException() => ManagedRegistrar.Register<NSException>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSException(handle);
#endif
	}

	partial class NSDictionary : IManagedRegistrarType {
		static NSDictionary() => ManagedRegistrar.Register<NSDictionary>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSDictionary(handle);
#endif
	}
	
	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
			static NSObject_Disposer() => ManagedRegistrar.Register<NSObject_Disposer>();
		}
	}
}

// TODO: THE REGISTRAR CALLBACKS COULDN'T BE GENERATED HERE BECAUSE OF LINKING???
// -- what exactly was the problem?

#endif // NET
