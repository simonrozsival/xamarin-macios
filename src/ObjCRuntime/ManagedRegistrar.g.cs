#if NET

using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;

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
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Microsoft_MacCatalyst__UIKit_UIApplicationDelegate (out bool is_custom_type);
	}
	
	partial class UIResponder : IManagedRegistrarType {
		static UIResponder() => ManagedRegistrar.Register<UIResponder>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIResponder(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIResponder (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIResponder (out bool is_custom_type);
	}
	
	partial class UIViewController : IManagedRegistrarType {
		static UIViewController() => ManagedRegistrar.Register<UIViewController>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIViewController(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIViewController (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIViewController (out bool is_custom_type);
	}

	partial class UIView : IManagedRegistrarType {
		static UIView() => ManagedRegistrar.Register<UIView>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIView(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIView (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIView (out bool is_custom_type);
	}
	
	partial class UIControl : IManagedRegistrarType {
		static UIControl() => ManagedRegistrar.Register<UIControl>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIControl(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIControl (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIControl (out bool is_custom_type);
	}

	partial class UIControlEventProxy : IManagedRegistrarType {
		static UIControlEventProxy() => ManagedRegistrar.Register<UIControlEventProxy>();
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIControlEventProxy (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIControlEventProxy (out bool is_custom_type);
	}
	
	partial class UIButton : IManagedRegistrarType {
		static UIButton() => ManagedRegistrar.Register<UIButton>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIButton(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIButton (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIButton (out bool is_custom_type);
	}

	partial class UIApplication : IManagedRegistrarType {
		static UIApplication() => ManagedRegistrar.Register<UIApplication>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIApplication(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIApplication (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIApplication (out bool is_custom_type);
	}

	partial class UIScreen : IManagedRegistrarType {
		static UIScreen() => ManagedRegistrar.Register<UIScreen>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIScreen(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIScreen (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIScreen (out bool is_custom_type);
	}
	
	partial class UIWindow : IManagedRegistrarType {
		static UIWindow() => ManagedRegistrar.Register<UIWindow>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new UIWindow(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_UIKit_UIWindow (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_UIKit_UIWindow (out bool is_custom_type);
	}
}

namespace Foundation {
	partial class NSDispatcher : IManagedRegistrarType {
		static NSDispatcher() => ManagedRegistrar.Register<NSDispatcher>();
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSDispatcher (out bool is_custom_type);
	}

	partial class NSSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSSynchronizationContextDispatcher() => ManagedRegistrar.Register<NSSynchronizationContextDispatcher>();
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSSynchronizationContextDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSSynchronizationContextDispatcher (out bool is_custom_type);
	}

	partial class NSAsyncDispatcher : IManagedRegistrarType {
		static NSAsyncDispatcher() => ManagedRegistrar.Register<NSAsyncDispatcher>();
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSAsyncDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAsyncDispatcher (out bool is_custom_type);
	}
	
	partial class NSAsyncSynchronizationContextDispatcher : IManagedRegistrarType {
		static NSAsyncSynchronizationContextDispatcher() => ManagedRegistrar.Register<NSAsyncSynchronizationContextDispatcher>();
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSAsyncSynchronizationContextDispatcher (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAsyncSynchronizationContextDispatcher (out bool is_custom_type);
	}
	
	partial class NSRunLoop : IManagedRegistrarType {
		static NSRunLoop() => ManagedRegistrar.Register<NSRunLoop>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSRunLoop(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSRunLoop (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSRunLoop (out bool is_custom_type);
	}
	
	partial class NSAutoreleasePool : IManagedRegistrarType {
		static NSAutoreleasePool() => ManagedRegistrar.Register<NSAutoreleasePool>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSAutoreleasePool(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSAutoreleasePool (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSAutoreleasePool (out bool is_custom_type);
	}
	
	partial class NSException : IManagedRegistrarType {
		static NSException() => ManagedRegistrar.Register<NSException>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSException(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSException (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSException (out bool is_custom_type);
	}

	partial class NSDictionary : IManagedRegistrarType {
		static NSDictionary() => ManagedRegistrar.Register<NSDictionary>();
#if __IOS__ || __MACCATALYST__
		public static NSObject CreateNSObject(NativeHandle handle) => new NSDictionary(handle);
#endif
		public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSDictionary (out is_custom_type);
		[DllImport("__Internal")]
		private static extern IntPtr get_objc_class_Foundation_NSDictionary (out bool is_custom_type);
	}

	partial class NSObject {
		partial class NSObject_Disposer : IManagedRegistrarType {
			static NSObject_Disposer() => ManagedRegistrar.Register<NSObject_Disposer>();
			public static IntPtr GetObjCClass (out bool is_custom_type) => get_objc_class_Foundation_NSObject_Disposer (out is_custom_type);
			[DllImport("__Internal")]
			private static extern IntPtr get_objc_class_Foundation_NSObject_Disposer (out bool is_custom_type);
		}
	}

	// public static class GetDotnetType__Callbacks
	// {
	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__IntentsUI_INUIAddVoiceShortcutButtonDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__IntentsUI_INUIAddVoiceShortcutButtonDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIAddVoiceShortcutButtonDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__IntentsUI_INUIAddVoiceShortcutViewControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__IntentsUI_INUIAddVoiceShortcutViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIAddVoiceShortcutViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__IntentsUI_INUIEditVoiceShortcutViewControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__IntentsUI_INUIEditVoiceShortcutViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIEditVoiceShortcutViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__NotificationCenter_NCWidgetListViewDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__NotificationCenter_NCWidgetListViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NotificationCenter.NCWidgetListViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__NotificationCenter_NCWidgetProviding")]
	// 	public static IntPtr callback_Microsoft_macOS__NotificationCenter_NCWidgetProviding_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NotificationCenter.NCWidgetProviding));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__NotificationCenter_NCWidgetSearchViewDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__NotificationCenter_NCWidgetSearchViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NotificationCenter.NCWidgetSearchViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__PhotosUI_PHLivePhotoViewDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__PhotosUI_PHLivePhotoViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PhotosUI.PHLivePhotoViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__PhotosUI_PHPickerViewControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__PhotosUI_PHPickerViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PhotosUI.PHPickerViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__PhotosUI_PHProjectTypeDescriptionDataSource")]
	// 	public static IntPtr callback_Microsoft_macOS__PhotosUI_PHProjectTypeDescriptionDataSource_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PhotosUI.PHProjectTypeDescriptionDataSource));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__PushKit_PKPushRegistryDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__PushKit_PKPushRegistryDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PushKit_PKPushRegistryDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__ReplayKit_RPBroadcastActivityControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__ReplayKit_RPBroadcastActivityControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ReplayKit.RPBroadcastActivityControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__ReplayKit_RPBroadcastControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__ReplayKit_RPBroadcastControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ReplayKit.RPBroadcastControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__ReplayKit_RPPreviewViewControllerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__ReplayKit_RPPreviewViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ReplayKit.RPPreviewViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__ReplayKit_RPScreenRecorderDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__ReplayKit_RPScreenRecorderDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ReplayKit.RPScreenRecorderDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__SharedWithYou_SWCollaborationViewDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__SharedWithYou_SWCollaborationViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SharedWithYou.SWCollaborationViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__SharedWithYou_SWHighlightCenterDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__SharedWithYou_SWHighlightCenterDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SharedWithYou.SWHighlightCenterDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__ShazamKit_SHSessionDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__ShazamKit_SHSessionDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ShazamKit.SHSessionDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__Speech_SFSpeechRecognitionTaskDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__Speech_SFSpeechRecognitionTaskDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (Speech.SFSpeechRecognitionTaskDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Microsoft_macOS__Speech_SFSpeechRecognizerDelegate")]
	// 	public static IntPtr callback_Microsoft_macOS__Speech_SFSpeechRecognizerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (Speech.SFSpeechRecognizerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "NCWidgetListViewDelegate")]
	// 	public static IntPtr callback_NCWidgetListViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NCWidgetListViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "NCWidgetProviding")]
	// 	public static IntPtr callback_NCWidgetProviding_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NCWidgetProviding));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "NCWidgetSearchViewDelegate")]
	// 	public static IntPtr callback_NCWidgetSearchViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (NCWidgetSearchViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "PHLivePhotoViewDelegate")]
	// 	public static IntPtr callback_PHLivePhotoViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PHLivePhotoViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "PHProjectTypeDescriptionDataSource")]
	// 	public static IntPtr callback_PHProjectTypeDescriptionDataSource_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PHProjectTypeDescriptionDataSource));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "PKPushRegistryDelegate")]
	// 	public static IntPtr callback_PKPushRegistryDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PKPushRegistryDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "RPBroadcastControllerDelegate")]
	// 	public static IntPtr callback_RPBroadcastControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (RPBroadcastControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "RPPreviewViewControllerDelegate")]
	// 	public static IntPtr callback_RPPreviewViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (RPPreviewViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "RPScreenRecorderDelegate")]
	// 	public static IntPtr callback_RPScreenRecorderDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (RPScreenRecorderDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "SFSpeechRecognitionTaskDelegate")]
	// 	public static IntPtr callback_SFSpeechRecognitionTaskDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SFSpeechRecognitionTaskDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "SFSpeechRecognizerDelegate")]
	// 	public static IntPtr callback_SFSpeechRecognizerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SFSpeechRecognizerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__IntentsUI_INUIAddVoiceShortcutButtonDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__IntentsUI_INUIAddVoiceShortcutButtonDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIAddVoiceShortcutButtonDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__IntentsUI_INUIAddVoiceShortcutViewControllerDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__IntentsUI_INUIAddVoiceShortcutViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIAddVoiceShortcutViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__IntentsUI_INUIEditVoiceShortcutViewControllerDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__IntentsUI_INUIEditVoiceShortcutViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (IntentsUI.INUIEditVoiceShortcutViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__PhotosUI_PHPickerViewControllerDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__PhotosUI_PHPickerViewControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (PhotosUI.PHPickerViewControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__ReplayKit_RPBroadcastActivityControllerDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__ReplayKit_RPBroadcastActivityControllerDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ReplayKit.RPBroadcastActivityControllerDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__SharedWithYou_SWCollaborationViewDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__SharedWithYou_SWCollaborationViewDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SharedWithYou.SWCollaborationViewDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__SharedWithYou_SWHighlightCenterDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__SharedWithYou_SWHighlightCenterDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (SharedWithYou.SWHighlightCenterDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}

	// 	[UnmanagedCallersOnly(EntryPoint = "Xamarin_Mac__ShazamKit_SHSessionDelegate")]
	// 	public static IntPtr callback_Xamarin_Mac__ShazamKit_SHSessionDelegate_GetDotnetType (IntPtr ptr, IntPtr sel, IntPtr* exception_handle)
	// 	{
	// 		try {
	// 			return Runtime.AllocGCHandle (typeof (ShazamKit.SHSessionDelegate));
	// 		} catch (Exception ex) {
	// 			*exception_handle = Runtime.AllocGCHandle (ex);
	// 		}
	// 	}
	// }
}

// TODO: THE REGISTRAR CALLBACKS COULDN'T BE GENERATED HERE BECAUSE OF LINKING???
// -- what exactly was the problem?

#endif // NET
