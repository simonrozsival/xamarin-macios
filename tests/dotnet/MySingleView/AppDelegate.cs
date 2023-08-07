using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Foundation;
using UIKit;
using ObjCRuntime;

using CoreGraphics;
using CoreLocation;
using NativeException = ObjCRuntime.ObjCException;

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

			ExportedGenericsTest ();
			TestProperties ();
			TestStaticProperties ();
			TestINativeObject ();

			return true;
		}

		static class Messaging {
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend_IntPtr (IntPtr receiver, IntPtr selector, IntPtr p0);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend_bool (IntPtr receiver, IntPtr selector, bool p0);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern void void_objc_msgSend_IntPtr (IntPtr receiver, IntPtr selector, IntPtr p0);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend (IntPtr receiver, IntPtr selector);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern bool bool_objc_msgSend (IntPtr receiver, IntPtr selector);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern bool bool_objc_msgSend_IntPtr (IntPtr receiver, IntPtr selector, IntPtr p0);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern void void_objc_msgSend_out_IntPtr_bool (IntPtr receiver, IntPtr selector, out IntPtr p0, bool p1);
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern bool bool_objc_msgSend_ref_intptr (IntPtr receiver, IntPtr selector, ref IntPtr p0);
		}

		public void ExportedGenericsTest ()
		{
			using (var obj = new RegistrarTestClass ()) {
				var arr = Messaging.IntPtr_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("fetchNSArrayOfNSString:"), IntPtr.Zero);
				Console.WriteLine($"---");
				var rv = Runtime.GetNSObject<NSArray<NSString>> (arr);
				if (rv is null) Console.WriteLine ("[FAIL]: method");
				// else Console.WriteLine ($"[OK]: method: {rv}");

				using (var number_array = NSArray<NSNumber>.FromNSObjects ((NSNumber) 314)) {
					rv = Runtime.GetNSObject<NSArray<NSString>> (Messaging.IntPtr_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("fetchNSArrayOfNSString:"), number_array.Handle));
					if (rv is null) Console.WriteLine("[FAIL]: method param");
					// else Console.WriteLine ($"[OK]: method param: {rv}");
				}

				rv = Runtime.GetNSObject<NSArray<NSString>> (Messaging.IntPtr_objc_msgSend (obj.Handle, Selector.GetHandle ("nSArrayOfNSString")));
				if (rv is null) Console.WriteLine("[FAIL]: property");
				// else Console.WriteLine ($"[OK]: property: {rv}");

				Messaging.void_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("setNSArrayOfNSString:"), IntPtr.Zero);
				Messaging.void_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("setNSArrayOfNSString:"), rv.Handle);

				var rv2 = Runtime.GetNSObject<NSArray<NSArray<NSString>>> (Messaging.IntPtr_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("fetchComplexGenericType:"), IntPtr.Zero));
				if (rv2 is null) Console.WriteLine("[FAIL]: complex");
				// else Console.WriteLine ($"[OK]: complex: {rv2}");

				using (var complex = new NSArray<NSDictionary<NSString, NSArray<NSNumber>>> ()) {
					Runtime.GetNSObject<NSArray<NSArray<NSString>>> (Messaging.IntPtr_objc_msgSend_IntPtr (obj.Handle, Selector.GetHandle ("fetchComplexGenericType:"), complex.Handle));
					if (rv2 is null) Console.WriteLine("[FAIL]: complex param");
					// else Console.WriteLine ($"[OK]: complex param: {rv2}");
				}
			}
		}

		public void TestProperties ()
		{
			RegistrarTestClass obj = new RegistrarTestClass ();
			IntPtr receiver = obj.Handle;
			int dummy = 314;

			// readonly, attribute on property
			CallProperty (receiver, "Property1", ref obj.called_Property1Getter, "#Instance-1-r");
			CallProperty (receiver, "setProperty1:", ref dummy, "#Instance-1-w", true);
			// rw, attribute on property
			CallProperty (receiver, "Property2", ref obj.called_Property2Getter, "#Instance-2-r");
			CallProperty (receiver, "setProperty2:", ref obj.called_Property2Setter, "#Instance-2-w");
			// writeonly, attribute on property
			//CallProperty (receiver, "Property3", ref dummy, "#Instance-3-r", true);
			CallProperty (receiver, "setProperty3:", ref obj.called_Property3Setter, "#Instance-3-w");

			// readonly, atteribute on getter
			CallProperty (receiver, "Property4", ref obj.called_Property4Getter, "#Instance-4-r");
			CallProperty (receiver, "setProperty4:", ref dummy, "#Instance-4-w", true);
			// rw, attribyte on getter/setter
			CallProperty (receiver, "Property5", ref obj.called_Property5Getter, "#Instance-5-r");
			CallProperty (receiver, "setProperty5:", ref dummy, "#Instance-5-w1", true);
			CallProperty (receiver, "WProperty5:", ref obj.called_Property5Setter, "#Instance-5-w2");
			// writeonly, attribute on setter
			CallProperty (receiver, "setProperty6:", ref dummy, "#Instance-6-r", true);
			CallProperty (receiver, "Property6:", ref obj.called_Property6Setter, "#Instance-6-w");
		}

		public void TestStaticProperties ()
		{
			IntPtr receiver = Class.GetHandle ("RegistrarTestClass");
			int dummy = 314;

			RegistrarTestClass.called_StaticProperty1Getter = 0;
			RegistrarTestClass.called_StaticProperty2Getter = 0;
			RegistrarTestClass.called_StaticProperty4Getter = 0;
			RegistrarTestClass.called_StaticProperty5Getter = 0;
			RegistrarTestClass.called_StaticProperty2Setter = 0;
			RegistrarTestClass.called_StaticProperty3Setter = 0;
			RegistrarTestClass.called_StaticProperty5Setter = 0;
			RegistrarTestClass.called_StaticProperty6Setter = 0;

			// readonly, attribute on property
			CallProperty (receiver, "StaticProperty1", ref RegistrarTestClass.called_StaticProperty1Getter, "#Static-1-r");
			CallProperty (receiver, "setStaticProperty1:", ref dummy, "#Static-1-w", true);
			// rw, attribute on property
			CallProperty (receiver, "StaticProperty2", ref RegistrarTestClass.called_StaticProperty2Getter, "#Static-2-r");
			CallProperty (receiver, "setStaticProperty2:", ref RegistrarTestClass.called_StaticProperty2Setter, "#Static-2-w");
			// writeonly, attribute on property
			CallProperty (receiver, "StaticProperty3", ref dummy, "#Static-3-r", true);
			CallProperty (receiver, "setStaticProperty3:", ref RegistrarTestClass.called_StaticProperty3Setter, "#Static-3-w");

			// readonly, atteribute on getter
			CallProperty (receiver, "StaticProperty4", ref RegistrarTestClass.called_StaticProperty4Getter, "#Static-4-r");
			CallProperty (receiver, "setStaticProperty4:", ref dummy, "#Static-4-w", true);
			// rw, attribyte on getter/setter
			CallProperty (receiver, "StaticProperty5", ref RegistrarTestClass.called_StaticProperty5Getter, "#Static-5-r");
			CallProperty (receiver, "setStaticProperty5:", ref dummy, "#Static-5-w1", true);
			CallProperty (receiver, "WStaticProperty5:", ref RegistrarTestClass.called_StaticProperty5Setter, "#Static-5-w2");
			// writeonly, attribute on setter
			CallProperty (receiver, "setStaticProperty6:", ref dummy, "#Static-6-r", true);
			CallProperty (receiver, "StaticProperty6:", ref RegistrarTestClass.called_StaticProperty6Setter, "#Static-6-w");
		}

		void CallProperty (IntPtr receiver, string selector, ref int called_var, string id, bool expectFailure = false)
		{
			try {
				Messaging.bool_objc_msgSend (receiver, new Selector (selector).Handle);
				Assert.True (!expectFailure, id + "-expected-failure-but-succeeded");
				Assert.True (called_var == 1, id + "-called-var");
			} catch (NativeException ex) {
				Assert.True (expectFailure, id + "-expected-success-but-failed: " + ex.Message);
			}
		}
		
		static class Assert {
			// public static void That<T> (bool x, string msg) {
			// 	if (x) {
			// 		Console.WriteLine ($"[FAIL] {msg}");
			// 	} else {
			// 		Console.WriteLine ($"[OK] {msg}");
			// 	}
			// }

			public static void False (bool x, string msg) {
				if (x) {
					Console.WriteLine ($"[FAIL] {msg}");
				} else {
					// Console.WriteLine ($"[OK] {msg}");
				}
			}

			public static void True (bool x, string msg) {
				if (!x) {
					Console.WriteLine ($"[FAIL] {msg}");
				} else {
					// Console.WriteLine ($"[OK] {msg}");
				}
			}
		}

		public void TestINativeObject ()
		{
			var receiver = Class.GetHandle ("RegistrarTestClass");
			IntPtr ptr;
			CGPath path;

			Assert.False (Messaging.bool_objc_msgSend_IntPtr (receiver, new Selector ("INativeObject1:").Handle, NativeHandle.Zero), "#a1");
			Assert.True (Messaging.bool_objc_msgSend_IntPtr (receiver, new Selector ("INativeObject1:").Handle, new CGPath ().Handle), "#a2");

			Assert.True ((NativeHandle) Messaging.IntPtr_objc_msgSend_bool (receiver, new Selector ("INativeObject2:").Handle, false) == NativeHandle.Zero, "#b1");
			ptr = Messaging.IntPtr_objc_msgSend_bool (receiver, new Selector ("INativeObject2:").Handle, true);
			Assert.True ((NativeHandle) ptr != NativeHandle.Zero, "#b2");
			CGPathRelease (ptr);

			Messaging.void_objc_msgSend_out_IntPtr_bool (receiver, new Selector ("INativeObject3:create:").Handle, out ptr, true);
			Assert.True (ptr != NativeHandle.Zero, "#c1");
			Messaging.void_objc_msgSend_out_IntPtr_bool (receiver, new Selector ("INativeObject3:create:").Handle, out ptr, false);
			Assert.True (ptr != NativeHandle.Zero, "#c2");

			path = null;
			ptr = NativeHandle.Zero;
			Assert.False (Messaging.bool_objc_msgSend_ref_intptr (receiver, new Selector ("INativeObject4:").Handle, ref ptr), "#d1");
			Assert.True (ptr == NativeHandle.Zero, "#d2");
			path = new CGPath ();
			ptr = path.Handle;
			Assert.True (Messaging.bool_objc_msgSend_ref_intptr (receiver, new Selector ("INativeObject4:").Handle, ref ptr), "#d3");
			Assert.True (ptr == path.Handle, "#d4");

			ptr = Messaging.IntPtr_objc_msgSend_bool (receiver, new Selector ("INativeObject5:").Handle, false);
			Assert.True (ptr == NativeHandle.Zero, "#e1");
			ptr = Messaging.IntPtr_objc_msgSend_bool (receiver, new Selector ("INativeObject5:").Handle, true);
			Assert.True (ptr != NativeHandle.Zero, "#e2");
			path = Runtime.GetINativeObject<CGPath> (ptr, false);
			path.AddArc (1, 2, 3, 4, 5, false); // this should crash if we get back a bogus ptr
			CGPathRelease (ptr);
		}

		[DllImport ("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		extern static void CGPathRelease (/* CGPathRef */ IntPtr path);
	}
	
	static class Messaging {
		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public extern static IntPtr IntPtr_objc_msgSend_IntPtr (IntPtr ptr, IntPtr sel, IntPtr p0);
		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public extern static IntPtr IntPtr_objc_msgSend (IntPtr ptr, IntPtr sel);
		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public extern static void void_objc_msgSend_IntPtr (IntPtr ptr, IntPtr sel, IntPtr p0);
	}

	// public partial class CustomGenericNSObject<T1, T2> : NSObject {
	// 	public CustomGenericNSObject (NativeHandle handle) : base(handle) { }
	// }
	
	[Register ("RegistrarTestClass")]
	partial class RegistrarTestClass : NSObject {
		public virtual bool B1 {
			[Export ("b1")]
			get {
				return false;
			}
		}

		// static properties
		public static int called_StaticProperty1Getter;
		[Export ("StaticProperty1")]
		static bool StaticProperty1 {
			get {
				called_StaticProperty1Getter++;
				return true;
			}
		}

		public static int called_StaticProperty2Getter;
		public static int called_StaticProperty2Setter;
		[Export ("StaticProperty2")]
		static bool StaticProperty2 {
			get {
				called_StaticProperty2Getter++;
				return true;
			}
			set {
				called_StaticProperty2Setter++;
			}
		}

		public static int called_StaticProperty3Setter;
		[Export ("StaticProperty3")]
		static bool StaticProperty3 {
			set {
				called_StaticProperty3Setter++;
			}
		}

		public static int called_StaticProperty4Getter;
		static bool StaticProperty4 {
			[Export ("StaticProperty4")]
			get {
				called_StaticProperty4Getter++;
				return true;
			}
		}

		public static int called_StaticProperty5Getter;
		public static int called_StaticProperty5Setter;
		static bool StaticProperty5 {
			[Export ("StaticProperty5")]
			get {
				called_StaticProperty5Getter++;
				return true;
			}
			[Export ("WStaticProperty5:")] // can't use same name as getter, and don't use the default "set_" prefix either (to ensure we're not exporting the default prefix always)
			set {
				called_StaticProperty5Setter++;
			}
		}

		public static int called_StaticProperty6Setter;
		static bool StaticProperty6 {
			[Export ("StaticProperty6:")]
			set {
				called_StaticProperty6Setter++;
			}
		}

		// instance properties
		public int called_Property1Getter;
		[Export ("Property1")]
		bool Property1 {
			get {
				called_Property1Getter++;
				return true;
			}
		}

		public int called_Property2Getter;
		public int called_Property2Setter;
		[Export ("Property2")]
		bool Property2 {
			get {
				called_Property2Getter++;
				return true;
			}
			set {
				called_Property2Setter++;
			}
		}

		public int called_Property3Setter;
		[Export ("Property3")]
		bool Property3 {
			set {
				called_Property3Setter++;
			}
		}

		public int called_Property4Getter;
		bool Property4 {
			[Export ("Property4")]
			get {
				called_Property4Getter++;
				return true;
			}
		}

		public int called_Property5Getter;
		public int called_Property5Setter;
		bool Property5 {
			[Export ("Property5")]
			get {
				called_Property5Getter++;
				return true;
			}
			[Export ("WProperty5:")] // can't use same name as getter, and don't use the default "set_" prefix either (to ensure we're not exporting the default prefix always)
			set {
				called_Property5Setter++;
			}
		}

		public int called_Property6Setter;
		bool Property6 {
			[Export ("Property6:")]
			set {
				called_Property6Setter++;
			}
		}

		[Export ("INativeObject1:")]
		static bool INativeObject1 (CGPath img /*CGPath is a INativeObject */)
		{
			return img is not null;
		}

		[Export ("INativeObject2:")]
		[return: ReleaseAttribute] // can't return an INativeObject without retaining it (we can autorelease NSObjects, but that doesn't work for INativeObjects)
		static CGPath INativeObject2 (bool create)
		{
			return create ? new CGPath () : null;
		}

		[Export ("INativeObject3:create:")]
		static void INativeObject3 (out CGPath path, bool create)
		{
			path = create ? new CGPath () : null;
		}

		[Export ("INativeObject4:")]
		static bool INativeObject4 (ref CGPath path)
		{
			return path is not null;
		}

		[Export ("INativeObject5:")]
		[return: ReleaseAttribute] // can't return an INativeObject without retaining it (we can autorelease NSObjects, but that doesn't work for INativeObjects)
		static CGPath INativeObject5 (bool create)
		{
			return create ? new CGPath () : null;
		}

		[Export ("VirtualMethod")]
		public virtual string VirtualMethod ()
		{
			return "base";
		}

		[Export ("testNSAction:")]
		public void TestNSAction (Action action)
		{
		}

		[Export ("testOutNSString:")]
		public void TestOutNSString (out string value)
		{
			value = "Santa is coming";
		}

		[Export ("testOutParametersWithStructs:in:out:")]
		public void TestOutParameters (SizeF a, NSError @in, out NSError value)
		{
			// bug 16078
			value = @in;
		}

		// [Export ("testAction:")]
		// public void TestAction ([BlockProxy (typeof (NIDActionArity1V1))] Action<UIBackgroundFetchResult> action)
		// {
		// 	// bug ?
		// }

		[return: ReleaseAttribute ()]
		[Export ("testRetainArray")]
		public NSObject [] TestRetainArray ()
		{
			return new NSObject [] { new NSObject () };
		}

		[Export ("testBug23289:")]
		public bool TestBug23289 (CLLocation [] array)
		{
			return true;
		}

		[return: ReleaseAttribute ()]
		[Export ("testReturnINativeObject")]
		public INativeObject TestRetainINativeObject ()
		{
			return new NSObject ();
		}

		[return: ReleaseAttribute ()]
		[Export ("testRetainNSObject")]
		public NSObject TestRetainNSObject ()
		{
			return new NSObject ();
		}

		[return: ReleaseAttribute ()]
		[Export ("testRetainString")]
		public string TestRetainString ()
		{
			return "some string that does not match a constant NSString";
		}

		[return: ReleaseAttribute ()]
		[Export ("testOverriddenRetainNSObject")]
		public virtual NSObject TestOverriddenRetainNSObject ()
		{
			return new NSObject ();
		}

		[Export ("testNativeEnum1:")]
		public virtual void TestNativeEnum1 (NSWritingDirection twd)
		{
			if (Array.IndexOf(Enum.GetValues (typeof (NSWritingDirection)), twd) < 0)
				Console.WriteLine("[FAIL]: TestNativeEnum1");
		}

		public virtual UIPopoverArrowDirection TestNativeEnum2 {
			[Export ("testNativeEnum2")]
			get {
				return UIPopoverArrowDirection.Right;
			}
			[Export ("setTestNativeEnum2:")]
			set {
				if (UIPopoverArrowDirection.Left != value) Console.WriteLine("[FAIL] setTestNativeEnum2:");
			}
		}

		// TODO: I broke de-duplication of callback methods by removing the callback counter
		// [Export ("testNativeEnum3:a:b:")]
		// public virtual void TestNativeEnum1 (NSWritingDirection twd, int a, long b)
		// {
		// 	if (Array.IndexOf(Enum.GetValues (typeof (NSWritingDirection)), twd) < 0)
		// 		Console.WriteLine("[FAIL] TestNativeEnum3");
		// 	if (31415 != a) Console.WriteLine("[FAIL] TestNativeEnum3 a");
		// 	if (3141592 != b) Console.WriteLine("[FAIL] TestNativeEnum3 b");
		// }

		[Export ("testCGPoint:out:")]
		public void TestCGPoint (PointF pnt, ref PointF pnt2)
		{
			pnt2.X = pnt.X;
			pnt2.Y = pnt.Y;
		}

		[Export ("arrayOfINativeObject")]
		public IUIKeyInput [] NativeObjects { get { return null; } }

		[Export ("fetchNSArrayOfNSString:")]
		NSArray<NSString> FetchNSArrayOfNSString (NSArray<NSNumber> p0)
		{
			return NSArray<NSString>.FromNSObjects ((NSString) "abc");
		}

		[Export ("fetchComplexGenericType:")]
		NSArray<NSArray<NSString>> FetchComplexGenericType (NSArray<NSDictionary<NSString, NSArray<NSNumber>>> p0)
		{
			return new NSArray<NSArray<NSString>> ();
		}

		[Export ("nSArrayOfNSString")]
		NSArray<NSString> NSArrayOfNSString {
			get {
				return new NSArray<NSString> ();
			}
			set {
			}
		}
	}

	// [UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
	// internal delegate void DActionArity1V1 (IntPtr block, IntPtr obj);

	// internal class NIDActionArity1V1 {
	// 	IntPtr blockPtr;
	// 	DActionArity1V1 invoker;

	// 	[Preserve (Conditional = true)]
	// 	public unsafe NIDActionArity1V1 (BlockLiteral* block)
	// 	{
	// 		blockPtr = (IntPtr) block;
	// 		invoker = block->GetDelegateForBlock<DActionArity1V1> ();
	// 	}
	// 	[Preserve (Conditional = true)]
	// 	public unsafe static global::System.Action<UIBackgroundFetchResult> Create (IntPtr block)
	// 	{
	// 		return new NIDActionArity1V1 ((BlockLiteral*) block).Invoke;
	// 	}

	// 	[Preserve (Conditional = true)]
	// 	unsafe void Invoke (UIBackgroundFetchResult obj)
	// 	{
	// 		invoker (blockPtr, obj);
	// 	}
	// }
}
