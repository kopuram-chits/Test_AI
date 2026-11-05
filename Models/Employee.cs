using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    public DateTime DOB { get; set; }

    [Range(1, int.MaxValue)]
    public int DeptId { get; set; }
}
