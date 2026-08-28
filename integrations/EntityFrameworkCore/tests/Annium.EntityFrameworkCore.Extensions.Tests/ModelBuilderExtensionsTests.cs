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
    /// A configuration is applied once, even when the context implements an interface declared beside it.
    /// The assemblies were deduplicated before the context's own was added, so that assembly appeared
    /// twice - harmless for a fluent call made twice, but seed data applied twice is a duplicate key.
    /// </summary>
    [Fact]
    public void ApplyConfigurations_ContextImplementingLocalInterface_AppliesEachAssemblyOnce()
    {
        // arrange
        CountingConfiguration.Applied = 0;

        // act
        using var context = new CountingContext();
        _ = context.Model;

        // assert
        CountingConfiguration.Applied.Is(1, "the same assembly must not be scanned twice");
    }

    /// <summary>
    /// An owned type's link to its owner keeps cascading. EF Core requires it: the owned row exists only
    /// as part of the owner, so anything else leaves it behind when the owner goes. Nothing rejects the
    /// change at model-build time, which is why setting it is a silent way to strand rows.
    /// </summary>
    [Fact]
    public void UseDeleteBehavior_LeavesOwnershipAlone()
    {
        // arrange & act
        using var context = new OwnershipContext();
        var owned = context.Model.FindEntityType(typeof(Address))!;

        // assert
        var ownership = owned.GetForeignKeys().Single(x => x.IsOwnership);
        ownership.DeleteBehavior.Is(DeleteBehavior.Cascade, "an ownership must keep cascading");
    }

    /// <summary>
    /// Asking for a null where none can be stored is refused while the model is built, rather than when a
    /// row is deleted. A required relationship has nowhere to put the null, and EF Core does not check
    /// this at build time - so without the guard the first failure is a constraint violation at
    /// SaveChanges, a long way from the call that caused it.
    /// </summary>
    [Fact]
    public void UseDeleteBehavior_SetNullOnRequiredRelationship_IsRefused()
    {
        // act & assert
        var error = Wrap.It(() =>
            {
                using var context = new RequiredRelationContext();
                _ = context.Model;
            })
            .Throws<ArgumentException>();

        error.Message.Contains("required relationship").IsTrue("the message must name what cannot be done");
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
    /// Context implementing an interface declared in this same assembly.
    /// </summary>
    private class CountingContext : DbContext, ILocalMarker
    {
        /// <summary>
        /// Points the context at an in-memory SQLite database.
        /// </summary>
        /// <param name="options">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=:memory:");
        }

        /// <summary>
        /// Applies configurations the way a consumer does.
        /// </summary>
        /// <param name="builder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurations<CountingContext>();
        }
    }

    /// <summary>
    /// Marker interface declared beside the context that implements it.
    /// </summary>
    private interface ILocalMarker;

    /// <summary>
    /// Context with an owned type, to check what the delete-behaviour convention does to ownerships.
    /// </summary>
    private class OwnershipContext : DbContext
    {
        /// <summary>
        /// Points the context at an in-memory SQLite database.
        /// </summary>
        /// <param name="options">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=:memory:");
        }

        /// <summary>
        /// Builds a model with an owned type and applies the convention.
        /// </summary>
        /// <param name="builder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Person>().OwnsOne(x => x.Address);

            builder.UseDeleteBehavior(DeleteBehavior.Restrict);
        }
    }

    /// <summary>
    /// Context with a required relationship, to check what the delete-behaviour convention refuses.
    /// </summary>
    private class RequiredRelationContext : DbContext
    {
        /// <summary>
        /// Points the context at an in-memory SQLite database.
        /// </summary>
        /// <param name="options">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=:memory:");
        }

        /// <summary>
        /// Builds a model with a required relationship and asks for a behaviour it cannot hold.
        /// </summary>
        /// <param name="builder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Child>().HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).IsRequired();

            builder.UseDeleteBehavior(DeleteBehavior.SetNull);
        }
    }

    /// <summary>
    /// Principal of the required relationship.
    /// </summary>
    private class Parent
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Dependent of the required relationship.
    /// </summary>
    private class Child
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the required foreign key.
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the principal.
        /// </summary>
        public Parent Parent { get; set; } = new();
    }

    /// <summary>
    /// Owner of an owned type.
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the owned value.
        /// </summary>
        public Address Address { get; set; } = new();
    }

    /// <summary>
    /// Value owned by <see cref="Person"/>.
    /// </summary>
    private class Address
    {
        /// <summary>
        /// Gets or sets the street.
        /// </summary>
        public string Street { get; set; } = string.Empty;
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
