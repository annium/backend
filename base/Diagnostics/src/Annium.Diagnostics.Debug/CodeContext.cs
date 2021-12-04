using System.IO;

namespace Annium.Diagnostics.Debug;

public readonly struct CodeContext
{
    public readonly string Caller;
    public readonly string Member;
    public readonly int Line;

    public CodeContext(
        string callerFilePath,
        string member,
        int line
    )
    {
        Caller = Path.GetFileNameWithoutExtension(callerFilePath);
        Member = member;
        Line = line;
    }

    public override string ToString() => $"{Caller}.{Member}#{Line}";
}