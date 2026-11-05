using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Models;

public class Designation
{
    public int DesignationId { get; set; }

    [Required]
    [StringLength(100)]
    public string DesignationName { get; set; } = string.Empty;
}
