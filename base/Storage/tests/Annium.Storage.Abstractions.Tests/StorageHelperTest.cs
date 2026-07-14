using System;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.Abstractions.Tests;

/// <summary>
/// Tests for the shared path-validation helper. Every storage provider validates through it,
/// so its accept/reject boundaries are the contract all of them inherit.
/// </summary>
public class StorageHelperTest
{
    /// <summary>
    /// Tests that the bucket root is accepted as a root.
    /// </summary>
    [Fact]
    public void VerifyRoot_Root_Passes()
    {
        // assert: does not throw
        StorageHelper.VerifyRoot("/");
    }

    /// <summary>
    /// Tests that an absolute, non-trailing-slash directory is accepted as a root.
    /// </summary>
    [Fact]
    public void VerifyRoot_AbsoluteDirectory_Passes()
    {
        // assert: does not throw
        StorageHelper.VerifyRoot("/files/nested");
    }

    /// <summary>
    /// Tests that a relative root is rejected, since a root must be absolute.
    /// </summary>
    [Fact]
    public void VerifyRoot_Relative_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyRoot("files")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a root with a trailing slash is rejected.
    /// </summary>
    [Fact]
    public void VerifyRoot_TrailingSlash_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyRoot("/files/")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a root with a malformed part is rejected.
    /// </summary>
    [Fact]
    public void VerifyRoot_InvalidPart_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyRoot("/files/..")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that an empty prefix is accepted, since it means "no filtering".
    /// </summary>
    [Fact]
    public void VerifyPrefix_Empty_Passes()
    {
        // assert: does not throw
        StorageHelper.VerifyPrefix("");
    }

    /// <summary>
    /// Tests that a relative, non-trailing-slash prefix is accepted.
    /// </summary>
    [Fact]
    public void VerifyPrefix_Relative_Passes()
    {
        // assert: does not throw
        StorageHelper.VerifyPrefix("files/nested");
    }

    /// <summary>
    /// Tests that an absolute prefix is rejected, since a prefix is relative to the root.
    /// </summary>
    [Fact]
    public void VerifyPrefix_Absolute_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPrefix("/files")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a prefix with a trailing slash is rejected.
    /// </summary>
    [Fact]
    public void VerifyPrefix_TrailingSlash_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPrefix("files/")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a prefix with a malformed part is rejected.
    /// </summary>
    [Fact]
    public void VerifyPrefix_InvalidPart_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPrefix("files/..")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a relative, non-trailing-slash path is accepted.
    /// </summary>
    [Fact]
    public void VerifyPath_Relative_Passes()
    {
        // assert: does not throw
        StorageHelper.VerifyPath("files/nested.txt");
    }

    /// <summary>
    /// Tests that an empty path is rejected, since every operation needs a target.
    /// </summary>
    [Fact]
    public void VerifyPath_Empty_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPath("")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that an absolute path is rejected, since a path is relative to the root.
    /// </summary>
    [Fact]
    public void VerifyPath_Absolute_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPath("/files.txt")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a path with a trailing slash is rejected, since it names a directory, not a file.
    /// </summary>
    [Fact]
    public void VerifyPath_TrailingSlash_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPath("files/")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a path with a malformed part is rejected.
    /// </summary>
    [Fact]
    public void VerifyPath_InvalidPart_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPath(".")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that names may contain an underscore anywhere, including leading and trailing.
    /// </summary>
    /// <param name="path">The path expected to be accepted.</param>
    [Theory]
    [InlineData("_leading")]
    [InlineData("trailing_")]
    [InlineData("in_the_middle")]
    [InlineData("_")]
    [InlineData(".hidden")]
    [InlineData("dashed-name")]
    [InlineData("dotted.name")]
    public void VerifyPath_AllowedName_Passes(string path)
    {
        // assert: does not throw
        StorageHelper.VerifyPath(path);
    }

    /// <summary>
    /// Tests that a part containing punctuation is rejected. These characters all sit between Z and a
    /// in ASCII, so a name pattern written A-z rather than A-Za-z would wrongly accept them —
    /// the backslash among them separates directories on Windows, letting a part escape the root.
    /// </summary>
    /// <param name="path">The path expected to be rejected.</param>
    [Theory]
    [InlineData(@"back\slash")]
    [InlineData(@"back\..\..\escape")]
    [InlineData("caret^name")]
    [InlineData("tick`name")]
    [InlineData("bracket[name")]
    [InlineData("bracket]name")]
    public void VerifyPath_PunctuationInPart_ThrowsArgumentException(string path)
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyPath(path)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that the same punctuation is rejected in a root and in a prefix, not just in a path.
    /// </summary>
    [Fact]
    public void VerifyRootAndPrefix_PunctuationInPart_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => StorageHelper.VerifyRoot(@"/back\..\escape")).Throws<ArgumentException>();
        Wrap.It(() => StorageHelper.VerifyPrefix(@"back\..\escape")).Throws<ArgumentException>();
    }
}
