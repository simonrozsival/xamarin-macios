using System;
using System.Runtime.InteropServices;

using Foundation;
using UIKit;
using ObjCRuntime;

namespace MySingleView {
	public partial class AppDelegate : UIApplicationDelegate {
		UIWindow window;

		// public AppDelegate(NativeHandle handle) : base(handle) {}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			var dvc = new UIViewController ();
			var button = new UIButton (window.Bounds);
			button.SetTitle ("\"Source-generated\" Managed Static Registrar", UIControlState.Normal);
			var clicked = 0;
			button.TouchUpInside += (sender, e) => {
				var n = ++clicked;
				var pluralSuffix = n > 1 ? "s" : "";
				button.SetTitle ($"Clicked {n} time{pluralSuffix}.", UIControlState.Normal);
			};
			dvc.Add (button);

			window.RootViewController = dvc;
			window.MakeKeyAndVisible ();

			// var openClass = Class.GetHandle ("MySingleView_CustomGenericNSObject_2");
			// Console.WriteLine($"openClass: {openClass}");
			// var handle = IntPtr_objc_msgSend (openClass, Selector.GetHandle ("alloc"));
			// handle = IntPtr_objc_msgSend (handle, Selector.GetHandle ("init"));
			// // var inst = Runtime.ConstructNSObject<CustomGenericNSObject<int, string>> (handle);
			// // var inst = Runtime.GetNSObject<CustomGenericNSObject<int, string>> (handle); -- OK
			// // var inst = Runtime.GetNSObject (handle, typeof (CustomGenericNSObject<int, string>).GetGenericTypeDefinition (), Runtime.MissingCtorResolution.ThrowConstructor1NotFound, evenInFinalizerQueue: true, createNewInstanceIfWrongType: true, out bool created); -- throws
			// var inst = Runtime.GetNSObject (handle); // -- throws in regular mode, but is "not great" in the new mode
			// Console.WriteLine ($"inst: {inst} ({inst.GetType()})");

			return true;
		}

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend (IntPtr receiver, IntPtr selector);
	}

	// public partial class CustomGenericNSObject<T1, T2> : NSObject {
	// 	public CustomGenericNSObject (NativeHandle handle) : base(handle) { }
	// }
}
