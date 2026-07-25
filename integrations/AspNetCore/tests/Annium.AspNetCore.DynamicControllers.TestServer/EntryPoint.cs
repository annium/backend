namespace Annium.AspNetCore.DynamicControllers.TestServer;

/// <summary>
/// Marker type identifying this assembly to <c>WebApplicationFactory&lt;TEntryPoint&gt;</c>.
/// The actual application entry point is the compiler-generated top-level-statement <c>Program</c> class in
/// <c>Program.cs</c>; it cannot be referenced by name here because <c>Annium.AspNetCore.TestServer</c> (also
/// referenced by consuming test projects) defines its own top-level <c>Program</c> class in the same global
/// namespace, which would make an unqualified <c>Program</c> reference ambiguous. Passing any public type from
/// this assembly is sufficient: <c>WebApplicationFactory&lt;TEntryPoint&gt;</c> only uses it to locate the
/// assembly, not the type itself.
/// </summary>
public sealed class EntryPoint;
