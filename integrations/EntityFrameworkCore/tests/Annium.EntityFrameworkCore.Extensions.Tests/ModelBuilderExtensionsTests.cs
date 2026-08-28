using System;
using System.Linq;
using Annium.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Annium.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for the model-wide conventions this package applies. Each walks the model the way EF Core does,
/// so a convention that reaches only part of it - the usual failure here - shows up as a name or a
/// converter that is missing rather than as an exception.
/// </summary>
public class ModelBuilderExtensionsTests
{
    /// <summary>
    /// Every column is snake_cased, including the ones declared on a derived type. Derived types are
    /// separate entity types in EF Core's model and their properties belong to them, not to the root, so
    /// a convention that walks only root types leaves half a table in one naming style and half in
    /// another - and the half it misses is whichever the hierarchy happens to declare later.
    /// </summary>
    [Fact]
    public void UseSnakeCase_NamesEveryColumn_IncludingOnDerivedTypes()
    {
        // arrange & act
        using var context = new TestContext();
        var model = context.Model;

        // assert - the root's own table and columns
        var animal = model.FindEntityType(typeof(Animal))!;
        animal.GetTableName().Is("animal");
        animal.GetProperty(nameof(Animal.LoudestCall)).GetColumnName().Is("loudest_call");

        // and a property declared by a type that shares that table
        var dog = model.FindEntityType(typeof(Dog))!;
        dog.GetProperty(nameof(Dog.FavouriteToy)).GetColumnName().Is("favourite_toy");
    }

    /// <summary>
    /// A nullable DateTime is read back as UTC too. The property's CLR type is what the convention filters
    /// on, and a value that is sometimes absent is still a timestamp when it is present - a caller has no
    /// way to tell that the one kind of column was converted and the other was not.
    /// </summary>
    [Fact]
    public void UseDateTimeUtc_Converts_NullableAsWellAsNot()
    {
        // arrange & act
        using var context = new TestContext();
        var animal = context.Model.FindEntityType(typeof(Animal))!;

        // assert
        animal.GetProperty(nameof(Animal.SeenAt)).GetValueConverter().IsNotDefault("a DateTime is converted");
        animal
            .GetProperty(nameof(Animal.LastFedAt))
            .GetValueConverter()
            .IsNotDefault("a nullable DateTime is a timestamp too");
    }

    /// <summary>
    /// Context whose model exercises the conventions: a hierarchy sharing one table, and both a required
    /// and an optional timestamp.
    /// </summary>
    private class TestContext : DbContext
    {
        /// <summary>
        /// Points the context at an in-memory SQLite database - a relational provider is needed for table
        /// and column names to exist at all, and nothing here opens a connection.
        /// </summary>
        /// <param name="options">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=:memory:");
        }

        /// <summary>
        /// Builds the model and applies the conventions under test.
        /// </summary>
        /// <param name="builder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Animal>();
            builder.Entity<Dog>();

            builder.UseSnakeCase();
            builder.UseDateTimeUtc();
        }
    }

    /// <summary>
    /// Root of the test hierarchy.
    /// </summary>
    private class Animal
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a property declared by the root type.
        /// </summary>
        public string LoudestCall { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a required timestamp.
        /// </summary>
        public DateTime SeenAt { get; set; }

        /// <summary>
        /// Gets or sets an optional timestamp.
        /// </summary>
        public DateTime? LastFedAt { get; set; }
    }

    /// <summary>
    /// Derived type sharing the root's table, with a property of its own.
    /// </summary>
    private class Dog : Animal
    {
        /// <summary>
        /// Gets or sets a property declared by the derived type, mapped into the root's table.
        /// </summary>
        public string FavouriteToy { get; set; } = string.Empty;
    }
}
