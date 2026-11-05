using EmployeeAPI.Models;

namespace EmployeeAPI.Services;

public class DesignationService : IDesignationService
{
    private readonly EmployeeDataStore _store;

    public DesignationService(EmployeeDataStore store) => _store = store;

    public IReadOnlyList<Designation> GetAll()
    {
        lock (_store.SyncRoot)
            return _store.Designations.Select(Clone).ToList();
    }

    public Designation? GetById(int id)
    {
        lock (_store.SyncRoot)
        {
            var designation = _store.Designations.FirstOrDefault(d => d.DesignationId == id);
            return designation is null ? null : Clone(designation);
        }
    }

    public Designation Add(Designation designation)
    {
        lock (_store.SyncRoot)
        {
            var created = Clone(designation);
            created.DesignationId = _store.NextDesignationId++;
            _store.Designations.Add(created);
            return Clone(created);
        }
    }

    public Designation? Update(int id, Designation designation)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Designations.FirstOrDefault(d => d.DesignationId == id);
            if (existing is null)
                return null;
            existing.DesignationName = designation.DesignationName;
            return Clone(existing);
        }
    }

    public bool Delete(int id)
    {
        lock (_store.SyncRoot)
        {
            var existing = _store.Designations.FirstOrDefault(d => d.DesignationId == id);
            if (existing is null)
                return false;
            if (_store.Employees.Any(e => e.DesignationId == id))
                throw new DesignationInUseException();
            _store.Designations.Remove(existing);
            return true;
        }
    }

    private static Designation Clone(Designation designation) => new()
    {
        DesignationId = designation.DesignationId,
        DesignationName = designation.DesignationName
    };
}
