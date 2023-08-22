using Microsoft.CodeAnalysis;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal static class NameMangler
	{
		internal static string GetRegistrarCallbackIdentifier(string nativeClassName, ExportedMemberInfo memberInfo)
		{
			var selector = memberInfo.Attribute?.Selector?.Replace(":", "_") ?? memberInfo.Symbol.MetadataName.Replace("`", "_");
			// TODO encode the length as hexadecimal number or as base64 to shorten it?
			return $"callback_{nativeClassName.Length}_{nativeClassName}_{selector}";
		}
	}
}
