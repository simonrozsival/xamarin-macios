// #if NET
#nullable enable

using System;
using System.Diagnostics;
using System.Text;

namespace ObjCRuntime {
	public static partial class Runtime {
		internal static class ManagedRegistrar {
			private static Lazy<NativeHandle> s_dotnet = new (() => Class.GetHandle ("__dotnet"));
			private static Lazy<NativeHandle> s_respondsToSelector = new (() => Selector.GetHandle ("respondsToSelector:"));
			private static Lazy<NativeHandle> s_createManagedInstance = new (() => Selector.GetHandle ("__dotnet_CreateManagedInstance:"));
			private static Lazy<NativeHandle> s_isUserType = new (() => Selector.GetHandle ("__dotnet_IsUserType"));

			internal static INativeObject? TryCreateManagedInstance (NativeHandle handle, Type type, bool owns)
			{
				EnsureManagedStaticRegistrar ();

				if (handle == NativeHandle.Zero) {
					return null;
				}

				if (!RespondsToSelector (handle, s_createManagedInstance.Value)) {
					return null;
				}

				// TODO if we only generated this method for NSObjects and not for C-structs, we could drop the `owns` parameter
				IntPtr instancePtr = IntPtr_objc_msgSend_bool (handle, s_createManagedInstance.Value, owns);

				object? managedInstance = GetGCHandleTarget (instancePtr);
				if (managedInstance is Exception exception) {
					throw exception;
				}

				// TODO: there was an idea to use the `type` parameter to implement the fallback via an ObjC class method on __dotnet:
				// var selector = Selector.GetHandle($"__dotnet_CreateManagedInstance_{Sanitize (type.FullName)}");
				// var instancePtr = IntPtr_objc_msgSend_IntPtr (s_dotnet.Handle, selector, handle);
				//
				// @implementation __dotnet
				//    +(id) __dotnet_CreateManagedInstance_CoreGraphics_CGColor:(id)ptr {
				//         ...
				//    }
				//
				// ... in the end I used the `INativeObject.CreateNativeObject` virtual static interface method because it allows
				//     support for generic types but maybe we need both fallbacks for C-structs for certain scenarios? do we?

				return managedInstance as INativeObject;
			}

			internal static T? TryCreateManagedInstance<T> (NativeHandle handle, bool owns)
				where T : class, INativeObject
			{
				EnsureManagedStaticRegistrar ();

				if (handle == NativeHandle.Zero) {
					return null;
				}

				INativeObject? obj = null;

				bool skipVirtualDispatch = !typeof (Foundation.NSObject).IsAssignableFrom (typeof (T)) || typeof (T).IsGenericType || typeof (T).IsInterface;
				if (!skipVirtualDispatch) {
					// Calling the Objective-C virtual factory method allows us to resolve the _actual_ derived type of the object
					obj = TryCreateManagedInstance (handle, typeof(T), owns);

					if (obj is T managedInstance) {
						return managedInstance;
					}

					string expectedTypeName = typeof (T)?.FullName ?? "<unknown>";
					string actualObjectType = obj?.GetType().FullName ?? "<null>";
					Runtime.NSLog ($"Could not create managed instance of {typeof(T)} via objective-c virtual dispatch. Got {actualObjectType} instead of {expectedTypeName}.");

					if (owns) {
						Runtime.TryReleaseINativeObject (obj);
					}
				}

				{
					// Fallback to the static factory method -- this is necessary for C-structs, generic types, and protocols. For generic types, which are NSObjects, it doesn't allow us to resolve the _actual_ derived type of the object but we're stuck at that very specific level of abstraction anyway.
					obj = T.CreateManagedInstance (handle, owns);

					if (obj is T managedInstance) {
						return managedInstance;
					}

					string expectedTypeName = typeof (T)?.FullName ?? "<unknown>";
					string actualObjectType = obj?.GetType().FullName ?? "<null>";
					Runtime.NSLog ($"Could not create managed instance of {typeof(T)} via objective-c virtual factory method. Got {actualObjectType} instead of {expectedTypeName}.");

					if (owns) {
						Runtime.TryReleaseINativeObject (obj);
					}
				}

				return null;
			}

			internal static T CreateManagedInstance<T> (NativeHandle handle, bool owns)
				where T : class, INativeObject
			{
				return TryCreateManagedInstance<T> (handle, owns)
					?? throw new InvalidOperationException ($"Could not create managed instance of {typeof(T)} for {handle:x}.");
			}

