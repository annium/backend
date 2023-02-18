using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests;

public class TypeRecordExtensionsTest
{
    [Fact]
    public void TryGetRecordElementTypes_Ok()
    {
        typeof(string).TryGetRecordElementTypes().IsDefault();
        typeof(Array).TryGetRecordElementTypes().IsDefault();
        typeof(string[]).TryGetRecordElementTypes().IsDefault();
        typeof(IEnumerable).TryGetRecordElementTypes().IsDefault();
        typeof(IEnumerable<int>).TryGetRecordElementTypes().IsDefault();
        typeof(IReadOnlyDictionary<int, string>).TryGetRecordElementTypes().IsNotDefault();
        typeof(IReadOnlyDictionary<int, string>).TryGetRecordElementTypes().Is((typeof(int), typeof(string)));
    }
}