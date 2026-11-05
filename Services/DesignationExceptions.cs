namespace EmployeeAPI.Services;

public sealed class InvalidDesignationException : Exception
{
    public InvalidDesignationException() : base("Designation does not exist.") { }
}

public sealed class DesignationInUseException : Exception
{
    public DesignationInUseException() : base("Designation is assigned to employees and cannot be deleted.") { }
}
