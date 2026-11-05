using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public class EmployeeService : IEmployeeService
{
    // Demo-only in-memory storage. Data is lost when the application stops.
    private readonly List<Employee> _employees =
    [
        new Employee { Id = 1, Name = "Rahul", Salary = 30000m, DOB = new DateTime(1995, 5, 10) },
        new Employee { Id = 2, Name = "Amit", Salary = 45000m, DOB = new DateTime(1992, 8, 15) }
    ];

    private int _nextId = 3;
    private readonly object _lock = new();

    public IReadOnlyList<Employee> GetAll()
    {
        lock (_lock)
        {
            return _employees.Select(Clone).ToList();
        }
    }

    public Employee? GetById(int id)
    {
        lock (_lock)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            return employee is null ? null : Clone(employee);
        }
    }

    public Employee Add(Employee employee)
    {
        lock (_lock)
        {
            var newEmployee = Clone(employee);
            newEmployee.Id = _nextId++;
            _employees.Add(newEmployee);
            return Clone(newEmployee);
        }
    }

    public Employee? Update(int id, Employee employee)
    {
        lock (_lock)
        {
            var existing = _employees.FirstOrDefault(e => e.Id == id);
            if (existing is null)
                return null;

            existing.Name = employee.Name;
            existing.Salary = employee.Salary;
            existing.DOB = employee.DOB;

            return Clone(existing);
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee is null)
                return false;

            _employees.Remove(employee);
            return true;
        }
    }

    private static Employee Clone(Employee employee) => new()
    {
        Id = employee.Id,
        Name = employee.Name,
        Salary = employee.Salary,
        DOB = employee.DOB
    };
}
