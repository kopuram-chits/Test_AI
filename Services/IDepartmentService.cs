using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public interface IDepartmentService
{
    IReadOnlyList<Department> GetAll();
    Department? GetById(int id);
    Department Add(Department department);
    Department? Update(int id, Department department);
    bool Delete(int id);
}
