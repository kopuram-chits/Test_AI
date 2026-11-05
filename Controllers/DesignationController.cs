using EmployeeAPI.Models;
using EmployeeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesignationController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationController(IDesignationService designationService) => _designationService = designationService;

    [HttpGet]
    public ActionResult<IReadOnlyList<Designation>> GetAll() => Ok(_designationService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Designation> GetById(int id)
    {
        var designation = _designationService.GetById(id);
        return designation is null ? NotFound(new { message = "Designation not found." }) : Ok(designation);
    }

    [HttpPost]
    public ActionResult<Designation> Create([FromBody] Designation designation)
    {
        var created = _designationService.Add(designation);
        return CreatedAtAction(nameof(GetById), new { id = created.DesignationId }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Designation> Update(int id, [FromBody] Designation designation)
    {
        var updated = _designationService.Update(id, designation);
        return updated is null ? NotFound(new { message = "Designation not found." }) : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            return _designationService.Delete(id)
                ? NoContent()
                : NotFound(new { message = "Designation not found." });
        }
        catch (DesignationInUseException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
