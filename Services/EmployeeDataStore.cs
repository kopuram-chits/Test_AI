using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

// Both services share this singleton and lock so relationship checks and writes are atomic.
// Demo-only storage: all data is lost when the application stops.
public sealed class EmployeeDataStore
{
    internal object SyncRoot { get; } = new();
    internal List<Department> Departments { get; } =
    [
        new Department { DeptId = 1, DeptName = "Engineering" },
        new Department { DeptId = 2, DeptName = "Human Resources" }
    ];
    internal List<Employee> Employees { get; } =
    [
        new Employee { Id = 1, Name = "Rahul", Salary = 30000m, DOB = new DateTime(1995, 5, 10), DeptId = 1 },
        new Employee { Id = 2, Name = "Amit", Salary = 45000m, DOB = new DateTime(1992, 8, 15), DeptId = 1 },
        new Employee { Id = 3, Name = "Priya", Salary = 60000m, DOB = new DateTime(1994, 3, 20), DeptId = 2 },
        new Employee { Id = 4, Name = "Neha", Salary = 40000m, DOB = new DateTime(1996, 7, 12), DeptId = 2 }
    ];
    internal int NextEmployeeId = 5;
    internal int NextDepartmentId = 3;
}
