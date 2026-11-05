using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public interface IDesignationService
{
    IReadOnlyList<Designation> GetAll();
    Designation? GetById(int id);
    Designation Add(Designation designation);
    Designation? Update(int id, Designation designation);
    bool Delete(int id);
}