			internal static unsafe NativeHandle GetNativeClass (Type type, out bool isCustomType)
			{
				EnsureManagedStaticRegistrar ();

				string selectorName = ConstructGetNativeClassSelector (type);
				IntPtr selector = Selector.GetHandle (selectorName);

				if (s_dotnet.Value == NativeHandle.Zero) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ("The __dotnet class is not available.");
				} else if (!RespondsToSelector (s_dotnet.Value, selector)) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ($"We are missing the {selectorName} ({selector}) selector on the __dotnet class.");
				}

				byte _isCustomType = 0;
				IntPtr handle = IntPtr_objc_msgSend_ref_byte (s_dotnet.Value, selector, &_isCustomType);
				isCustomType = _isCustomType == 1;
				return handle;
			}

			internal static bool IsUserType (NativeHandle classHandle)
			{
				EnsureManagedStaticRegistrar ();

				return classHandle != NativeHandle.Zero
					&& ClassRespondsToSelector (classHandle, s_isUserType.Value)
					&& Messaging.bool_objc_msgSend (classHandle, s_isUserType.Value) == 1; // TODO it might not even be necessary to call this selector, it's simply there :D --> should it be an optional property or something like that maybe?
			}

			private static string ConstructGetNativeClassSelector (Type type)
				=> ConstructGetNativeClassSelector (type.Assembly!.GetName ()!.Name!, type.FullName!);
				
			private static string ConstructGetNativeClassSelector (string assemblyName, string typeName)
				=> $"__dotnet_GetNativeClass_{MangleName (assemblyName, typeName)}:";

			private static string ConstructCreateManagedInstanceSelector (Type type)
				=> ConstructCreateManagedInstanceSelector (type.Assembly!.GetName ()!.Name!, type.FullName!);

			private static string ConstructCreateManagedInstanceSelector (string assemblyName, string typeName)
				=> $"__dotnet_CreateManagedInstance_{MangleName (assemblyName, typeName)}:";

			private static string MangleName (string assemblyName, string typeName)
				=> $"{Sanitize (assemblyName)}__{Sanitize (DropGenericsIfNeeded (typeName))}";

			private static string DropGenericsIfNeeded(string typeName)
				=> typeName.IndexOf ("[[") switch {
					-1 => typeName,
					int n => typeName.Substring (0, n),
				};

			private static void EnsureManagedStaticRegistrar()
			{
				if (!Runtime.IsManagedStaticRegistrar) {
					throw new UnreachableException ();
				}
			}

			private static bool ClassRespondsToSelector (NativeHandle handle, NativeHandle selectorHandle)
				=> class_respondsToSelector (handle, selectorHandle) == 1;

			private static bool RespondsToSelector (NativeHandle handle, NativeHandle selectorHandle)
				=> Messaging.bool_objc_msgSend_IntPtr (handle, s_respondsToSelector.Value, selectorHandle) == 1;

			// TODO how do I put this into the `Messaging` class? all of the Messaging methods are generated somehow.
			[System.Runtime.InteropServices.DllImport (Messaging.LIBOBJC_DYLIB, EntryPoint = "class_respondsToSelector")]
			private unsafe extern static byte class_respondsToSelector (IntPtr receiver, IntPtr selector);

			// TODO how do I put this into the `Messaging` class? all of the Messaging methods are generated somehow.
			[System.Runtime.InteropServices.DllImport (Messaging.LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
			private unsafe extern static IntPtr IntPtr_objc_msgSend_ref_byte (IntPtr receiver, IntPtr selector, byte* _byte);

			// TODO how do I put this into the `Messaging` class? all of the Messaging methods are generated somehow.
			[System.Runtime.InteropServices.DllImport (Messaging.LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
			private unsafe extern static IntPtr IntPtr_objc_msgSend_bool (IntPtr receiver, IntPtr selector, bool _bool);

			private static string Sanitize (string str)
			{
				str = str.Replace ('.', '_');
				str = str.Replace ('-', '_');
				str = str.Replace ('+', '_');
				str = str.Replace ('/', '_');
				str = str.Replace ('`', '_');
				str = str.Replace ('<', '_');
				str = str.Replace ('>', '_');
				str = str.Replace ('$', '_');
				str = str.Replace ('@', '_');
				str = EncodeNonAsciiCharacters (str);
				str = str.Replace ('\\', '_');
				return str;
			}

			private static string EncodeNonAsciiCharacters (string value)
			{
				StringBuilder? sb = null;
				for (int i = 0; i < value.Length; i++) {
					char c = value [i];
					if (c > 127) {
						if (sb is null) {
							sb = new StringBuilder (value.Length);
							sb.Append (value, 0, i);
						}
						sb.Append ("\\u");
						sb.Append (((int) c).ToString ("x4"));
					} else if (sb is not null) {
						sb.Append (c);
					}
				}
				return sb is not null ? sb.ToString () : value;
			}
		}
	}
}

// #endif
