using System.ComponentModel.DataAnnotations;
using EmployeeAPI.Controllers;
using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EmployeeAPI.Tests;

public class DepartmentTests
{
    [Fact]
    public void DepartmentCrud_PreservesRouteIdAndReturnsCopies()
    {
        var service = new DepartmentService(new EmployeeDataStore());
        var input = new Department { DeptId = 999, DeptName = "Finance" };
        var created = service.Add(input);
        Assert.NotEqual(input.DeptId, created.DeptId);
        input.DeptName = "Input changed";
        created.DeptName = "Output changed";
        var fetched = service.GetById(created.DeptId)!;
        Assert.Equal("Finance", fetched.DeptName);
        fetched.DeptName = "Fetched changed";
        var listed = service.GetAll().Single(d => d.DeptId == created.DeptId);
        Assert.Equal("Finance", listed.DeptName);
        listed.DeptName = "List changed";
        Assert.Equal("Finance", service.GetById(created.DeptId)!.DeptName);
        var updated = service.Update(created.DeptId, new Department { DeptId = 999, DeptName = "Accounts" })!;
        Assert.Equal(created.DeptId, updated.DeptId);
        updated.DeptName = "Update result changed";
        Assert.Equal("Accounts", service.GetById(created.DeptId)!.DeptName);
        Assert.True(service.Delete(created.DeptId));
        Assert.Null(service.GetById(created.DeptId));
        Assert.Null(service.Update(created.DeptId, input));
        Assert.False(service.Delete(created.DeptId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void UnknownDepartment_ReturnsNotFoundResults(int id)
    {
        var controller = new DepartmentController(new DepartmentService(new EmployeeDataStore()));
        Assert.IsType<NotFoundObjectResult>(controller.GetById(id).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Update(id, new Department { DeptName = "Test" }).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Delete(id));
    }

    [Fact]
    public void Controllers_ReturnExpectedCrudResponses()
    {
        var controller = new DepartmentController(new DepartmentService(new EmployeeDataStore()));
        Assert.IsType<OkObjectResult>(controller.GetAll().Result);
        var response = Assert.IsType<CreatedAtActionResult>(controller.Create(new Department { DeptName = "Test" }).Result);
        var created = Assert.IsType<Department>(response.Value);
        Assert.Equal(nameof(DepartmentController.GetById), response.ActionName);
        Assert.Equal(created.DeptId, response.RouteValues!["id"]);
        Assert.IsType<OkObjectResult>(controller.GetById(created.DeptId).Result);
        Assert.IsType<OkObjectResult>(controller.Update(created.DeptId, new Department { DeptName = "Renamed" }).Result);
        Assert.IsType<NoContentResult>(controller.Delete(created.DeptId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DepartmentName_IsRequired(string? name)
    {
        var department = new Department { DeptName = name! };
        Assert.False(Validator.TryValidateObject(department, new ValidationContext(department), new List<ValidationResult>(), true));
    }

    [Fact]
    public void DepartmentName_MaximumLengthIs100()
    {
        var department = new Department { DeptName = new string('a', 100) };
        Assert.True(Validator.TryValidateObject(department, new ValidationContext(department), new List<ValidationResult>(), true));
        department.DeptName += "a";
        Assert.False(Validator.TryValidateObject(department, new ValidationContext(department), new List<ValidationResult>(), true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EmployeeDepartmentId_MustBePositive(int deptId)
    {
        var employee = new Employee { Name = "Test", DeptId = deptId };
        Assert.False(Validator.TryValidateObject(employee, new ValidationContext(employee), new List<ValidationResult>(), true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void InvalidEmployeeDepartment_IsRejectedWithoutChangingStorage(int deptId)
    {
        var employees = new EmployeeService(new EmployeeDataStore());
        var before = employees.GetById(1)!;
        var count = employees.GetAll().Count;
        var invalid = new Employee { Name = "Changed", DeptId = deptId };
        Assert.Throws<InvalidDepartmentException>(() => employees.Add(invalid));
        Assert.Throws<InvalidDepartmentException>(() => employees.Update(1, invalid));
        Assert.Equal(count, employees.GetAll().Count);
        Assert.Equal(before.Name, employees.GetById(1)!.Name);
        Assert.Equal(before.DeptId, employees.GetById(1)!.DeptId);
        var controller = new EmployeeController(employees);
        Assert.IsType<BadRequestObjectResult>(controller.Create(invalid).Result);
        Assert.IsType<BadRequestObjectResult>(controller.Update(1, invalid).Result);
    }

    [Fact]
    public void EmployeeDepartmentUpdate_PersistsAndPreservesPreviousSnapshot()
    {
        var store = new EmployeeDataStore();
        var employees = new EmployeeService(store);
        var departments = new DepartmentService(store);
        var created = departments.Add(new Department { DeptName = "New department" });
        var snapshot = employees.GetById(1)!;
        var updated = employees.Update(1, new Employee { Name = snapshot.Name, DeptId = created.DeptId })!;
        Assert.Equal(created.DeptId, updated.DeptId);
        Assert.Equal(created.DeptId, employees.GetAll().Single(e => e.Id == 1).DeptId);
        Assert.Equal(1, snapshot.DeptId);
        Assert.Throws<DepartmentInUseException>(() => departments.Delete(created.DeptId));
        Assert.IsType<ConflictObjectResult>(new DepartmentController(departments).Delete(created.DeptId));
        Assert.NotNull(departments.GetById(created.DeptId));
        Assert.True(employees.Delete(1));
        Assert.True(departments.Delete(created.DeptId));
    }

    [Fact]
    public async Task ConcurrentEmployeeCreationAndDepartmentDeletion_CannotLeaveAnInvalidReference()
    {
        var store = new EmployeeDataStore();
        var employees = new EmployeeService(store);
        var departments = new DepartmentService(store);
        // Exercise both possible serialized outcomes without assuming scheduler order.
        for (var i = 0; i < 50; i++)
        {
            var department = departments.Add(new Department { DeptName = "Concurrent" });
            using var start = new ManualResetEventSlim();
            var add = Task.Run(() =>
            {
                start.Wait();
                try { employees.Add(new Employee { Name = "Concurrent", DeptId = department.DeptId }); }
                catch (InvalidDepartmentException) { }
            });
            var delete = Task.Run(() =>
            {
                start.Wait();
                try { departments.Delete(department.DeptId); }
                catch (DepartmentInUseException) { }
            });
            start.Set();
            await Task.WhenAll(add, delete);
            if (employees.GetAll().Any(e => e.DeptId == department.DeptId))
                Assert.NotNull(departments.GetById(department.DeptId));
            else
                Assert.Null(departments.GetById(department.DeptId));
        }
    }
}
