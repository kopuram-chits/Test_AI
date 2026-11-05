using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Xunit;

namespace EmployeeAPI.Tests;

public class EmployeeDeleteTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Delete_RemovesOnlySelectedEmployeeAndPreservesOrder(int index)
    {
        var service = new EmployeeService(new EmployeeDataStore());
        foreach (var employee in service.GetAll())
            service.Delete(employee.Id);
        var added = Enumerable.Range(0, 3)
            .Select(i => service.Add(new Employee { Name = $"Employee {i}", DeptId = 1, DesignationId = 1 }))
            .ToList();
        var id = added[index].Id;

        Assert.True(service.Delete(id));

        Assert.Equal(added.Where(e => e.Id != id).Select(e => e.Id), service.GetAll().Select(e => e.Id));
        Assert.False(service.Delete(id));
    }

    [Fact]
    public void Delete_MissingEmployee_LeavesStorageUnchanged()
    {
        var service = new EmployeeService(new EmployeeDataStore());
        var ids = service.GetAll().Select(e => e.Id).ToArray();

        Assert.False(service.Delete(int.MaxValue));

        Assert.Equal(ids, service.GetAll().Select(e => e.Id));
    }
}
