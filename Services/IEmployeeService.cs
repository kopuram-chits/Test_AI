using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public interface IEmployeeService
{
    IReadOnlyList<Employee> GetAll();
    Employee? GetById(int id);
    Employee Add(Employee employee);
    Employee? Update(int id, Employee employee);
    bool Delete(int id);
}
