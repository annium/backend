using System;
using System.Threading.Tasks;
using Annium.Core.Runtime.Time;
using Annium.linq2db.Extensions.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Testing;
using Annium.Testing.Collection;
using LinqToDB;
using NodaTime;
using Xunit;

namespace Annium.linq2db.Tests.Lib.Extensions;

public class TableInsertOrUpdateExtensionsTestsBase : TestBase
{
    protected TableInsertOrUpdateExtensionsTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<LibServicePack>();
    }

    protected async Task Insert_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var now = timeManager.Now;
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(Name(), metadata);
        var chief = new Employee(Name(), null);
        var companyChief = new CompanyEmployee(company, chief, "chief");
        var worker = new Employee(Name(), chief);
        var companyWorker = new CompanyEmployee(company, worker, "worker");

        // act
        timeManager.AddSecond();
        await conn.Companies.InsertAsync(company);
        await conn.Employees.InsertAsync(chief);
        await conn.CompanyEmployees.InsertAsync(companyChief);
        await conn.Employees.InsertAsync(worker);
        await conn.CompanyEmployees.InsertAsync(companyWorker);

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == company.Name);
        company.CreatedAt.IsGreater(now).IsLess(now + Duration.FromSeconds(2));
        company.UpdatedAt.Is(company.CreatedAt);
        company.Metadata.Is(metadata);
    }

    protected async Task Update_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(Name(), metadata);
        await conn.Companies.InsertAsync(company);
        company = await conn.Companies.SingleAsync(x => x.Id == company.Id);
        var companyCreatedAt = company.CreatedAt;
        var companyUpdatedAt = company.UpdatedAt;
        var chief = new Employee(Name(), null);
        await conn.Employees.InsertAsync(chief);
        var companyChief = new CompanyEmployee(company, chief, "chief");
        await conn.CompanyEmployees.InsertAsync(companyChief);
        var worker = new Employee(Name(), chief);
        await conn.Employees.InsertAsync(worker);
        var companyWorker = new CompanyEmployee(company, worker, "worker");
        await conn.CompanyEmployees.InsertAsync(companyWorker);

        // act
        metadata = new CompanyMetadata("outdoors");
        company.SetMetadata(metadata);
        timeManager.AddSecond();
        await conn.Companies.UpdateAsync(company);

        worker.SetChief(null);
        await conn.Employees.UpdateAsync(worker);

        // assert
        company = await conn.Companies.SingleAsync(x => x.Name == company.Name);
        company.CreatedAt.Is(companyCreatedAt);
        company.UpdatedAt.Is(companyUpdatedAt + Duration.FromSeconds(1));
        company.Metadata.Is(metadata);
        worker = await conn.Employees.LoadWith(x => x.Chief).SingleAsync(x => x.Name == worker.Name);
        worker.ChiefId.IsDefault();
        worker.Chief.IsDefault();

        // act
        var name = company.Name;
        companyUpdatedAt = company.UpdatedAt;
        timeManager.AddSecond();
        await conn.Companies.Set(x => x.Name, x => x.Name + " Main").UpdateAsync();

        // assert
        company = await conn.Companies.SingleAsync(x => x.Id == company.Id);
        company.Name.Is(name + " Main");
        company.UpdatedAt.Is(companyUpdatedAt + Duration.FromSeconds(1));
    }

    protected async Task InsertOrUpdate_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var metadata = new CompanyMetadata("somewhere");
        var company = new Company(Name(), metadata);
        var chief = new Employee(Name(), null);
        var companyChief = new CompanyEmployee(company, chief, "chief");

        // act
        await conn.Companies.InsertOrUpdateAsync(company);
        await conn.Employees.InsertOrUpdateAsync(chief);
        await conn.CompanyEmployees.InsertOrUpdateAsync(companyChief);

        // assert
        company = await conn.Companies.LoadWith(x => x.Employees).SingleAsync(x => x.Name == company.Name);
        var companyCreatedAt = company.CreatedAt;
        var companyUpdatedAt = company.UpdatedAt;
        company.Metadata.Is(metadata);
        company.Employees.Has(1);
        company.Employees.At(0).Role.Is("chief");

        // act
        metadata = new CompanyMetadata("outdoors");
        company.SetMetadata(metadata);
        timeManager.AddSecond();
        await conn.Companies.InsertOrUpdateAsync(company);
        companyChief.SetRole("main chief");
        await conn.CompanyEmployees.InsertOrUpdateAsync(companyChief);

        // assert
        company = await conn.Companies.LoadWith(x => x.Employees).SingleAsync(x => x.Name == company.Name);
        company.CreatedAt.Is(companyCreatedAt);
        company.UpdatedAt.Is(companyUpdatedAt + Duration.FromSeconds(1));
        company.Metadata.Is(metadata);
        company.Employees.Has(1);
        company.Employees.At(0).Role.Is("main chief");
    }

    private static string Name() => Guid.NewGuid().ToString();
}
