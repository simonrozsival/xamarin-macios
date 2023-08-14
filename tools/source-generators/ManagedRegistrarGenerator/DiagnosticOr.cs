using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xamarin.ManagedRegistrarGenerator
{
	internal sealed class DiagnosticOr<T>
		where T : class
	{
		private readonly object _value;

		private DiagnosticOr(object @value) => _value = @value;

		public bool IsDiagnostic => _value is Diagnostic;
		public bool IsValue => _value is T;

		public Diagnostic Diagnostic => (Diagnostic)_value;
		public T Value => (T)_value;

		public static DiagnosticOr<T> FromValue(T value) => new(value);
		public static DiagnosticOr<T> FromDiagnostic(Diagnostic diagnostic) => new(diagnostic);

		public static (IncrementalValuesProvider<Diagnostic>, IncrementalValuesProvider<T>) Split(
			IncrementalValuesProvider<DiagnosticOr<T>> diagnosticOr)
		{
			var diagnostics = diagnosticOr.Where(static data => data.IsDiagnostic).Select(static (data, _) => data.Diagnostic);
			var values = diagnosticOr.Where(static data => data.IsValue).Select(static (data, _) => data.Value);
			return (diagnostics, values);
		}
	}
}
