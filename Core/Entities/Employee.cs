namespace EmployeeSalaryProcessor.Core.Entities;
public class Employee
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string FullName => $"{Name} {Surname}";
}