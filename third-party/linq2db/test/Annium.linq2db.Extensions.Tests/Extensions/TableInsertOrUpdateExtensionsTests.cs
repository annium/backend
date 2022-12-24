using System;
using System.Threading.Tasks;
using Annium.linq2db.Extensions.Extensions;
using Annium.linq2db.Extensions.Tests.Db;
using Annium.linq2db.Extensions.Tests.Db.Models;
using Annium.Testing;
using Annium.Testing.Lib;
using LinqToDB;
using Xunit;

namespace Annium.linq2db.Extensions.Tests.Extensions;

public class TableInsertOrUpdateExtensionsTests : TestBase
{
    public TableInsertOrUpdateExtensionsTests()
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task Insert()
    {
        // arrange
        await using var conn = Get<Connection>();
        var companyName = $"demo:{Guid.NewGuid()}";
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(companyName, metadata);

        // act
        await conn.Companies.InsertAsync(company);

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == companyName);
        company.Metadata.Is(metadata);
    }

    [Fact]
    public async Task Update()
    {
        // arrange
        await using var conn = Get<Connection>();
        var companyName = $"demo:{Guid.NewGuid()}";
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(companyName, metadata);
        await conn.Companies.InsertAsync(company);

        // act
        metadata = new CompanyMetadata("outdoors");
        await conn.Companies.UpdateAsync(company with { Metadata = metadata });

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == companyName);
        company.Metadata.Is(metadata);
    }

    [Fact]
    public async Task InsertOrUpdate()
    {
        // arrange
        await using var conn = Get<Connection>();
        var companyName = $"demo:{Guid.NewGuid()}";
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(companyName, metadata);

        // act
        await conn.Companies.InsertOrUpdateAsync(company);

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == companyName);
        company.Metadata.Is(metadata);

        // act
        metadata = new CompanyMetadata("outdoors");
        await conn.Companies.InsertOrUpdateAsync(company with { Metadata = metadata });

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == companyName);
        company.Metadata.Is(metadata);
    }
}