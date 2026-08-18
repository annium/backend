using System;
using System.Threading.Tasks;
using Annium.Core.Runtime.Time;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Testing;
using LinqToDB;
using LinqToDB.Async;
using NodaTime;
using Xunit;

namespace Annium.linq2db.Tests.Lib.Extensions;

/// <summary>
/// Base class providing shared test methods for table insert or update extensions functionality.
/// </summary>
public class TableInsertOrUpdateExtensionsTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableInsertOrUpdateExtensionsTestsBase"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    protected TableInsertOrUpdateExtensionsTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<LibServicePack>();
    }

    /// <summary>
    /// Tests basic insert functionality with automatic timestamp tracking.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests update functionality with automatic timestamp tracking and field modifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests insert or update functionality with automatic timestamp tracking.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that inserting a created-only entity (ICreatedTimeEntity, no UpdatedAt) auto-stamps
    /// CreatedAt, exercising the created-only branch of the insert time-query processing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Insert_CreatedOnly_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var now = timeManager.Now;
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var entity = new CreatedOnlyEntity("content");

        // act
        timeManager.AddSecond();
        await conn.GetTable<CreatedOnlyEntity>().InsertAsync(entity);

        // assert — CreatedAt auto-stamped; there is no UpdatedAt to manage
        var loaded = await conn.GetTable<CreatedOnlyEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("content");
        loaded.CreatedAt.IsGreater(now).IsLess(now + Duration.FromSeconds(2));
    }

    /// <summary>
    /// Tests that upserting a created-only entity stamps CreatedAt on insert and leaves it unchanged
    /// on update, exercising the created-only branch of the insert-or-update time-query processing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task InsertOrUpdate_CreatedOnly_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var entity = new CreatedOnlyEntity("content");

        // act — insert via upsert
        await conn.GetTable<CreatedOnlyEntity>().InsertOrUpdateAsync(entity);

        // assert
        var loaded = await conn.GetTable<CreatedOnlyEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("content");
        var createdAt = loaded.CreatedAt;

        // act — update via upsert; CreatedAt is ignored on the update path, content changes
        entity.SetContent("updated");
        timeManager.AddSecond();
        await conn.GetTable<CreatedOnlyEntity>().InsertOrUpdateAsync(entity);

        // assert — content updated, CreatedAt unchanged
        loaded = await conn.GetTable<CreatedOnlyEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("updated");
        loaded.CreatedAt.Is(createdAt);
    }

    /// <summary>
    /// Tests that inserting and updating a manual-timestamp entity (ConfigureManualCreatedUpdatedTime)
    /// through a Connection whose ProcessQuery routes into the auto-timestamp pipeline leaves the
    /// application-supplied CreatedAt/UpdatedAt untouched (the auto pipeline must skip manual columns).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Insert_ManualTime_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;

        var appCreated = Instant.FromUtc(2020, 1, 1, 0, 0, 0);
        var appUpdated = Instant.FromUtc(2021, 1, 1, 0, 0, 0);
        var entity = new ManualTimeEntity("content");
        entity.SetCreatedAt(appCreated);
        entity.SetUpdatedAt(appUpdated);

        // act — insert through the auto-ProcessQuery Connection; the manual columns must be left alone
        await conn.GetTable<ManualTimeEntity>().InsertAsync(entity);

        // assert — application-supplied timestamps preserved, not overwritten by the auto pipeline
        var loaded = await conn.GetTable<ManualTimeEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("content");
        loaded.CreatedAt.Is(appCreated);
        loaded.UpdatedAt.Is(appUpdated);

        // act — update through the same pipeline; manual timestamps still left to the application
        var appUpdated2 = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
        entity.SetContent("updated");
        entity.SetUpdatedAt(appUpdated2);
        await conn.GetTable<ManualTimeEntity>().UpdateAsync(entity);

        // assert
        loaded = await conn.GetTable<ManualTimeEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("updated");
        loaded.CreatedAt.Is(appCreated);
        loaded.UpdatedAt.Is(appUpdated2);

        // act — upsert (insert-or-update) through the same pipeline; manual timestamps still untouched
        var appUpdated3 = Instant.FromUtc(2023, 1, 1, 0, 0, 0);
        entity.SetContent("upserted");
        entity.SetUpdatedAt(appUpdated3);
        await conn.GetTable<ManualTimeEntity>().InsertOrUpdateAsync(entity);

        // assert
        loaded = await conn.GetTable<ManualTimeEntity>().SingleAsync(x => x.Id == entity.Id);
        loaded.Content.Is("upserted");
        loaded.CreatedAt.Is(appCreated);
        loaded.UpdatedAt.Is(appUpdated3);
    }

    /// <summary>
    /// Tests that updating an entity with a composite primary key targets only the row matching
    /// every key column, exercising the AndAlso-aggregation branch of the predicate builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Update_CompositeKey_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var company = new Company(Name(), new CompanyMetadata("somewhere"));
        var emp1 = new Employee(Name(), null);
        var emp2 = new Employee(Name(), null);
        await conn.Companies.InsertAsync(company);
        await conn.Employees.InsertAsync(emp1);
        await conn.Employees.InsertAsync(emp2);
        // both rows share CompanyId and differ only by EmployeeId, so a predicate that ignored
        // EmployeeId would wrongly update ce2 as well
        var ce1 = new CompanyEmployee(company, emp1, "role-1");
        var ce2 = new CompanyEmployee(company, emp2, "role-2");
        await conn.CompanyEmployees.InsertAsync(ce1);
        await conn.CompanyEmployees.InsertAsync(ce2);

        // act
        ce1.SetRole("role-1-updated");
        await conn.CompanyEmployees.UpdateAsync(ce1);

        // assert — only the composite-key-matched row changed
        var reloaded1 = await conn.CompanyEmployees.SingleAsync(x =>
            x.CompanyId == company.Id && x.EmployeeId == emp1.Id
        );
        reloaded1.Role.Is("role-1-updated");
        var reloaded2 = await conn.CompanyEmployees.SingleAsync(x =>
            x.CompanyId == company.Id && x.EmployeeId == emp2.Id
        );
        reloaded2.Role.Is("role-2");
    }

    /// <summary>
    /// Tests that updating an entity whose table has no primary key throws, pinning the
    /// empty-sequence behavior of the primary-key predicate builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Update_NoPrimaryKey_Throws_Base()
    {
        // arrange
        await using var scope = Provider.GetConnectionScope<Connection>();
        scope.ThrowIfDisposed();
        var conn = scope.Cn;
        var entity = new NoPkEntity("value");

        // act + assert — the predicate builder aggregates over zero primary-key columns
        await Wrap.It(async () => await conn.GetTable<NoPkEntity>().UpdateAsync(entity))
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Generates a unique name for test entities.
    /// </summary>
    /// <returns>A unique string identifier.</returns>
    private static string Name() => Guid.NewGuid().ToString();
}
