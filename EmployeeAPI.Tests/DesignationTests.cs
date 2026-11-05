using System.ComponentModel.DataAnnotations;
using EmployeeAPI.Controllers;
using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EmployeeAPI.Tests;

public class DesignationTests
{
    [Fact]
    public void DesignationCrud_PreservesRouteIdAndReturnsCopies()
    {
        var service = new DesignationService(new EmployeeDataStore());
        var input = new Designation { DesignationId = 999, DesignationName = "Developer" };
        var created = service.Add(input);
        Assert.NotEqual(input.DesignationId, created.DesignationId);
        input.DesignationName = "Input changed";
        created.DesignationName = "Output changed";
        var fetched = service.GetById(created.DesignationId)!;
        Assert.Equal("Developer", fetched.DesignationName);
        fetched.DesignationName = "Fetched changed";
        var listed = service.GetAll().Single(d => d.DesignationId == created.DesignationId);
        Assert.Equal("Developer", listed.DesignationName);
        listed.DesignationName = "List changed";
        Assert.Equal("Developer", service.GetById(created.DesignationId)!.DesignationName);
        var updated = service.Update(created.DesignationId, new Designation { DesignationId = 999, DesignationName = "Senior Developer" })!;
        Assert.Equal(created.DesignationId, updated.DesignationId);
        updated.DesignationName = "Update result changed";
        Assert.Equal("Senior Developer", service.GetById(created.DesignationId)!.DesignationName);
        Assert.True(service.Delete(created.DesignationId));
        Assert.Null(service.GetById(created.DesignationId));
        Assert.Null(service.Update(created.DesignationId, input));
        Assert.False(service.Delete(created.DesignationId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void UnknownDesignation_ReturnsNotFoundResults(int id)
    {
        var controller = new DesignationController(new DesignationService(new EmployeeDataStore()));
        Assert.IsType<NotFoundObjectResult>(controller.GetById(id).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Update(id, new Designation { DesignationName = "Test" }).Result);
        Assert.IsType<NotFoundObjectResult>(controller.Delete(id));
    }

    [Fact]
    public void Controllers_ReturnExpectedCrudResponses()
    {
        var controller = new DesignationController(new DesignationService(new EmployeeDataStore()));
        Assert.IsType<OkObjectResult>(controller.GetAll().Result);
        var response = Assert.IsType<CreatedAtActionResult>(controller.Create(new Designation { DesignationName = "Test" }).Result);
        var created = Assert.IsType<Designation>(response.Value);
        Assert.Equal(nameof(DesignationController.GetById), response.ActionName);
        Assert.Equal(created.DesignationId, response.RouteValues!["id"]);
        Assert.IsType<OkObjectResult>(controller.GetById(created.DesignationId).Result);
        Assert.IsType<OkObjectResult>(controller.Update(created.DesignationId, new Designation { DesignationName = "Renamed" }).Result);
        Assert.IsType<NoContentResult>(controller.Delete(created.DesignationId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DesignationName_IsRequired(string? name)
    {
        var designation = new Designation { DesignationName = name! };
        Assert.False(Validator.TryValidateObject(designation, new ValidationContext(designation), new List<ValidationResult>(), true));
    }

    [Fact]
    public void DesignationName_MaximumLengthIs100()
    {
        var designation = new Designation { DesignationName = new string('a', 100) };
        Assert.True(Validator.TryValidateObject(designation, new ValidationContext(designation), new List<ValidationResult>(), true));
        designation.DesignationName += "a";
        Assert.False(Validator.TryValidateObject(designation, new ValidationContext(designation), new List<ValidationResult>(), true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EmployeeDesignationId_MustBePositive(int designationId)
    {
        var employee = new Employee { DeptId = 1, Name = "Test", DesignationId = designationId };
        Assert.False(Validator.TryValidateObject(employee, new ValidationContext(employee), new List<ValidationResult>(), true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void InvalidEmployeeDesignation_IsRejectedWithoutChangingStorage(int designationId)
    {
        var employees = new EmployeeService(new EmployeeDataStore());
        var before = employees.GetById(1)!;
        var count = employees.GetAll().Count;
        var invalid = new Employee { DeptId = 1, Name = "Changed", DesignationId = designationId };
        Assert.Throws<InvalidDesignationException>(() => employees.Add(invalid));
        Assert.Throws<InvalidDesignationException>(() => employees.Update(1, invalid));
        Assert.Equal(count, employees.GetAll().Count);
        Assert.Equal(before.Name, employees.GetById(1)!.Name);
        Assert.Equal(before.DesignationId, employees.GetById(1)!.DesignationId);
        var controller = new EmployeeController(employees);
        Assert.IsType<BadRequestObjectResult>(controller.Create(invalid).Result);
        Assert.IsType<BadRequestObjectResult>(controller.Update(1, invalid).Result);
    }

    [Fact]
    public void EmployeeDesignationUpdate_PersistsAndPreservesPreviousSnapshot()
    {
        var store = new EmployeeDataStore();
        var employees = new EmployeeService(store);
        var designations = new DesignationService(store);
        var created = designations.Add(new Designation { DesignationName = "New designation" });
        var snapshot = employees.GetById(1)!;
        var updated = employees.Update(1, new Employee { DeptId = 1, Name = snapshot.Name, DesignationId = created.DesignationId })!;
        Assert.Equal(created.DesignationId, updated.DesignationId);
        Assert.Equal(created.DesignationId, employees.GetAll().Single(e => e.Id == 1).DesignationId);
        Assert.Equal(1, snapshot.DesignationId);
        Assert.Throws<DesignationInUseException>(() => designations.Delete(created.DesignationId));
        Assert.IsType<ConflictObjectResult>(new DesignationController(designations).Delete(created.DesignationId));
        Assert.NotNull(designations.GetById(created.DesignationId));
        Assert.True(employees.Delete(1));
        Assert.True(designations.Delete(created.DesignationId));
    }

    [Fact]
    public async Task ConcurrentEmployeeCreationAndDesignationDeletion_CannotLeaveAnInvalidReference()
    {
        var store = new EmployeeDataStore();
        var employees = new EmployeeService(store);
        var designations = new DesignationService(store);
        // Exercise both possible serialized outcomes without assuming scheduler order.
        for (var i = 0; i < 50; i++)
        {
            var designation = designations.Add(new Designation { DesignationName = "Concurrent" });
            using var start = new ManualResetEventSlim();
            var add = Task.Run(() =>
            {
                start.Wait();
                try { employees.Add(new Employee { DeptId = 1, Name = "Concurrent", DesignationId = designation.DesignationId }); }
                catch (InvalidDesignationException) { }
            });
            var delete = Task.Run(() =>
            {
                start.Wait();
                try { designations.Delete(designation.DesignationId); }
                catch (DesignationInUseException) { }
            });
            start.Set();
            await Task.WhenAll(add, delete);
            if (employees.GetAll().Any(e => e.DesignationId == designation.DesignationId))
                Assert.NotNull(designations.GetById(designation.DesignationId));
            else
                Assert.Null(designations.GetById(designation.DesignationId));
        }
    }
}
