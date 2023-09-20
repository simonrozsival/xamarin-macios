# NativeAOT on macOS/iOS: Replacing _custom linker steps_ with a Roslyn Rource Generator

## Problem statement
The Native AOT app build on iOS requires running IL Linker with Xamarin-specific _custom linker steps_ in order to
expose certain .NET classes to the native (Objective-C) runtime. These custom linker steps modify IL Linker's trimming logic
and they also generate necessary "glue" IL and Objective-C code to facilitate bi-directional interop between the native
and managed runtime. The pre-processed managed assemblies are later AOT compiled using ILC.

We want to remove the IL Linker step from the build process and replace it with a different, simpler, solution.

## Proposed solution
We can use Roslyn Source Generators to generate fully-trimmable C# code which can be trimmed and compiled directly
using just CSC and ILC.

The solution involves several components:
1. Defining a modified version of _Managed Static Registrar_ that is fully trimmable
2. Implementing a Roslyn Source Generator and integrating it into the build process
3. Implementing missing features in ILC
4. Producing migration guide and migration tools for library authors and app developers
