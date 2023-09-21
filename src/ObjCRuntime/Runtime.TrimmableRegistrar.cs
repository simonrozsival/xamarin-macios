//
// Runtime.cs: Mac/iOS shared runtime code
//
// Authors:
//   Miguel de Icaza
//
// Copyright 2013 Xamarin Inc.

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using CoreFoundation;
using Foundation;
using Registrar;

#if MONOMAC
using AppKit;
#endif

#if !NET
#error This file should only be included in the .NET build
#endif

namespace ObjCRuntime {

	public partial class Runtime {
		static Dictionary<IntPtr, GCHandle> object_map = new();

		// TODO use initialization flags
		static bool IsCoreClr => false; 

		internal static void RegisterNSObject (NSObject obj, IntPtr ptr)
		{
			GCHandle handle;
			if (Runtime.IsCoreCLR) {
				handle = CreateTrackingGCHandle (obj, ptr);
			} else {
				handle = GCHandle.Alloc (obj, GCHandleType.WeakTrackResurrection);
			}

			lock (lock_obj) {
				if (object_map.Remove (ptr, out var existing))
					existing.Free ();
				object_map [ptr] = handle;
				obj.Handle = ptr;
			}
		}

		internal static void UnregisterNSObject (IntPtr native_obj, IntPtr managed_obj)
		{
			NativeObjectHasDied (native_obj, GetGCHandleTarget (managed_obj) as NSObject);
		}

		internal static void NativeObjectHasDied (IntPtr ptr, NSObject? managed_obj)
		{
			lock (lock_obj) {
				if (object_map.TryGetValue (ptr, out var wr)) {
					if (managed_obj is null || wr.Target == (object) managed_obj) {
						object_map.Remove (ptr);
						wr.Free ();
					} else if (wr.Target is null) {
						// We can remove null entries, and free the corresponding GCHandle
						object_map.Remove (ptr);
						wr.Free ();
					}
				}

				if (managed_obj is not null)
					managed_obj.ClearHandle ();
			}
		}

		internal static T? TryGetNSObject<T> (IntPtr ptr, bool evenInFinalizerQueue)
			where T : NSObject
		{
			lock (lock_obj) {
				if (object_map.TryGetValue (ptr, out var reference)) {
					var target = reference.Target as T;
					if (target is null) {
						return null;
					}

					if (target.InFinalizerQueue) {
						if (!evenInFinalizerQueue) {
							// Don't return objects that's been queued for finalization unless requested to.
							return null;
						}

						if (target.IsDirectBinding && !target.IsRegisteredToggleRef) {
							// This is a non-toggled direct binding, which is safe to re-create.
							// We get here if the native object is still around, while the GC
							// has collected the managed object, and then we end up needing
							// a managed wrapper again before we've completely cleaned up 
							// the existing managed wrapper (which is the one we just found).
							// Returning null here will cause us to re-create the managed wrapper.
							// See bug #37670 for a real-world scenario.
							return null;
						}
					}

					return target;
				}
			}

			return null;
		}
	}
}
