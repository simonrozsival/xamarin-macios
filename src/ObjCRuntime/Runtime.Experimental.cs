#nullable enable
#if NET

using System;

namespace ObjCRuntime {
	public static partial class Runtime {
		public static class ManagedRegistrar {
			private static Lazy<NativeHandle> s_dotnetTypes = new (() => Class.GetHandle ("__DotnetTypes__"));
			private static Lazy<NativeHandle> s_respondsToSelector = new (() => Selector.GetHandle ("respondsToSelector:"));
			private static Lazy<NativeHandle> s_createManagedInstance = new (() => Selector.GetHandle ("__dotnet_CreateManagedInstance"));
			private static Lazy<NativeHandle> s_isUserType = new (() => Selector.GetHandle ("__dotnet_IsUserType"));

			public static T? TryGetManagedInstance<T> (NativeHandle handle, bool evenInFinalizerQueue = false) where T : INativeObject
			{
				EnsureManagedStaticRegistrar ();

				return TryGetNSObject (handle, evenInFinalizerQueue) as T;
			}

			public static bool CanCreateManagedInstance (NativeHandle handle)
			{
				EnsureManagedStaticRegistrar ();

				return handle != NativeHandle.Zero
					&& RespondsToSelector (handle, s_createManagedInstance);
			}

			public static T? TryCreateManagedInstance<T> (NativeHandle handle) where T : INativeObject
			{
				EnsureManagedStaticRegistrar ();

				if (!CanCreateManagedInstance (handle)) {
					return null;
				}

				var objectHandle = Messaging.IntPtr_objc_msgSend (handle.Handle, s_createManagedInstance);
				return GetGCHandleTarget (objectHandle) as T;
			}

			public static T? CreateManagedInstance<T> (NativeHandle handle) where T : INativeObject
			{
				EnsureManagedStaticRegistrar ();

				if (handle == NativeHandle.Zero) {
					return null;
				}

				return TryCreateManagedInstance<T> (handle)
					?? throw Runtime.CreateRuntimeException (8027, $"Failed to marshal Objective-C object 0x{handle.Handle:x} to managed type {typeof (T)}.");
			}

			public static T? GetOrCreateManagedInstance<T> (NativeHandle handle)
				where T : INativeObject
			{
				EnsureManagedStaticRegistrar ();

				return TryGetManagedInstance<T> (handle)
					?? CreateManagedInstance<T> (handle);
			}

			public static NativeHandle GetNativeClass (Type type, out bool isCustomType)
			{
				EnsureManagedStaticRegistrar ();

				var selector = Selector.GetHandle (ConstructGetNativeClassSelector (type));

				if (s_dotnetTypes.Value == NativeHandle.Zero) {
					throw new InvalidOperationException ("The __DotnetTypes__ class is not available.");
				} else if (!RespondsToSelector (s_dotnetTypes.Value, selector)) {
					throw new InvalidOperationException ($"We are missing the {selector} selector on the __DotnetTypes__ class.");
				}
				
				byte _isCustomType = 0;
				var handle = Messaging.IntPtr_objc_msgSend_ref_byte (s_dotnetTypes.Value.Handle, selector, &_isCustomType);
				isCustomType = _isCustomType == 1;
				return handle;
			}

			public bool IsUserType(NativeHandle classHandle)
			{
				EnsureManagedStaticRegistrar ();

				return classHandle != NativeHandle.Zero
					&& RespondsToSelector (classHandle, s_isUserType)
					&& Messaging.bool_objc_msgSend_IntPtr (classHandle, s_isUserType); // TODO it might not even be necessary to call this selector, it's simply there :D --> should it be an optional property or something like that maybe?
			}

			private static string ConstructGetNativeClassSelector (Type type)
				=> ConstructGetNativeClassSelector (type.Assembly.GetName ().Name, type.FullName);

			internal static string ConstructGetNativeClassSelector (string assemblyName, string typeName)
			{
				return $"_dotnet_{Sanitize(assemblyName)}__{Sanitize(typeName)}_GetNativeClass:";

				static string Sanitize (string str)
				{
					str = str.Replace ('.', '_');
					str = str.Replace ('/', '_');
					str = str.Replace ('`', '_');
					str = str.Replace ('<', '_');
					str = str.Replace ('>', '_');
					str = str.Replace ('$', '_');
					str = str.Replace ('@', '_');
					str = StaticRegistrar.EncodeNonAsciiCharacters (str);
					str = str.Replace ('\\', '_');
					return str;
				}
			}

			private static void EnsureManagedStaticRegistrar()
			{
				if (!Runtime.IsManagedStaticRegistrar) {
					throw new UnreachableException();
				}
			}

			private static bool RespondsToSelector (NativeHandle handle, NativeHandle selectorHandle)
				=> Messaging.bool_objc_msgSend_IntPtr (handle, s_respondsToSelector, selectorHandle);
		}
	}
}

#endif
