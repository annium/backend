using System;
using System.Text.RegularExpressions;

namespace Annium.Storage.Abstractions;

public static class StorageHelper
{
    private static readonly Regex _nameRe =
        new(@"^(?:[A-z0-9]+|\.?[A-z0-9]+[A-z0-9-_.]*[A-z0-9]+)$", RegexOptions.Compiled | RegexOptions.Singleline);

    public static void VerifyRoot(string root)
    {
        if (root == "/")
            return;

        EnsureStartingSlash(root);
        EnsureNoTrailingSlash(root);
        VerifyPathParts(root);
    }

    public static void VerifyPrefix(string prefix)
    {
        if (prefix == "")
            return;

        EnsureNoStartingSlash(prefix);
        EnsureNoTrailingSlash(prefix);
        VerifyPathParts(prefix);
    }

    public static void VerifyPath(string path)
    {
        if (path == "")
            throw new ArgumentException($"Path {path} is empty");

        EnsureNoStartingSlash(path);
        EnsureNoTrailingSlash(path);
        VerifyPathParts(path);
    }

    private static void EnsureStartingSlash(string path)
    {
        if (!path.StartsWith('/'))
            throw new ArgumentException($"Path {path} is not absolute");
    }

    private static void EnsureNoStartingSlash(string path)
    {
        if (path.StartsWith('/'))
            throw new ArgumentException($"Path {path} is absolute");
    }

    private static void EnsureNoTrailingSlash(string path)
    {
        if (path.EndsWith('/'))
            throw new ArgumentException($"Path {path} must not end with /");
    }

    private static void VerifyPathParts(string path)
    {
        foreach (var part in path.TrimStart('/').Split('/'))
            if (!_nameRe.IsMatch(part))
                throw new ArgumentException($"Path part {part} has invalid format");
    }
}
