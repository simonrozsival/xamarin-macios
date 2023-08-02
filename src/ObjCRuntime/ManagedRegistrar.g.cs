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
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplicationDelegate(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate (out bool is_custom_type);
	}
	
	partial class UIResponder : IManagedRegistrarType {
		static UIResponder() => RegistrarHelper.Register<UIResponder>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIResponder(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIResponder (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIResponder (out bool is_custom_type);
	}
	
	partial class UIViewController : IManagedRegistrarType {
		static UIViewController() => RegistrarHelper.Register<UIViewController>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIViewController(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIViewController (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIViewController (out bool is_custom_type);
	}

	partial class UIView : IManagedRegistrarType {
		static UIView() => RegistrarHelper.Register<UIView>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIView(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIView (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIView (out bool is_custom_type);
	}
	
	partial class UIControl : IManagedRegistrarType {
		static UIControl() => RegistrarHelper.Register<UIControl>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIControl(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIControl (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIControl (out bool is_custom_type);
	}

	partial class UIControlEventProxy : IManagedRegistrarType {
		static UIControlEventProxy() => RegistrarHelper.Register<UIControlEventProxy>();
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIControlEventProxy (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIControlEventProxy (out bool is_custom_type);
	}
	
	partial class UIButton : IManagedRegistrarType {
		static UIButton() => RegistrarHelper.Register<UIButton>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIButton(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIButton (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIButton (out bool is_custom_type);
	}

	partial class UIApplication : IManagedRegistrarType {
		static UIApplication() => RegistrarHelper.Register<UIApplication>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplication(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIApplication (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIApplication (out bool is_custom_type);
	}

	partial class UIScreen : IManagedRegistrarType {
		static UIScreen() => RegistrarHelper.Register<UIScreen>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIScreen(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIScreen (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIScreen (out bool is_custom_type);
	}
	
	partial class UIWindow : IManagedRegistrarType {
		static UIWindow() => RegistrarHelper.Register<UIWindow>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIWindow(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_UIKit_UIWindow (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIWindow (out bool is_custom_type);
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
		static NSDispatcher() => RegistrarHelper.Register<NSDispatcher>();
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSDispatcher (out bool is_custom_type);
	}

	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSSynchronizationContextDispatcher() => RegistrarHelper.Register<NSSynchronizationContextDispatcher>();
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSSynchronizationContextDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSSynchronizationContextDispatcher (out bool is_custom_type);
	}

	partial class NSAsyncDispatcher : IManagedRegistrarType {
		static NSAsyncDispatcher() => RegistrarHelper.Register<NSAsyncDispatcher>();
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSAsyncDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAsyncDispatcher (out bool is_custom_type);
	}
	
	partial class NSAsyncSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSAsyncSynchronizationContextDispatcher() => RegistrarHelper.Register<NSAsyncSynchronizationContextDispatcher>();
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSAsyncSynchronizationContextDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAsyncSynchronizationContextDispatcher (out bool is_custom_type);
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
		static NSRunLoop() => RegistrarHelper.Register<NSRunLoop>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSRunLoop(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSRunLoop (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSRunLoop (out bool is_custom_type);
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
		static NSAutoreleasePool() => RegistrarHelper.Register<NSAutoreleasePool>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSAutoreleasePool(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSAutoreleasePool (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAutoreleasePool (out bool is_custom_type);
	}
	
	partial class NSException : IManagedRegistrarType {
		static NSException() => RegistrarHelper.Register<NSException>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSException(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSException (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSException (out bool is_custom_type);
	}

	partial class NSDictionary : IManagedRegistrarType {
		static NSDictionary() => RegistrarHelper.Register<NSDictionary>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSDictionary(handle);
#endif
		public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSDictionary (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSDictionary (out bool is_custom_type);
	}

	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
			static NSObject_Disposer() => RegistrarHelper.Register<NSObject_Disposer>();
			public static IntPtr GetNativeClass (out bool is_custom_type) => get_objc_class_Foundation_NSObject_Disposer (out is_custom_type);
			[DllImport("__Internal")]
			private static extern IntPtr get_objc_class_Foundation_NSObject_Disposer (out bool is_custom_type);
		}
	}
}

#endif // NET
