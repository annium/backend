using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Annium.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Entity this assembly's configuration applies to.
/// </summary>
public class Counted
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Configuration that records how many times it was applied, so a repeated assembly scan is visible.
/// </summary>
public class CountingConfiguration : IEntityTypeConfiguration<Counted>
{
    /// <summary>
    /// Gets or sets the number of times this configuration has been applied.
    /// </summary>
    public static int Applied { get; set; }

    /// <summary>
    /// Records the call and configures the entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Counted> builder)
    {
        Applied++;
        builder.HasKey(x => x.Id);
    }
}
