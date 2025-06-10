using System;
using System.Text.RegularExpressions;

namespace Annium.Storage.Abstractions;

/// <summary>
/// Provides utility methods for validating storage paths and ensuring they conform to storage requirements
/// </summary>
public static class StorageHelper
{
    /// <summary>
    /// Regular expression for validating path component names
    /// </summary>
    private static readonly Regex _nameRe = new(
        @"^(?:[A-z0-9]+|\.?[A-z0-9]+[A-z0-9-_.]*[A-z0-9]+)$",
        RegexOptions.Compiled | RegexOptions.Singleline
    );

    /// <summary>
    /// Verifies that a root path is valid for storage operations
    /// </summary>
    /// <param name="root">The root path to verify</param>
    public static void VerifyRoot(string root)
    {
        if (root == "/")
            return;

        EnsureStartingSlash(root);
        EnsureNoTrailingSlash(root);
        VerifyPathParts(root);
    }

    /// <summary>
    /// Verifies that a prefix path is valid for storage operations
    /// </summary>
    /// <param name="prefix">The prefix path to verify</param>
    public static void VerifyPrefix(string prefix)
    {
        if (prefix == "")
            return;

        EnsureNoStartingSlash(prefix);
        EnsureNoTrailingSlash(prefix);
        VerifyPathParts(prefix);
    }

    /// <summary>
    /// Verifies that a file path is valid for storage operations
    /// </summary>
    /// <param name="path">The file path to verify</param>
    public static void VerifyPath(string path)
    {
        if (path == "")
            throw new ArgumentException($"Path {path} is empty");

        EnsureNoStartingSlash(path);
        EnsureNoTrailingSlash(path);
        VerifyPathParts(path);
    }

    /// <summary>
    /// Ensures the path starts with a forward slash for absolute paths
    /// </summary>
    /// <param name="path">The path to check</param>
    private static void EnsureStartingSlash(string path)
    {
        if (!path.StartsWith('/'))
            throw new ArgumentException($"Path {path} is not absolute");
    }

    /// <summary>
    /// Ensures the path does not start with a forward slash for relative paths
    /// </summary>
    /// <param name="path">The path to check</param>
    private static void EnsureNoStartingSlash(string path)
    {
        if (path.StartsWith('/'))
            throw new ArgumentException($"Path {path} is absolute");
    }

    /// <summary>
    /// Ensures the path does not end with a trailing slash
    /// </summary>
    /// <param name="path">The path to check</param>
    private static void EnsureNoTrailingSlash(string path)
    {
        if (path.EndsWith('/'))
            throw new ArgumentException($"Path {path} must not end with /");
    }

    /// <summary>
    /// Verifies that all parts of a path conform to the valid naming pattern
    /// </summary>
    /// <param name="path">The path to verify</param>
    private static void VerifyPathParts(string path)
    {
        foreach (var part in path.TrimStart('/').Split('/'))
            if (!_nameRe.IsMatch(part))
                throw new ArgumentException($"Path part {part} has invalid format");
    }
}
