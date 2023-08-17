using Microsoft.CodeAnalysis;

namespace Xamarin.ManagedRegistrarGenerator
{
	public static class GeneratorDiagnostics
	{
		public static class Ids
		{
			// TODO: what should the IDs be? the SYSLIB prefix and the IDs are taken from ComInterfaceGenerator
			public const string Prefix = "SYSLIB"; // TODO what prefix?
			public const string RequiresAllowUnsafeBlocks = Prefix + "1062"; // TODO what ID?
			public const string InvalidGeneratedComClassAttributeUsage = Prefix + "1095"; // TODO what ID?
		}

		private const string Category = "ManagedRegistrarGenerator";

		public static readonly DiagnosticDescriptor RequiresAllowUnsafeBlocks =
			new DiagnosticDescriptor(
				Ids.RequiresAllowUnsafeBlocks,
				"'RegisterAttribute' and custom NSObject subclasses require unsafe code", // TODO extract resource
				"'RegisterAttribute' and custom NSObject subclasses require unsafe code. Project must be updated with '<AllowUnsafeBlocks>true</AllowUnsafeBlocks>'.", // TODO extract resource
				Category,
				DiagnosticSeverity.Error,
				isEnabledByDefault: true,
				description: "'RegisterAttribute' and custom NSObject subclasses require unsafe code. Project must be updated with '<AllowUnsafeBlocks>true</AllowUnsafeBlocks>'."); // TODO extract resource

		public static readonly DiagnosticDescriptor InvalidAttributedClassMissingPartialModifier =
			new DiagnosticDescriptor(
				Ids.InvalidGeneratedComClassAttributeUsage,
				"Invalid subclass of NSObject",
				"NSObject subclass '{0}' must be marked partial",
				Category,
				DiagnosticSeverity.Error,
				isEnabledByDefault: true,
				description: "NSObject subclasses must be marked partial.");
	}
}
