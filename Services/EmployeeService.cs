using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeDataStore _store;

    public EmployeeService(EmployeeDataStore store) => _store = store;

    public IReadOnlyList<Employee> GetAll()
    {
        lock (_store.SyncRoot)
        {
            return _store.Employees.Select(Clone).ToList();
        }
    }

    /// <summary>
    /// Retrieves a copy of the employee with the specified ID.
    /// </summary>
    /// <param name="id">The employee ID to find.</param>
    /// <returns>
    /// An independent copy of the employee, or <see langword="null"/> if not found.
    /// Changes to the returned object do not affect stored data.
    /// </returns>
    public Employee? GetById(int id)
    {
        lock (_store.SyncRoot)
        {
            var employee = _store.Employees.FirstOrDefault(e => e.Id == id);
            return employee is null ? null : Clone(employee);
        }
    }

    public Employee Add(Employee employee)
    {
        lock (_store.SyncRoot)
        {
            ValidateDepartment(employee.DeptId);
            var newEmployee = Clone(employee);
            newEmployee.Id = _store.NextEmployeeId++;
            _store.Employees.Add(newEmployee);
            return Clone(newEmployee);
        }
    }

    public Employee? Update(int id, Employee employee)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Employees.FirstOrDefault(e => e.Id == id);
            if (existing is null)
                return null;

            ValidateDepartment(employee.DeptId);
            existing.DeptId = employee.DeptId;
            existing.Name = employee.Name;
            existing.Salary = employee.Salary;
            existing.DOB = employee.DOB;

            return Clone(existing);
        }
    }

    public bool Delete(int id)
    {
        lock (_store.SyncRoot)
        {
            var employee = _store.Employees.FirstOrDefault(e => e.Id == id);
            if (employee is null)
                return false;

            _store.Employees.Remove(employee);
            return true;
        }
    }

    // Called only while the shared store lock is held.
    private void ValidateDepartment(int deptId)
    {
        if (!_store.Departments.Any(d => d.DeptId == deptId))
            throw new InvalidDepartmentException();
    }

    private static Employee Clone(Employee employee) => new()
    {
        Id = employee.Id,
        Name = employee.Name,
        Salary = employee.Salary,
        DOB = employee.DOB,
        DeptId = employee.DeptId
    };
}
