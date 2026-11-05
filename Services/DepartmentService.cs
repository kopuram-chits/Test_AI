using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public class DepartmentService : IDepartmentService
{
    private readonly EmployeeDataStore _store;

    public DepartmentService(EmployeeDataStore store) => _store = store;

    public IReadOnlyList<Department> GetAll()
    {
        lock (_store.SyncRoot)
            return _store.Departments.Select(Clone).ToList();
    }

    public Department? GetById(int id)
    {
        lock (_store.SyncRoot)
        {
            var department = _store.Departments.FirstOrDefault(d => d.DeptId == id);
            return department is null ? null : Clone(department);
        }
    }

    public Department Add(Department department)
    {
        lock (_store.SyncRoot)
        {
            var created = Clone(department);
            created.DeptId = _store.NextDepartmentId++;
            _store.Departments.Add(created);
            return Clone(created);
        }
    }

    public Department? Update(int id, Department department)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Departments.FirstOrDefault(d => d.DeptId == id);
            if (existing is null)
                return null;
            existing.DeptName = department.DeptName;
            return Clone(existing);
        }
    }

    public bool Delete(int id)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Departments.FirstOrDefault(d => d.DeptId == id);
            if (existing is null)
                return false;
            if (_store.Employees.Any(e => e.DeptId == id))
                throw new DepartmentInUseException();
            _store.Departments.Remove(existing);
            return true;
        }
    }

    private static Department Clone(Department department) => new()
    {
        DeptId = department.DeptId,
        DeptName = department.DeptName
    };
}
