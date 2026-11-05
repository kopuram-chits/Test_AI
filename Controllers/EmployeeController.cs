using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        int j = 90;
        _employeeService = employeeService;
    }

    // GET: api/Employee
    [HttpGet]
    public ActionResult<IReadOnlyList<Employee>> GetAll()
    {
        int j = 900;
        return Ok(_employeeService.GetAll());
    }

    // GET: api/Employee/1
    [HttpGet("{id:int}")]
    public ActionResult<Employee> GetById(int id)
    {
        var employee = _employeeService.GetById(id);
        return employee is null ? NotFound(new { message = "Employee not found." }) : Ok(employee);
    }

    // POST: api/Employee
    [HttpPost]
    public ActionResult<Employee> Create([FromBody] Employee employee)
    {
        try
        {
            var created = _employeeService.Add(employee);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidDesignationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidDepartmentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/Employee/1
    [HttpPut("{id:int}")]
    public ActionResult<Employee> Update(int id, [FromBody] Employee employee)
    {
        try
        {
            var updated = _employeeService.Update(id, employee);
            return updated is null ? NotFound(new { message = "Employee not found." }) : Ok(updated);
        }
        catch (InvalidDesignationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidDepartmentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/Employee/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return _employeeService.Delete(id)
            ? NoContent()
            : NotFound(new { message = "Employee not found." });
    }
}
