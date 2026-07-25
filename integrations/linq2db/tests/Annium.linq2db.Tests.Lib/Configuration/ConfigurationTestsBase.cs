using System.Linq;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Testing;
using Xunit;

namespace Annium.linq2db.Tests.Lib.Configuration;

/// <summary>
/// Base class for testing linq2db configuration and database metadata validation
/// </summary>
public class ConfigurationTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ConfigurationTestsBase class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    protected ConfigurationTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<LibServicePack>();
    }

    /// <summary>
    /// Base test method to validate that database metadata is correctly configured with expected number of tables
    /// </summary>
    protected void Metadata_IsValid_Base()
    {
        // arrange
        using var conn = Get<Connection>();
        var databaseMetadata = conn.MappingSchema.Describe();

        // assert — three core tables plus the no-primary-key fixture (Update_NoPrimaryKey_Throws),
        // the created-only fixture (Insert_CreatedOnly), and the manual-timestamp fixture
        // (Insert_ManualTime); assert identity and key structure, not just the count
        databaseMetadata.Tables.Has(6);

        var companies = databaseMetadata.Tables[typeof(Company)];
        companies.Name.Is("companies");
        companies.Columns.Values.Count(c => c.PrimaryKey is not null).Is(1);

        var employees = databaseMetadata.Tables[typeof(Employee)];
        employees.Name.Is("employees");
        employees.Columns.Values.Count(c => c.PrimaryKey is not null).Is(1);

        // CompanyEmployee has a composite primary key (CompanyId + EmployeeId)
        var companyEmployees = databaseMetadata.Tables[typeof(CompanyEmployee)];
        companyEmployees.Name.Is("company_employees");
        companyEmployees.Columns.Values.Count(c => c.PrimaryKey is not null).Is(2);
    }
}
