using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.linq2db.Extensions.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Testing;
using Annium.Testing.Lib;
using LinqToDB;

namespace Annium.linq2db.Tests.Lib;

public class IntegrationTestsBase : TestBase
{
    public IntegrationTestsBase()
    {
        AddServicePack<LibServicePack>();
    }

    public async Task EndToEnd_Base()
    {
        // arrange
        await using var conn = Get<Connection>();
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
        company = await conn.Companies
            .LoadWith(x => x.Employees).ThenLoad(x => x.Company)
            .LoadWith(x => x.Employees).ThenLoad(x => x.Employee.Chief)
            .LoadWith(x => x.Employees).ThenLoad(x => x.Employee.Subordinates)
            .SingleAsync(x => x.Name == companyName);
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
}