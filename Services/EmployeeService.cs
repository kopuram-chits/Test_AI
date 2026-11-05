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

    /// <summary>
    /// Stores a copy of the employee with a newly assigned ID.
    /// </summary>
    /// <param name="employee">The employee to copy; its ID is ignored and the input is unchanged.</param>
    /// <returns>An independent copy of the stored employee with its assigned ID.</returns>
    /// <exception cref="InvalidDepartmentException">The employee's department does not exist.</exception>
    /// <exception cref="InvalidDesignationException">The employee's designation does not exist.</exception>
    public Employee Add(Employee employee)
    {
        lock (_store.SyncRoot)
        {
            ValidateDepartment(employee.DeptId);
            ValidateDesignation(employee.DesignationId);
            var newEmployee = Clone(employee);
            newEmployee.Id = _store.NextEmployeeId++;
            _store.Employees.Add(newEmployee);
            return Clone(newEmployee);
        }
    }

    /// <summary>
    /// Updates an existing employee after validating both references, leaving stored data unchanged if validation fails.
    /// </summary>
    /// <param name="id">The stored employee ID to update.</param>
    /// <param name="employee">The replacement field values; its ID is ignored and the input is unchanged.</param>
    /// <returns>An independent copy of the updated employee, or <see langword="null"/> if the ID is not found.</returns>
    /// <remarks>A missing employee returns before reference validation. Validation and writes share the store lock.</remarks>
    /// <exception cref="InvalidDepartmentException">The employee exists but the specified department does not.</exception>
    /// <exception cref="InvalidDesignationException">The employee and department exist but the specified designation does not.</exception>
    public Employee? Update(int id, Employee employee)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Employees.FirstOrDefault(e => e.Id == id);
            if (existing is null)
                return null;

            ValidateDepartment(employee.DeptId);
            ValidateDesignation(employee.DesignationId);
            existing.DeptId = employee.DeptId;
            existing.DesignationId = employee.DesignationId;
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
            var index = _store.Employees.FindIndex(employee => employee.Id == id);
            if (index < 0)
                return false;

            _store.Employees.RemoveAt(index);
            return true;
        }
    }

    // Called only while the shared store lock is held.
    private void ValidateDepartment(int deptId)
    {
        if (!_store.Departments.Any(d => d.DeptId == deptId))
            throw new InvalidDepartmentException();
    }

    // Called only while the shared store lock is held.
    private void ValidateDesignation(int designationId)
    {
        if (!_store.Designations.Any(d => d.DesignationId == designationId))
            throw new InvalidDesignationException();
    }

    private static Employee Clone(Employee employee) => new()
    {
        Id = employee.Id,
        Name = employee.Name,
        Salary = employee.Salary,
        DOB = employee.DOB,
        DeptId = employee.DeptId,
        DesignationId = employee.DesignationId
    };
}
