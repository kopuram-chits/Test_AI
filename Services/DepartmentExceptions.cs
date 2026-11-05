namespace EmployeeAPI.Services;

public sealed class InvalidDepartmentException : Exception
{
    public InvalidDepartmentException() : base("Department does not exist.") { }
}

public sealed class DepartmentInUseException : Exception
{
    public DepartmentInUseException() : base("Department is assigned to employees and cannot be deleted.") { }
}
