using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService) => _departmentService = departmentService;

    [HttpGet]
    public ActionResult<IReadOnlyList<Department>> GetAll() => Ok(_departmentService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Department> GetById(int id)
    {
        var department = _departmentService.GetById(id);
        return department is null ? NotFound(new { message = "Department not found." }) : Ok(department);
    }

    [HttpPost]
    public ActionResult<Department> Create([FromBody] Department department)
    {
        var created = _departmentService.Add(department);
        return CreatedAtAction(nameof(GetById), new { id = created.DeptId }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Department> Update(int id, [FromBody] Department department)
    {
        var updated = _departmentService.Update(id, department);
        return updated is null ? NotFound(new { message = "Department not found." }) : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            return _departmentService.Delete(id)
                ? NoContent()
                : NotFound(new { message = "Department not found." });
        }
        catch (DepartmentInUseException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
