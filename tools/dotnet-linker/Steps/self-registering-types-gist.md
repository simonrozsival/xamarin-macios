# Self-registering types

The idea is to generate static constructor that will register the type:

```csharp
// ObjCRuntime
interface IManagedRegistrarType {
    static virtual NSObject CreateNSObject(NativeHandle handle) => new NotImplementedException();
    static virtual INativeObject CreateINativeObject(NativeHandle handle, bool owns) => new NotImplementedException();
}

// Customer's code:
partial class MyCustomNSObject : NSObject {
    // ...
}

// Generated code:
partial class MyCustomNSObject : IManagedRegistrarType {
    static MyCustomNSObject() => ManagedRegistrar.Registar<MyCustomObject>();
    // - if the type has a "NativeHandle" ctor, we will generate `CreateNSObject` override
    // - if the type has a "NativeHandle, bool" ctor, we will generate `CreateINativeObject` override
}
```

- Types which already have a static constructor should call the `ManagedRegistrar.Register<T>()` method manually.
- The method call is a no-op (no exception thrown) when the MSR isn't used.

- Pros:
    - It is trimmable. We can generate all the .NET code _before_ trimming (either in a linker step or maybe even in roslyn source generator).
    - Not all class constructors run at startup.
    - The `Runtime.ConstructNSObject()` and related methods get much simpler (especially when compared to https://github.com/xamarin/xamarin-macios/pull/18519)
- Cons:
    - It's not clear to me how _bad_ it is to use `RuntimeHelpers.RunClassConstructor(handle)` and what its impact is
        - From my tests it works _just fine_ with both Mono and NativeAOT
        - It doesn't seem to add _too much_ extra size to the app.
    - There's a scenario in which the `LookupType(uint typeId)` method is called _before_ the type is registered
      and there's no way to invoke the class ctor at that point. It's not clear if this is a real-world scenario though
      (I haven't run all unit tests with a PoC yet).
 
### Alternative ideas:
- "trimmable module initializers"
    - basically static constructors that _all_ run at startup
    - all C# module initializers are called from _module_ static constructor -> that makes them untrimmable.
    - the trimmer would need to trim them and remove all calls to the method if its type was trimmed
         - "weak method calls"?


## The Gist of the Registrar

```csharp
static class RegistrarHelper
{
    private static readonly Dictionary<string, ManagedRegistrar> assembly_map = new();

    public static void Register<T> () where T : IManagedRegistrarType {
        if (!assembly_map.TryGetValue (typeof(T).Assembly.FullName, out ManagedRegistrar registrar)) {
            assembly_map[typeof(T).Assembly.FullName] = registrar = new ManagedRegistrar ();
        }

        registrar.AddType<T>();
    }

    public static NSObject? CreateNSObject(Assembly assembly, NativeHandle nativeHandle) {
        if (assembly_map.TryGetValue (assembly.FullName, out ManagedRegistrar registrar)) {
            return registrar.CreateNSObject(nativeHandle);
        }

        throw new InvalidOperationException($"...");
    }

    // CreateINativeObject
    // LookupTypeId
    // LookupType
}

sealed class ManagedRegistrar
{
    private readonly Dictionary<RuntimeTypeHandle, Func<NativeHandle, NSObject>> _nsObjectFactories = new();
    private readonly Dictionary<RuntimeTypeHandle, Func<NativeHandle, bool, INativeObject>> _nativeObjectFactories = new();
    private readonly Dictionary<RuntimeTypeHandle, uint> _typeIds = new();
    private readonly Dictionary<uint, RuntimeTypeHandle> _typeHandles = new();

    internal void AddType<T> () where T : IManagedRegistrarType {
        var handle = typeof(T).TypeHandle;
        var typeId = ManagedRegistrarHelper.CalculateTypeId(...);

        _nsObjectFactories.Add(handle, T.CreateNSObject);
        _nativeObjectFactories.Add(handle, T.CreateNativeObject);

        // Note: there has to be some extra care taken of generic types
        _typeIds.Add(handle, typeId);
        _typeHandles.Add(typeId, handle);
    }

    internal NSObject CreateNSObject(RuntimeTypeHandle typeHandle, NativeHandle nativeHandle) {
        // Force running the static constructor of the given type -> this will register the type
        // before we get to the point where we read from the dictionary...
        RuntimeHelpers.RunClassConstructor(typeHandle);

        if (_nsObjectFactories.TryGetValue(typeHandle, out var factory))
            return factory(nativeHandle);

        throw new InvalidOperationException($"...");
    }
    
    // CreateINativeObject
    // LookupTypeId

    internal RuntimeTypeHandle LookupType (uint typeId) {
        // TODO: PROBLEM? We must _hope_ that by the time this method is called
        // the cctor of the type has already registered itself
        // - This hasn't been an issue in my prototype and I still have to check
        //   if there's a code path that would really need those IDs.
        return _typeHandles[typeId];
    }

    // TODO: Protocol wrappers can be solved in a similar way, I'm omitting that to make the gist shorter...
}

static class ManagedRegistrarHelper {
    internal static uint CalculateTypeId (string assemblyName, string typeName)
        => /* calculate deterministic 24 bit hash */0u;
}
```

## Type IDs

- Currently, we assign each type a numeric ID starting from 0. We do /this in a custom linker step _after_ trimming and
  we use it to generate .NET lookup tables in `ManagedRegistrarLookupTablesStep` and ObjC code generated in `StaticRegistrar`.
- Instead, we would generate the IDs deterministically from the type's assembly name and type name. We can do this
  to generate the .NET and Objective-C code completely independently.
- DO WE NEED THEM AT ALL??
    - They are used a lot in the `Class` class:
        - The `LookupTypeId` method is only called from ONE specific place in the codebase:
            - `RegistrarHelper.LookupRegisteredType` via `Class.ResolveToken`
        - The `Class.ResolveToken` method is called in TWO places in `Class`:
            - `MemberInfo? ResolveFullTokenReference(uint token_reference)`
            - `MemberInfo? ResolveTokenReference (uint token_reference, uint implicit_token_type)`
            - These methods load information about the `token_reference` through the `__xamarin_token_references` table
              generated in the `registrar.mm`
            - These methods are called from:
                - `Blocks.GetDelegateProxyType`
                - `Class`:
                    - `Type? FindType (NativeHandle @class, out bool is_custom_type)`
                - `Runtime`:
                    - `Type? FindProtocolWrapperType (Type? type)`
                    - `GetINativeObject_Static`
        - The (main?) use case in the registrar - `var obj = Runtime.GetNSObject (handle);`
            - We have a native object with native handle `handle`
            - We use Objective-C APIs to get the Objective-C class of the object `@class`
            - We find the .NET `type` correspondign to `@class` through the `uint typeId` that we embedd in the generated
              Objective-C code
            - We can then create an instance of the .NET type that wraps the Objective-C type through `ConstructNSObject`
        - Idea - we should be able to just call a method on an instance of an object and it should tell us what's its type.
            - C#-first objects:
                - if the `handle` belongs to a peer ObjC class to a user-defined C# class, the object
                  could respond to a selector we define (`-(int) dotnetTypeHandle`) which will call an UCO that will return
                  the `RuntimeTypeHandle` without ANY typeId
            - Objective-C-first objects: (e.g. `UIWindow`)
                > In Objective-C, categories are a way to add new methods and properties to existing classes without subclassing them. This means that categories can be used to extend the functionality of a class without having to write additional code. Categories can also be used to separate the code for a class into different logical sections, making the code easier to read and maintain.
                - we could generate categories for these types
                - the category method would call an UCO on the C# type (`__Registrar_Callbacks__`) and return
                  the runtime type handle of the object
                - AND if we're doing that... could it even return the GC handle of its counterpart?

        - By calling the UCO we will force the static constructor to run (although it's not a problem anymore).
        - My only concern is that we would be calling too many pinvokes and reverse pinvokes
            - That shouldn't be an issue in AOTed code, should it?
            - The current code is quite "convoluted" already and it might simplify the code quite a lot.
                - Although we would share less code with the Static registrar. That's OK though I think.

```csharp
class Runtime {
    public static NSObject? GetNSObject(NativeHandle nativeHandle) {
        if (TryGetExistingObjectFor(nativeHandle, out var obj))
            return obj;

        var ptr = Messaging.IntPtr_objc_msgSend_IntPtr(nativeHandle, new Selector("createDotnetInstance"));
        return GCHandle.FromIntPtr(ptr).Target as NSObject;
    }

    public static Type? FindType (NativeHandle nativeHandle, out bool is_custom_type) {
        var ptr = Messaging.IntPtr_objc_msgSend_IntPtr(nativeHandle, new Selector("getDotnetType"));
        return GCHandle.FromIntPtr(ptr).Target as Type;
    }
}

class Class {
    public static Type? FindType (NativeHandle @class, out bool is_custom_type)
    {
        var ptr = Messaging.IntPtr_objc_msgSend_IntPtr(@class, new Selector("getDotnetType"));
        return GCHandle.FromIntPtr(ptr).Target as Type;
    }
}

class CustomClass : NSObject {
    public CustomClass(NativeHandle handle) : base(handle) {}

    private static class __Registrar_Callbacks__ {
        [UnmanagedCallersOnly(EntryPoint = "_callback_CustomClass_CreateDotnetInstance:")]
        public static IntPtr CreateDotnetInstance(IntPtr self, IntPtr sel, GCHandle* exception_gchandle) {
            var obj = new CustomClass(self);
            return Runtime.AllocGCHandle(obj);
        }

        [UnmanagedCallersOnly(EntryPoint = "_callback_CustomClass_GetDotnetType:")]
        public static IntPtr GetDotnetType(IntPtr self, IntPtr sel, GCHandle* exception_gchandle) {
            return Runtime.AllocGCHandle(typeof(CustomClass));
        }
    }
}

```

```objc
@interface CustomClass { }
    -(void*) createDotnetInstance;
    -(void*) getDotnetType;
    +(void*) getDotnetType;
@end

@implementation CustomClass
    void* callback_CustomClass_CreateDotnetInstance (id self, SEL sel, GCHandle* exception_gchandle);
    -(void*) createDotnetInstance
    {
		GCHandle exception_gchandle = INVALID_GCHANDLE;
        void* gcHandle = callback_CustomClass_CreateDotnetInstance (self, _cmd, &exception_gchandle);
        xamarin_process_managed_exception_gchandle (exception_gchandle);
        return gcHandle;
    }

    void* callback_CustomClass_GetDotnetType (id self, SEL sel, GCHandle* exception_gchandle);
    -id getDotnetType
    {
        return [CustomClass getDotnetType];
    }

    +id getDotnetType
    {
        // the static method might need a different signature? I'm not sure how exactly this would work in Objective-C
        GCHandle exception_gchandle = INVALID_GCHANDLE;
        void* gcHandle = callback_CustomClass_CreateDotnetInstance (self, _cmd, &exception_gchandle);
        xamarin_process_managed_exception_gchandle (exception_gchandle);
        return gcHandle;
    }
@end

// a category for existing types
@interface UIWindow (UIWindowCategory) {}
-(void*) createDotnetInstance;
-(void*) getDotnetType;
+(void*) getDotnetType;
@end
```


Methods:
- Class.Lookup -> ObjC class -> .NET Type (continues up type hierarchy until it finds the best suitable type)
- Class.GetHandle -> objc_getClass
- Class.GetClassHandle -> also ptr to the ObjC class
- Class.ThrowOnInitFailure (bool = true)
- Class.IsCustomType
- Class.ResolveTypeTokenReference
- in Runtime:
    - Class.Initialize
    - Class.Register
    - Class.GetClassForObject
    - Class.class_getName
    - Class.GetTokenReference
    - Class.FindMapIndex
    - Class.class_getInstanceMethod

Other:
- Class.objc_attribute_prop