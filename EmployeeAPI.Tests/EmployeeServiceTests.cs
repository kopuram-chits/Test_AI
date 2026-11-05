using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Xunit;

namespace EmployeeAPI.Tests;

public class EmployeeServiceTests
{
    [Fact]
    public void GetById_ExistingEmployee_ReturnsAllFields()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var input = new Employee
        {
            DeptId = 1,
            Name = "Test Employee",
            Salary = 12345.67m,
            DOB = new DateTime(1990, 2, 3)
        };
        var added = service.Add(input);

        var result = service.GetById(added.Id);

        Assert.NotNull(result);
        Assert.Equal(added.Id, result!.Id);
        Assert.Equal(input.DeptId, result.DeptId);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Salary, result.Salary);
        Assert.Equal(input.DOB, result.DOB);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void GetById_UnknownId_ReturnsNull(int id)
    {
        var service = new EmployeeService(new EmployeeDataStore());

        Assert.Null(service.GetById(id));
    }

    [Fact]
    public void GetById_MutatingResult_DoesNotChangeStoredEmployee()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var added = service.Add(new Employee
        {
            DeptId = 1,
            Name = "Original",
            Salary = 1500m,
            DOB = new DateTime(1985, 6, 7)
        });
        var result = service.GetById(added.Id)!;

        result.DeptId = 2;
        result.Id = -1;
        result.Name = "Changed";
        result.Salary = 0m;
        result.DOB = DateTime.MinValue;
        var subsequent = service.GetById(added.Id);

        Assert.NotNull(subsequent);
        Assert.NotSame(result, subsequent);
        Assert.Equal(added.Id, subsequent!.Id);
        Assert.Equal(added.DeptId, subsequent.DeptId);
        Assert.Equal(added.Name, subsequent.Name);
        Assert.Equal(added.Salary, subsequent.Salary);
        Assert.Equal(added.DOB, subsequent.DOB);
    }

    [Fact]
    public void GetById_AfterUpdate_PreviousResultRemainsASnapshot()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var added = service.Add(new Employee
        {
            DeptId = 1,
            Name = "Before",
            Salary = 100m,
            DOB = new DateTime(1991, 1, 1)
        });
        var snapshot = service.GetById(added.Id)!;
        var replacement = new Employee
        {
            DeptId = 1,
            Name = "After",
            Salary = 200m,
            DOB = new DateTime(1992, 2, 2)
        };

        service.Update(added.Id, replacement);
        var current = service.GetById(added.Id)!;

        Assert.Equal(added.Name, snapshot.Name);
        Assert.Equal(added.Salary, snapshot.Salary);
        Assert.Equal(added.DOB, snapshot.DOB);
        Assert.Equal(replacement.Name, current.Name);
        Assert.Equal(replacement.Salary, current.Salary);
        Assert.Equal(replacement.DOB, current.DOB);
    }

    [Fact]
    public void GetById_DeletedEmployee_ReturnsNull()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var added = service.Add(new Employee { DeptId = 1, Name = "Temporary" });

        Assert.True(service.Delete(added.Id));

        Assert.Null(service.GetById(added.Id));
    }
}
