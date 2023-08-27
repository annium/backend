using System;
using System.IO;
using Annium.Debug;
using Xunit.Abstractions;

namespace Annium.Testing.Lib.Internal;

internal class TestTracer : ITracer
{
    private readonly ITestOutputHelper _outputHelper;

    public TestTracer(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public void Trace<T>(
        T subject,
        string message,
        bool withTrace,
        string file,
        string member,
        int line
    ) where T : notnull
    {
        var subjectString = $"{subject.GetType().FriendlyName()}#{subject.GetId()}";
        var caller = Path.GetFileNameWithoutExtension(file);
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        _outputHelper.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ADBG [{Environment.CurrentManagedThreadId:D3}] {subjectString} at {caller}.{member}:{line} >> {message}{trace}");
    }
}