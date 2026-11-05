using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Xunit;

namespace EmployeeAPI.Tests;

public class EmployeeAddTests
{
    [Fact]
    public void Add_AssignsSequentialIdsAndPreservesInputAndFields()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var before = service.GetAll();
        var input = new Employee
        {
            DesignationId = 1,
            Id = -123,
            Name = "New employee",
            DeptId = 2,
            Salary = 12345.67m,
            DOB = new DateTime(1990, 2, 3)
        };

        var first = service.Add(input);
        var second = service.Add(input);

        Assert.Equal(-123, input.Id);
        Assert.NotSame(input, first);
        Assert.DoesNotContain(before, employee => employee.Id == first.Id);
        Assert.Equal(first.Id + 1, second.Id);
        Assert.Equal(before.Count + 2, service.GetAll().Count);
        foreach (var added in new[] { first, second })
        {
            Assert.Equal("New employee", added.Name);
            Assert.Equal(2, added.DeptId);
            Assert.Equal(1, added.DesignationId);
            Assert.Equal(12345.67m, added.Salary);
            Assert.Equal(new DateTime(1990, 2, 3), added.DOB);
            Assert.Equal(added.Id, service.GetById(added.Id)!.Id);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Add_MutatingInputOrResult_DoesNotChangeStoredEmployee(bool mutateInput)
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var input = new Employee
        {
            DesignationId = 1,
            Name = "Original",
            DeptId = 1,
            Salary = 500m,
            DOB = new DateTime(1990, 1, 1)
        };
        var result = service.Add(input);
        var assignedId = result.Id;
        var target = mutateInput ? input : result;

        target.Id = -1;
        target.Name = "Changed";
        target.DeptId = 2;
        target.DesignationId = 2;
        target.Salary = 0m;
        target.DOB = DateTime.MinValue;

        var stored = service.GetById(assignedId);
        Assert.NotNull(stored);
        Assert.Equal(assignedId, stored!.Id);
        Assert.Equal("Original", stored.Name);
        Assert.Equal(1, stored.DeptId);
        Assert.Equal(1, stored.DesignationId);
        Assert.Equal(500m, stored.Salary);
        Assert.Equal(new DateTime(1990, 1, 1), stored.DOB);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Add_UnknownDepartment_DoesNotStoreEmployeeOrConsumeId(int deptId)
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var first = service.Add(new Employee { DesignationId = 1, Name = "Before", DeptId = 1 });
        var count = service.GetAll().Count;
        var invalid = new Employee { DesignationId = 1, Id = -123, Name = "Rejected", DeptId = deptId };

        Assert.Throws<InvalidDepartmentException>(() => service.Add(invalid));

        Assert.Equal(count, service.GetAll().Count);
        Assert.Equal(-123, invalid.Id);
        Assert.DoesNotContain(service.GetAll(), employee => employee.Name == "Rejected");
        var next = service.Add(new Employee { DesignationId = 1, Name = "After", DeptId = 1 });
        Assert.Equal(first.Id + 1, next.Id);
    }
}
