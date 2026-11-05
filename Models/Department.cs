using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Models;

public class Department
{
    public int DeptId { get; set; }

    [Required]
    [StringLength(100)]
    public string DeptName { get; set; } = string.Empty;
}
