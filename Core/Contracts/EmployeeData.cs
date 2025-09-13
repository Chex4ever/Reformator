
using EmployeeSalaryProcessor.Core.Entities;

namespace EmployeeSalaryProcessor.Core.Contracts;

public class EmployeeData
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public List<MonthSalary> Salaries { get; set; } = [];
}