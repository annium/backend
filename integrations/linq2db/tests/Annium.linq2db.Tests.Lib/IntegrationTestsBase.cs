using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Logging;
using Annium.Testing;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Xunit;

namespace Annium.linq2db.Tests.Lib;

/// <summary>
/// Base class providing integration tests for database operations and data access functionality.
/// </summary>
public class IntegrationTestsBase : TestBase
{
    protected IntegrationTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<LibServicePack>();
    }

    /// <summary>
    /// Tests complete end-to-end database operations including entity creation, relationships, and complex queries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task EndToEnd_Base()
    {
        // arrange
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var companyName = $"demo:{Guid.NewGuid()}";
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(companyName, metadata);
        var chief = new Employee("A", null);
        var companyChief = new CompanyEmployee(company, chief, "chief");
        var worker = new Employee("B", chief);
        var companyWorker = new CompanyEmployee(company, worker, "worker");

        // act
        await conn.Companies.InsertAsync(company);
        await conn.Employees.InsertAsync(chief);
        await conn.CompanyEmployees.InsertAsync(companyChief);
        await conn.Employees.InsertAsync(worker);
        await conn.CompanyEmployees.InsertAsync(companyWorker);

        // assert
        company = await conn
            .Companies.LoadWith(x => x.Employees)
            .ThenLoad(x => x.Company)
            .LoadWith(x => x.Employees)
            .ThenLoad(x => x.Employee.Chief)
            .LoadWith(x => x.Employees)
            .ThenLoad(x => x.Employee.Subordinates)
            .AsQueryable()
            .SingleAsync(x => x.Name == companyName);
        company.Name.Is(companyName);
        // company.CreatedAt.Is(createdAt);
        company.Metadata.Is(metadata);
        company.Employees.Has(2);
        // chief
        var chiefEmployee = company.Employees.Single(x => x.EmployeeId == chief.Id);
        chiefEmployee.CompanyId.Is(company.Id);
        chiefEmployee.Company.IsNotDefault();
        chiefEmployee.EmployeeId.Is(chief.Id);
        chiefEmployee.Employee.IsNotDefault();
        chiefEmployee.Employee.ChiefId.IsDefault();
        chiefEmployee.Employee.Chief.IsDefault();
        chiefEmployee.Employee.Subordinates.Has(1);
        chiefEmployee.Employee.Subordinates.At(0).Id.Is(worker.Id);
        // worker
        var workerEmployee = company.Employees.Single(x => x.EmployeeId == worker.Id);
        workerEmployee.CompanyId.Is(company.Id);
        workerEmployee.Company.IsNotDefault();
        workerEmployee.EmployeeId.Is(worker.Id);
        workerEmployee.Employee.IsNotDefault();
        workerEmployee.Employee.ChiefId.Is(chief.Id);
        workerEmployee.Employee.Chief.IsNotDefault();
        workerEmployee.Employee.Subordinates.IsEmpty();
    }

    /// <summary>
    /// Tests high-load concurrent insert operations to validate database performance and thread safety.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task HighLoad_Insert_Base()
    {
        this.Trace("start");

        var chunksCount = 80;
        var chunkSize = 1000;

        await Task.WhenAll(
            Enumerable
                .Range(0, chunksCount)
                .Select(async id =>
                {
                    this.Trace("{id} - start", id);

                    this.Trace("{id} - generate rows", id);
                    var companies = Enumerable
                        .Range(0, chunkSize)
                        .Select(_ =>
                        {
                            var companyName = $"demo:{Guid.NewGuid()}";
                            var metadata = new CompanyMetadata($"somewhere for {companyName}");

                            return new Company(companyName, metadata);
                        })
                        .ToArray();

                    this.Trace("{id} - create connection", id);
                    await using var scope = Provider.GetConnectionScope<Connection>();
                    scope.ThrowIfDisposed();
                    var conn = scope.Cn;

                    this.Trace("{id} - bulk copy", id);
                    var result = await conn.BulkCopyAsync(new BulkCopyOptions { KeepIdentity = true }, companies);

                    this.Trace("{id} - verify", id);
                    result.RowsCopied.Is(chunkSize);

                    this.Trace("{id} - done", id);
                })
        );

        this.Trace("done");
    }

    /// <summary>
    /// Tests high-load concurrent select operations to validate database performance and connection handling.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task HighLoad_Select_Base()
    {
        // arrange
        var companyName = $"demo:{Guid.NewGuid()}";
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(companyName, metadata);

        await using (var scope = Provider.GetConnectionScope<Connection>())
        {
            scope.ThrowIfDisposed();
            var conn = scope.Cn;
            await conn.Companies.InsertAsync(company);
        }

        // assert
        await Task.WhenAll(
            Enumerable
                .Range(0, 50)
                .Select(async _ =>
                {
                    await using var scope = Provider.GetConnectionScope<Connection>();
                    scope.ThrowIfDisposed();
                    var conn = scope.Cn;
                    var loadedCompany = await conn
                        .Companies.LoadWith(x => x.Employees)
                        .ThenLoad(x => x.Company)
                        .LoadWith(x => x.Employees)
                        .ThenLoad(x => x.Employee.Chief)
                        .LoadWith(x => x.Employees)
                        .ThenLoad(x => x.Employee.Subordinates)
                        .AsQueryable()
                        .SingleAsync(x => x.Name == companyName);

                    loadedCompany.Id.Is(company.Id);
                    loadedCompany.Name.Is(company.Name);
                    loadedCompany.Metadata.Is(metadata);
                })
        );
    }
}
