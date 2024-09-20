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
			private static Lazy<NativeHandle> s_createManagedInstance = new (() => Selector.GetHandle ("__dotnet_CreateManagedInstance"));
			private static Lazy<NativeHandle> s_isUserType = new (() => Selector.GetHandle ("__dotnet_IsUserType"));

			internal static T? TryCreateManagedInstance<T> (NativeHandle handle)
				where T : class, INativeObject
			{
				return TryCreateManagedInstance (handle, typeof (T)) as T
					?? T.CreateManagedInstance (handle, owns: false) as T;
			}

			internal static INativeObject? TryCreateManagedInstance (NativeHandle handle, Type type)
			{
				EnsureManagedStaticRegistrar ();

				if (handle == NativeHandle.Zero) {
					// Runtime.NSLog ($"TryCreateManagedInstance ({handle:x}, {type}) => handle is zero");
					return null;
				}

				var instancePtr = RespondsToSelector (handle, s_createManagedInstance.Value)
					? Messaging.IntPtr_objc_msgSend (handle, s_createManagedInstance.Value)
					: CreateManagedInstanceViaFallback (handle, type);

				var managedInstance = GetGCHandleTarget (instancePtr);
				// Runtime.NSLog ($"TryCreateManagedInstance ({handle:x}, {type}) => {managedInstance}");

				if (managedInstance is Exception exception) {
					throw exception;
				}

				return (INativeObject?)managedInstance;
			}

			internal static T? CreateManagedInstance<T> (NativeHandle handle)
				where T : class, INativeObject
			{
				EnsureManagedStaticRegistrar ();

				if (handle == NativeHandle.Zero) {
					return null;
				}

				// Calling the Objective-C virtual factory method allows us to resolve the _actual_ derived type of the object
				INativeObject? obj = TryCreateManagedInstance (handle, typeof(T));
				if (obj is T managedInstance) {
					return managedInstance;
				}

				// Runtime.NSLog($"Could not create managed instance of {typeof(T)} via objective-c virtual factory method");

				// Fallback to the static factory method -- this is necessary for generic types, but it doesn't allow us to resolve the _actual_ derived type of the object if any
				INativeObject? obj2 = T.CreateManagedInstance (handle, owns: false);
				if (obj2 is T managedInstance2) {
					// TODO releoase `obj`?

					return managedInstance2;
				}

				// Runtime.NSLog($"Could not create managed instance of {typeof(T)} via .NET static virtual factory method");

				string expectedTypeName = typeof (T)?.FullName ?? "<unknown>";
				string actualObjectType = obj?.GetType().FullName ?? "<null>";
				string actualObjectType2 = obj2?.GetType().FullName ?? "<null>";

				// TODO release `obj` and `obj2`?

				// TODO throw or return null?
				// throw Runtime.CreateRuntimeException (8027, $"Failed to marshal Objective-C object {handle:x} to managed type {expectedTypeName} - got {actualObjectType} instead and INativeObject.CreateManagedInstance did not return a valid instance but {actualObjectType2} instead.");
				Runtime.NSLog ($"Failed to marshal Objective-C object {handle:x} to managed type {expectedTypeName} - got {actualObjectType} instead and INativeObject.CreateManagedInstance did not return a valid instance but {actualObjectType2} instead.");
				return null;
			}

			internal static unsafe NativeHandle GetNativeClass (Type type, out bool isCustomType)
			{
				EnsureManagedStaticRegistrar ();

				var selectorName = ConstructGetNativeClassSelector (type);
				var selector = Selector.GetHandle (selectorName);

				if (s_dotnet.Value == NativeHandle.Zero) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ("The __dotnet class is not available.");
				} else if (!RespondsToSelector (s_dotnet.Value, selector)) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ($"We are missing the {selectorName} ({selector}) selector on the __dotnet class.");
				}

				byte _isCustomType = 0;
				var handle = IntPtr_objc_msgSend_ref_byte (s_dotnet.Value, selector, &_isCustomType);
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
				=> $"{Sanitize (assemblyName)}__{Sanitize (typeName)}";

			private static void EnsureManagedStaticRegistrar()
			{
				if (!Runtime.IsManagedStaticRegistrar) {
					throw new UnreachableException ();
				}
			}

			private static IntPtr CreateManagedInstanceViaFallback (NativeHandle handle, Type type)
			{
				// Fallback in case of C-structs and protocol wrappers
				// TODO pass the `owns` parameter? right now we always assume it's `false`
				// TODO we don't generate the Objective-C code this is supposed to call

				var selectorName = ConstructCreateManagedInstanceSelector (type);
				var selector = Selector.GetHandle (selectorName);

				if (s_dotnet.Value == NativeHandle.Zero) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ("The __dotnet class is not available.");
				} else if (!ClassRespondsToSelector (s_dotnet.Value, selector)) {
					// TODO this is a bug in the registrar generator, this should never happen
					throw new UnreachableException ($"We are missing the {selectorName} ({selector}) selector on the __dotnet class.");
				}

				return Messaging.IntPtr_objc_msgSend (handle, selector);
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
			private unsafe extern static IntPtr IntPtr_objc_msgSend_ref_byte (IntPtr receiver, IntPtr selector, byte* isCustomType);

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
